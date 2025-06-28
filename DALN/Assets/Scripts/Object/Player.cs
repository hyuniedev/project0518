using System;
using System.Collections;
using System.Collections.Generic;
using Controller;
using Data_Manager;
using DesignPattern;
using UI;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

namespace Object
{
    public class Player : NetworkBehaviour
    {
        public Team SelectedTeam { get; set; } = null;
        public List<Team> Teams { get; } = new List<Team>();
        private List<Soldier> _freeSoldier = new List<Soldier>();
        private Camera _camera;
        private CinemachineCamera _virtualCamera;
        [SerializeField] private GameObject soldierPrefab;

        private void Awake()
        {
            _camera = Camera.main;
            _virtualCamera = FindFirstObjectByType<CinemachineCamera>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsOwner) return;
            FindFirstObjectByType<GameUI>().Player = this;
            CreateNewTeam();
        }

        private void Update()
        {
            if (!IsOwner) return;
            if (SelectedTeam != null && SelectedTeam.GetNumSoldiers() == 0)
            {
                Teams.Remove(SelectedTeam);
                SelectedTeam = null;
            }
            TargetMouse();
            MoveOrTargetOpponentMouse();
            TargetTeamByKeyboard();
        }

        public void CreateNewTeam()
        {
            if (!IsOwner) return;
            for (int i = 0; i < GameData.Instance.gameData.initCountSoldierPerTeam; i++)
                RequestSpawnSoldierServerRpc(PlayerData.Instance.TeamId);
            StartCoroutine(WaitForSpawnSoldierAndCreateTeam());
        }

        private IEnumerator WaitForSpawnSoldierAndCreateTeam()
        {
            while (_freeSoldier.Count < GameData.Instance.gameData.initCountSoldierPerTeam)
            {
                yield return null;
            }
            var newTeam = new Team();
            foreach (var soldier in _freeSoldier)
            {
                newTeam.AddSoldier(soldier);
            }
            newTeam.OnAllSoldiersOnTeamDeath += RemoveTeam;
            Teams.Add(newTeam);
            _freeSoldier.Clear();
        }

        private void TargetTeamByKeyboard()
        {
            for (int i = 0; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    if (i - 1 < Teams.Count)
                        SelectedTeam = Teams[i - 1];
                }
            }
        }

        private void TargetMouse()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.CompareTag("Soldier"))
                    {
                        var soldier = hit.collider.GetComponent<Soldier>();
                        foreach (var team in Teams)
                        {
                            if (team.ContainsSoldier(soldier))
                            {
                                SelectedTeam = team;
                                break;
                            }
                        }
                    }
                    else
                    {
                        SelectedTeam = null;
                    }
                }
            }
        }

        private void MoveOrTargetOpponentMouse()
        {
            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (SelectedTeam != null)
                    {
                        if (hit.transform.TryGetComponent<Soldier>(out var soldier))
                        {
                            if (soldier.TeamId.Value == PlayerData.Instance.TeamId) return;
                            SelectedTeam.SetOpponentTeam(soldier.transform.GetComponent<NetworkObject>().NetworkObjectId);
                        }
                        else
                            SelectedTeam.TeamMoveTo(hit.point);
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (!IsOwner || SelectedTeam == null || SelectedTeam.GetNumSoldiers() <= 0) return;
            _virtualCamera.Follow = SelectedTeam.GetTransformFirstSoldier();
            _virtualCamera.LookAt = SelectedTeam.GetTransformFirstSoldier();
        }

        private void RemoveTeam(Team team)
        {
            team.OnAllSoldiersOnTeamDeath -= RemoveTeam;
            Teams.Remove(team);
        }

        [ServerRpc]
        private void RequestSpawnSoldierServerRpc(int teamId, ServerRpcParams rpcParams = default)
        {
            var go = Instantiate(soldierPrefab, GameData.Instance.gameData.TeamInitialPosition[teamId], Quaternion.identity);
            Soldier soldier = go.GetComponent<Soldier>();
            soldier.GetComponent<NetworkObject>().SpawnWithOwnership(rpcParams.Receive.SenderClientId);
            soldier.TeamId.Value = teamId;
            soldier.gameObject.layer = LayerMask.NameToLayer($"Soldier{teamId}");
            AddSoldierToFreeListClientRpc(soldier.GetComponent<NetworkObject>().NetworkObjectId);
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