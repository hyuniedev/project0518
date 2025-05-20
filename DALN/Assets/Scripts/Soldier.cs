using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

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

    private NetworkVariable<ulong> _opponentId = new NetworkVariable<ulong>(0,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    #endregion
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    // public override void OnNetworkSpawn()
    // {
    //     base.OnNetworkSpawn();
    //     if (IsServer)
    //     {
    //         _agent.enabled = true;
    //     }
    //     else
    //     {
    //         _agent.enabled = false;
    //     }
    // }

    private void Update()
    {
        if (!IsOwner) return;
        Debug.Log(_curState.Value);
        if (_soldierData.Value.Health <= 0)
        {
            ChangeStateServerRpc(ESoldierState.Death);
            OnDeath?.Invoke(this);
        }
        else if (CheckMoving())
        {
            ChangeStateServerRpc(ESoldierState.Move);
        }else if (_opponentId.Value != 0)
        {
            ChangeStateServerRpc(ESoldierState.Attack);
        }
        else
        {
            ChangeStateServerRpc(ESoldierState.Idle);
        }
    }

    public void SetOpponent(Soldier opponent)
    {
        if (!IsServer) return;
        _opponentId.Value = opponent.NetworkObjectId;
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
    
    
    private bool CheckMoving()
    {
        return _agent.remainingDistance > _agent.stoppingDistance;
    }
    
    [ServerRpc]
    public void MoveToServerRpc(Vector3 destination)
    {
        if(!IsServer) return;
        _agent.SetDestination(destination);
    }

    [ServerRpc]
    private void ChangeStateServerRpc(ESoldierState newState)
    {
        if (_curState.Value == newState) return;
        _curState.Value = newState;
    }
}