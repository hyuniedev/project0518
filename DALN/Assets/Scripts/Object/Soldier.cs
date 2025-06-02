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
            {
                newState = ESoldierState.Death;
            }
            else if (CheckMoving())
            {
                newState = ESoldierState.Move;
            }
            else if (_opponentId.Value != 0)
            {
                newState = ESoldierState.Attack;
            }
            else
            {
                newState = ESoldierState.Idle;
            }

            if (_curState.Value == newState) return;
            ChangeStateServerRpc(newState);
            if (_soldierData.Value.Health <= 0) OnDeath?.Invoke(this);
        }

        [ServerRpc]
        public void SetOpponentServerRpc(ulong opponentId)
        {
            if (!IsServer) return;
            _opponentId.Value = opponentId;
        }

        public Soldier GetOpponent()
        {
            if (_opponentId.Value == 0) return null;
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(_opponentId.Value, out var netObj))
            {
                return netObj.GetComponent<Soldier>();
            }

            return null;
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

        [ServerRpc(RequireOwnership = false)]
        private void ChangeStateServerRpc(ESoldierState newState)
        {
            _curState.Value = newState;
            ChangeStateClientRpc(newState.ToString());
        }

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