using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
public class Player : NetworkBehaviour
{
    private Team _selectedTeam = new Team();
    private List<Team> _teams = new List<Team>();
    private List<Soldier> _freeSoldier = new List<Soldier>();

    private void Update()
    {
        if(!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RequestSpawnSoldierServerRpc();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            var newTeam = new Team();
            foreach (var soldier in _freeSoldier)
            {
                newTeam.AddSoldier(soldier);
            }
            _teams.Add(newTeam);
        }
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Soldier"))
                {
                    var soldier = hit.collider.GetComponent<Soldier>();
                    foreach (var team in _teams)
                    {
                        if (team.ContainsSoldier(soldier))
                        {
                            _selectedTeam = team;
                            break;
                        }
                    }
                }
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                _selectedTeam.TeamMoveTo(hit.point);
            }
        }
    }
    
    [ServerRpc]
    private void RequestSpawnSoldierServerRpc(ServerRpcParams rpcParams = default)
    {
        var soldier = SoldierObjectPool.Singleton.Dequeue(rpcParams.Receive.SenderClientId);
        AddSoldierToFreeListClientRpc(soldier);
    }

    [ClientRpc]
    private void AddSoldierToFreeListClientRpc(ulong id)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out NetworkObject noj))
        {
            _freeSoldier.Add(noj.GetComponent<Soldier>());
        }
    }
}