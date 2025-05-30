using System;
using System.Collections.Generic;
using Controller;
using Data_Manager;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace Object
{
    public class Player : NetworkBehaviour
    {
        public int TeamId { get; set; } = 1;
        private Team _selectedTeam = null;
        private List<Team> _teams = new List<Team>();
        private List<Soldier> _freeSoldier = new List<Soldier>();
        [SerializeField] private Camera _camera;
        [SerializeField] private CinemachineCamera _virtualCamera;

        private void Awake()
        {
            _camera = Camera.main;
            _virtualCamera = FindFirstObjectByType<CinemachineCamera>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsOwner) return;
            for (int i = 0; i < GameData.InitCountSoldierPerPlayer; i++)
            {
                RequestSpawnSoldierServerRpc(TeamId);
            }

            ActionEvent.OnGroupFreeSoldiers += GroupFreeSoldiers;
        }

        private void Update()
        {
            if (!IsOwner) return;
            TargetMouse();
            MoveMouse();
            TargetTeamByKeyboard();
        }

        private void GroupFreeSoldiers()
        {
            if (!IsOwner) return;
            var newTeam = new Team();
            foreach (var soldier in _freeSoldier)
            {
                newTeam.AddSoldier(soldier);
            }

            newTeam.OnAllSoldiersOnTeamDeath += RemoveTeam;
            _teams.Add(newTeam);
            _freeSoldier.Clear();
        }

        private void TargetTeamByKeyboard()
        {
            for (int i = 0; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    if (i - 1 < _teams.Count)
                        _selectedTeam = _teams[i - 1];
                }
            }
        }

        private void TargetMouse()
        {
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
                    else
                    {
                        _selectedTeam = null;
                    }
                }
            }
        }

        private void MoveMouse()
        {
            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (_selectedTeam != null)
                    {
                        _selectedTeam.TeamMoveTo(hit.point);
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (!IsOwner || _selectedTeam == null) return;
            _virtualCamera.Follow = _selectedTeam.GetTransformFirstSoldier();
            _virtualCamera.LookAt = _selectedTeam.GetTransformFirstSoldier();
        }

        private void RemoveTeam(Team team)
        {
            _teams.Remove(team);
        }

        [ServerRpc]
        private void RequestSpawnSoldierServerRpc(int teamId, ServerRpcParams rpcParams = default)
        {
            var soldier = SoldierObjectPool.Singleton.Dequeue(rpcParams.Receive.SenderClientId, teamId);
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
}