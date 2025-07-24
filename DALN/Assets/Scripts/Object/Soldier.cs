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

        public NetworkVariable<SoldierData> SoldierData { get; private set; }

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
        private GameObject _target;
        
        [SerializeField]
        private AudioSource audioSource;
        
        [SerializeField] private Transform gunBarrelPosition;
        [SerializeField] private MouseController mouseController;
        
        private float _nextTimeShoot;
        private bool _settedDisableComponnnents = false;
        private bool _isPray;
        private float _timePray;

        #endregion

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = transform.GetChild(0).GetComponent<Animator>();
            _outline = transform.GetChild(0).GetComponent<Outline>();
            _outline.enabled = false;

            if (GameData.Instance != null)
            {
                SoldierData = new NetworkVariable<SoldierData>(
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
            if (IsClient)
            {
                if (TeamId.Value != _teamIdLocal)
                {
                    _teamIdLocal = TeamId.Value;
                    UpdateTexture();
                }

                audioSource.enabled = _curState.Value == ESoldierState.Move;
            }
            if (IsServer)
            {
                StateAnimUpdate();
                if (IsDeath)
                {
                    RemoveSoldierClientRpc();
                    if(!_settedDisableComponnnents)
                        SetDisableComponents();
                    return;
                }
                Pray();
                if (_opponentId.Value==0) return;
                if (_target.GetComponent<Soldier>().IsDeath || Vector3.Distance(_target.transform.position, transform.position) > 10f)
                    FindOpponentUpdate();
                LookToOpponent();
                if (_nextTimeShoot < Time.time && _target != null && !_target.GetComponent<Soldier>().IsDeath)
                {
                    _nextTimeShoot = Time.time + 0.3f;
                    AttackOpponent();
                }
            }
        }

        private void SetDisableComponents()
        {
            _settedDisableComponnnents = true;
            transform.GetComponent<CapsuleCollider>().enabled = false;
            transform.GetComponent<Soldier>().enabled = false;
            _agent.enabled = false; 
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
            if (IsDeath)
            {
                newState = ESoldierState.Death;
                StopPray();
            }
            else if (CheckMoving())
            {
                newState = ESoldierState.Move;
            }
            else if (_opponentId.Value != 0)
            {
                newState = ESoldierState.Attack;
                StopPray();                
            }
            else if(_isPray)
                newState = ESoldierState.Pray;
            else
                newState = ESoldierState.Idle;

            if (_curState.Value != newState)
            {
                _curState.Value = newState;
                ChangeStateClientRpc(newState.ToString());                
            }
        }

        private void StopPray()
        {
            if (_isPray)
            {
                Debug.Log("Stop Pray");
                _isPray = false;
                _timePray = 0;
                FindFirstObjectByType<AngelStatue>().SetWaitPrayServerRpc(false);
            }
        }

        private void Pray()
        {
            if (!_isPray || CheckMoving()) return;
            if (_timePray < 5f)
            {
                _timePray += Time.deltaTime;
            }
            else
            {
                var angelStatue = FindFirstObjectByType<AngelStatue>();
                angelStatue.RequestSetTeamIdTarget(TeamId.Value);
                StopPray();
            }
        }
        
        private void UpdateTexture()
        {
            if(TeamId.Value <=0) return;
            transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material.mainTexture =
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
            var nearObject = CheckOpponent();
            if (nearObject != null && nearObject.GetComponent<NetworkObject>().NetworkObjectId != _opponentId.Value)
                RequireSetOpponentToTeamClientRpc(nearObject.GetComponent<NetworkObject>().NetworkObjectId);
            else if (nearObject == null)
                RequireSetOpponentToTeamClientRpc(0);
        }

        private void LookToOpponent()
        {
            if (_target!=null)
            {
                var direction = _target.transform.position - transform.position;
                direction.Normalize();
                var targetAngle = Quaternion.LookRotation(direction);
                targetAngle *= Quaternion.Euler(0f, -55f, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle,Time.deltaTime * 10f);
            }
        }

        private void AttackOpponent()
        {
            var direction = _target.transform.position - transform.position;
            direction.Normalize();
            BulletObjectPool.Instance.Dequeue(TeamId.Value, gunBarrelPosition, direction, SoldierData.Value.Damage);
        }
        
        private GameObject CheckOpponent()
        {
            int layer = 0;
            for(int i = 1; i <= 3 ; i++)
                if(i!=TeamId.Value)
                    layer |= 1 << LayerMask.NameToLayer($"Soldier{i}");
            var colliders = Physics.OverlapBox(transform.position + new Vector3(0, 0, 15f), new Vector3(22,18,15),Quaternion.identity, layer);
            if (colliders.Length > 0)
            {
                foreach(var opponent in colliders)
                {
                    if (opponent.GetComponent<Soldier>().TeamId.Value != TeamId.Value)
                    {
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
            if (opponentId == 0)
            {
                _opponentId.Value = 0;
                _target = null;
            }
            else if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(opponentId, out var objNetwork))
            {
                var direction = objNetwork.transform.position - transform.position;
                var layer = 1 << LayerMask.NameToLayer("House");
                if (!Physics.Raycast(transform.position + Vector3.up, direction , Vector3.Distance(transform.position,objNetwork.transform.position),layer))
                {
                    _opponentId.Value = opponentId;
                    _target = objNetwork.gameObject;    
                    Debug.DrawRay(transform.position + Vector3.up, direction * Vector3.Distance(transform.position,_target.transform.position), Color.red, 1f);
                }
            }
        }

        #endregion
        
        #region Move

        private bool CheckMoving() => _agent.remainingDistance > _agent.stoppingDistance;


        public void RequestMoveTo(Vector3 position, bool isPray)
        {
            SetOpponentServerRpc(0);
            MoveToServerRpc(position, isPray);
        }

        [ServerRpc]
        private void MoveToServerRpc(Vector3 destination, bool isPray)
        {
            _agent.SetDestination(destination);
            _isPray = isPray;
            _timePray = 0;
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
            if (IsOwner)
                OnMouseTarget?.Invoke(true);
            else
                VisibleOutline(true);
        }

        private void OnMouseExit()
        {
            if (IsOwner)
                OnMouseTarget?.Invoke(false);
            else
                VisibleOutline(false);
        }

        public void VisibleOutline(bool visible) => _outline.enabled = visible;

        #endregion

        public bool IsDeath => SoldierData.Value.Health <= 0;

        [ServerRpc]
        public void UpSoldierDataServerRpc(int damage = 0, int armor = 0) => SoldierData.Value = new SoldierData(SoldierData.Value.Health,
            SoldierData.Value.Damage + damage, SoldierData.Value.Armor + armor);
        
        public void GetDamage(int damage)
        {
            var health = this.SoldierData.Value.Health;
            health -= (damage - SoldierData.Value.Armor) > 0 ? damage - SoldierData.Value.Armor : 1;
            SoldierData.Value = new SoldierData(health, SoldierData.Value.Damage, SoldierData.Value.Armor);
        }
    }
}