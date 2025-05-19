using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public struct SoldierData
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
}
public class Soldier : NetworkBehaviour
{
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
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!IsServer) return;
        
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
    public void MoveServerRpc(Vector3 destination)
    {
        if (IsServer)
        {
            _agent.SetDestination(destination);
        }
    }

    [ServerRpc]
    private void ChangeStateServerRpc(ESoldierState newState)
    {
        if (_curState.Value == newState) return;
        _curState.Value = newState;
    }
}