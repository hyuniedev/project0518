using System;
using Data_Manager;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace Object
{
    public struct SoldierData : INetworkSerializable
    {
        public int Health;
        public int Damage;
        public int Armor;
        
        public SoldierData(int health, int damage, int armor)
        {
            Health = health;
            Damage = damage;
            Armor = armor;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Health);
            serializer.SerializeValue(ref Damage);
            serializer.SerializeValue(ref Armor);
        }
    }

    public class Soldier : NetworkBehaviour
    {
        #region Define variable

        public Action<Soldier> OnDeath;
        private NavMeshAgent _agent;

        private NetworkVariable<ESoldierState> _curState = new NetworkVariable<ESoldierState>(ESoldierState.Idle,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<SoldierData> _soldierData = new NetworkVariable<SoldierData>(
            new SoldierData(GameData.InitHeath, GameData.InitDamage, GameData.InitArmor),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private NetworkVariable<ulong> _opponentId = new NetworkVariable<ulong>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private Animator _animator;
        private Outline _outline;
        public Action<bool> OnMouseTarget;
        public Action<ulong> OnTargetOpponent;
        private Collider[] _colliders = new Collider[3];

        #endregion

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                _agent.enabled = true;
            }
            else
            {
                _agent.enabled = false;
            }
        }

        private void Update()
        {
            if (!IsServer) return;

            ESoldierState newState;
            if (_soldierData.Value.Health <= 0)
                newState = ESoldierState.Death;
            else if (CheckMoving())
                newState = ESoldierState.Move;
            else if (_opponentId.Value != 0)
                newState = ESoldierState.Attack;
            else
                newState = ESoldierState.Idle;

            if (_curState.Value != newState)
            {
                _curState.Value = newState;
                ChangeStateClientRpc(newState.ToString());                
            }
            if (_soldierData.Value.Health <= 0) OnDeath?.Invoke(this);
            
            var opponent = CheckOpponent();
            if (opponent != null && opponent.GetComponent<NetworkObject>().NetworkObjectId != _opponentId.Value)
            {
                RequireSetOpponentToTeamClientRpc(opponent.GetComponent<NetworkObject>().NetworkObjectId);
                Debug.Log($"Soldier id: {GetComponent<NetworkObject>().NetworkObjectId} set opponent id: {opponent.GetComponent<NetworkObject>().NetworkObjectId}");
            }else if (opponent == null)
            {
                RequireSetOpponentToTeamClientRpc(0);
            }
        }

        private GameObject CheckOpponent()
        {
            int layer = 0;
            for(int i = 1; i<=3; i++)
                if(i!=PlayerData.Instance.TeamId)
                    layer |= 1 << LayerMask.NameToLayer($"Soldier{i}");
            var hitCount = Physics.OverlapBoxNonAlloc(transform.position, Vector3.one * 10f, _colliders ,Quaternion.identity, layer);
            return hitCount > 0 ? _colliders[0].gameObject : null;
        }

        [ClientRpc]
        private void RequireSetOpponentToTeamClientRpc(ulong opponentId)
        {
            OnTargetOpponent?.Invoke(opponentId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetOpponentServerRpc(ulong opponentId)
        {
            _opponentId.Value = opponentId;
        }

        #region Move

        private bool CheckMoving()
        {
            return _agent.remainingDistance > _agent.stoppingDistance;
        }

        // public void SetStoppingDistance(float stoppingDistance)
        // {
        //     if(!IsServer) return;
        //     _agent.stoppingDistance = stoppingDistance;
        // }

        public void RequestMoveTo(Vector3 position)
        {
            MoveToServerRpc(position);
        }

        [ServerRpc]
        private void MoveToServerRpc(Vector3 destination)
        {
            if (!IsServer) return;
            _agent.SetDestination(destination);
        }

        #endregion

        #region Animation State
        
        [ClientRpc]
        private void ChangeStateClientRpc(String newState)
        {
            _animator.CrossFade(newState, 0.01f);
        }

        #endregion

        #region Ouline

        private void OnMouseEnter()
        {
            OnMouseTarget?.Invoke(true);
        }

        private void OnMouseExit()
        {
            OnMouseTarget?.Invoke(false);
        }

        public void VisibleOutline(bool visible)
        {
            _outline.enabled = visible;
        }

        #endregion

    }
}