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
        private float _nextTimeCheckOpponent;
        public int TeamId{get;set;}
        private GameObject _opponent;
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
            if (IsServer)
            {
                StateAnimUpdate();
                if (_nextTimeCheckOpponent < Time.time)
                {
                    _nextTimeCheckOpponent = Time.time + 0.5f;
                    FindOpponentUpdate();
                }
                LookToOpponent();
            }
        }

        private void StateAnimUpdate()
        {
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
        }

        #region Check Opponent

        private void FindOpponentUpdate()
        {
            var opponent = CheckOpponent();
            if (opponent != null && opponent.GetComponent<NetworkObject>().NetworkObjectId != _opponentId.Value)
                RequireSetOpponentToTeamClientRpc(opponent.GetComponent<NetworkObject>().NetworkObjectId);
            else if (opponent == null)
                RequireSetOpponentToTeamClientRpc(0);
            _opponent = opponent;   
        }

        private void LookToOpponent()
        {
            if (_opponent!=null)
            {
                var direction = _opponent.transform.position - transform.position;
                direction.Normalize();
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction),Time.deltaTime * 10f);
            }
        }
        
        private GameObject CheckOpponent()
        {
            int layer = 1<< LayerMask.NameToLayer("Soldier");
            var _colliders = Physics.OverlapBox(transform.position, Vector3.one * 10f ,Quaternion.identity, layer);
            if (_colliders.Length > 0)
            {
                foreach(var opponent in _colliders)
                {
                    if(opponent.GetComponent<Soldier>().TeamId!=TeamId)
                        return opponent.gameObject;
                }
            }
            return null;
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

        #endregion
        
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
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            // Vẽ khung vùng box mà bạn dùng cho OverlapBox
            Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 10f * 2f);
        }

    }
}