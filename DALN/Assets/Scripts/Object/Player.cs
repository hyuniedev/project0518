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
using UnityEngine.EventSystems;

namespace Object
{
    public class Player : NetworkBehaviour
    {
        private Team _selectedTeam;

        public Team SelectedTeam
        {
            get => _selectedTeam;
            set
            {
                _selectedTeam = value;
                _virtualCamera.Follow = _selectedTeam.GetTransformFirstSoldier();
                _virtualCamera.LookAt = _selectedTeam.GetTransformFirstSoldier();
            }
        }

        public List<Team> Teams { get; } = new List<Team>();
        private List<Soldier> _freeSoldier = new List<Soldier>();
        private Camera _camera;
        private CinemachineCamera _virtualCamera;
        [SerializeField] private GameObject soldierPrefab;
        private AngelStatue angelStatue; 

        private GameUI _gameUI;
        
        private void Awake()
        {
            _camera = Camera.main;
            _virtualCamera = FindFirstObjectByType<CinemachineCamera>();
            angelStatue = FindFirstObjectByType<AngelStatue>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsOwner) return;
            _gameUI = FindFirstObjectByType<GameUI>();
            _gameUI.Player = this;
            CreateNewTeam();
            
            ActionEvent.OnMove += SelectedTeamMove;
        }
        
        private void OnDisable()
        { 
            if(!IsOwner) return;
            ActionEvent.OnMove -= SelectedTeamMove;
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
            if(SelectedTeam==null)
                SelectedTeam = newTeam;
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
                if (CheckTargetUI()) return;
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
                if (CheckTargetUI()) return;
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (SelectedTeam != null)
                    {
                        if (hit.transform.CompareTag("AngelStatue"))
                        {
                            if (angelStatue.WaitPray.Value) return;
                            angelStatue.SetWaitPrayServerRpc(true);
                            SelectedTeam.TeamMoveTo(angelStatue.transform.position, true);
                            return;
                        }
                        else
                        {
                            var colliders = Physics.OverlapSphere(hit.point, 1f);
                            foreach (var collider in colliders)
                            {
                                if (collider.transform.TryGetComponent<Soldier>(out var soldier))
                                {
                                    if (soldier.TeamId.Value == PlayerData.Instance.TeamId) return;
                                    SelectedTeam.SetOpponentTeam(soldier.transform.GetComponent<NetworkObject>()
                                        .NetworkObjectId);
                                    return;
                                }
                            }
                        }
                        ActionEvent.OnMove?.Invoke(hit.point);
                        if(angelStatue != null && angelStatue.WaitPray.Value)
                            angelStatue.SetWaitPrayServerRpc(false);
                    }
                }
            }
        }

        private void SelectedTeamMove(Vector3 position) => SelectedTeam.TeamMoveTo(position);

        // private void LateUpdate()
        // {
        //     if (!IsOwner || SelectedTeam == null || SelectedTeam.GetNumSoldiers() <= 0) return;
        //     _virtualCamera.Follow = SelectedTeam.GetTransformFirstSoldier();
        //     _virtualCamera.LookAt = SelectedTeam.GetTransformFirstSoldier();
        // }

        private bool CheckTargetUI()
        {
            if(EventSystem.current.IsPointerOverGameObject()) return true;
            if (_gameUI.IsOpenPanel)
                _gameUI.UpdateStateListTeamPanel();
            return false;
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