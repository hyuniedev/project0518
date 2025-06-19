using System;
using Controller;
using Data_Manager;
using DesignPattern;
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

    public class Soldier : NetworkBehaviour, IGetDamage
    {
        #region Define variable

        public Action<Soldier> OnDeath;
        private NavMeshAgent _agent;

        private NetworkVariable<ESoldierState> _curState = new NetworkVariable<ESoldierState>(ESoldierState.Idle,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkVariable<SoldierData> _soldierData;

        private NetworkVariable<ulong> _opponentId = new NetworkVariable<ulong>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public NetworkVariable<int> TeamId { get; set; } = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private int _teamIdLocal = 0;
        private Animator _animator;
        private Outline _outline;
        public Action<bool> OnMouseTarget;
        public Action<ulong> OnTargetOpponent;
        private float _nextTimeCheckOpponent;
        private GameObject _opponent;

        [SerializeField] private Transform gunBarrelPosition;
        private float _nextTimeShoot;
        private bool _settedDisableComponnnents = false;

        #endregion

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _outline = GetComponent<Outline>();
            _outline.enabled = false;

            if (GameData.Instance != null)
            {
                _soldierData = new NetworkVariable<SoldierData>(
                    new SoldierData(GameData.Instance.gameData.initHealth, GameData.Instance.gameData.initDamage,
                        GameData.Instance.gameData.initArmor),
                    NetworkVariableReadPermission.Everyone,
                    NetworkVariableWritePermission.Server
                );
            }
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
                if (_soldierData.Value.Health <= 0)
                {
                    RemoveSoldierClientRpc();
                    if(!_settedDisableComponnnents)
                        SetDisableComponents();
                    return;
                }

                LookToOpponent();
                FindOpponentUpdate();
                if (_opponent != null)
                {
                    AttackOpponent();
                }
            }

            if (IsClient)
            {
                if (TeamId.Value != _teamIdLocal)
                {
                    _teamIdLocal = TeamId.Value;
                    UpdateTexture();
                }
            }
        }

        private void SetDisableComponents()
        {
            _settedDisableComponnnents = true;
            transform.GetComponent<CapsuleCollider>().enabled = false;
            transform.GetComponent<Soldier>().enabled = false;
            SetDisableComponentsClientRpc();
        }

        [ClientRpc]
        private void SetDisableComponentsClientRpc()
        {
            transform.GetComponent<CapsuleCollider>().enabled = false;
            transform.GetComponent<Soldier>().enabled = false;
        }

        private void StateAnimUpdate()
        {
            ESoldierState newState;
            if (_soldierData.Value.Health <= 0)
            {
                newState = ESoldierState.Death;
            }
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
        }
        
        private void UpdateTexture()
        {
            if(TeamId.Value <=0) return;
            transform.GetChild(0).GetComponent<Renderer>().material.mainTexture =
                GameData.Instance.gameData.soliderTextures[TeamId.Value - 1];
        }

        [ClientRpc]
        private void RemoveSoldierClientRpc()
        {
            OnDeath?.Invoke(this);
        }
        #region Check Opponent

        private void FindOpponentUpdate()
        {
            _opponent = CheckOpponent();
            if (_opponent != null && _opponent.GetComponent<NetworkObject>().NetworkObjectId != _opponentId.Value)
                RequireSetOpponentToTeamClientRpc(_opponent.GetComponent<NetworkObject>().NetworkObjectId);
            else if (_opponent == null)
                RequireSetOpponentToTeamClientRpc(0);
        }

        private void LookToOpponent()
        {
            if (_opponent!=null)
            {
                var direction = _opponent.transform.position - transform.position;
                direction.Normalize();
                var targetAngle = Quaternion.LookRotation(direction);
                targetAngle *= Quaternion.Euler(0f, 50f, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle,Time.deltaTime * 10f);
            }
        }

        private void AttackOpponent()
        {
            var direction = _opponent.transform.position - transform.position;
            direction.Normalize();
            BulletObjectPool.Instance.Dequeue(TeamId.Value, gunBarrelPosition, direction);            
        }
        
        private GameObject CheckOpponent()
        {
            int layer = 0;
            for(int i = 1; i <= 3 ; i++)
                if(i!=TeamId.Value)
                    layer |= 1 << LayerMask.NameToLayer($"Soldier{i}");
            var _colliders = Physics.OverlapBox(transform.position, Vector3.one * 10f ,Quaternion.identity, layer);
            if (_colliders.Length > 0)
            {
                foreach(var opponent in _colliders)
                {
                    if (opponent.GetComponent<Soldier>().TeamId.Value != TeamId.Value)
                    {
                        var direction = opponent.transform.position - transform.position;
                        Debug.DrawRay(gunBarrelPosition.position, direction.normalized * 100f, Color.red, 1f);
                        if (Physics.Raycast(gunBarrelPosition.position, direction,out var hit))
                        {
                            var layerOpponent = LayerMask.LayerToName(hit.transform.gameObject.layer);
                            if (!layerOpponent.StartsWith("Soldier"))
                                return null;
                        }
                        return opponent.gameObject;
                    }
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

        public void GetDamage(int damage)
        {
            var health = this._soldierData.Value.Health;
            health -= damage;
            _soldierData.Value = new SoldierData(health, _soldierData.Value.Damage, _soldierData.Value.Armor);
        }
    }
}