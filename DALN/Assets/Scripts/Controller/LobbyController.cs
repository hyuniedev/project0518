using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data_Manager;
using DesignPattern;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Controller
{
    public class LobbyController : Singleton<LobbyController>
    {
        public Lobby CurrentLobby { get; private set; }
        private Coroutine _heartbeatLobby;
        private Coroutine _pollLobby;

        public async Task<Lobby> CreateLobby(string lobbyName)
        {
            try
            {
                CreateLobbyOptions options = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Player = new Player(
                        id: AuthenticationService.Instance.PlayerId, 
                        data: new Dictionary<string, PlayerDataObject>
                        {
                            {"Name",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PlayerData.Instance.Name)},
                            {"Rank",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PlayerData.Instance.Rank.ToString())}
                        })
                };
                Lobby lobby =
                    await LobbyService.Instance.CreateLobbyAsync(lobbyName: lobbyName, GameData.Instance.gameData.maxPlayersPerLobby,
                        options);
                
                var allocation = await RelayService.Instance.CreateAllocationAsync(GameData.Instance.gameData.maxPlayersPerLobby);
                var relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                
                await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject> { { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, relayCode) } }
                });
                
                var rsd = allocation.ToRelayServerData("dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(rsd);
                CurrentLobby = lobby;
                _heartbeatLobby = StartCoroutine(HeartbeatLobby());
                _pollLobby = StartCoroutine(PollLobby());
                NetworkManager.Singleton.StartHost();
                // Setup PlayerData
                PlayerData.Instance.TeamId = 1;
                
                return lobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogException(e);
            }
            return null;
        }

        public async Task JoinLobby(string lobbyId)
        {
            try
            {
                JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
                {
                    Player = new Player(
                        id: AuthenticationService.Instance.PlayerId,
                        data: new Dictionary<string, PlayerDataObject>
                        {
                            {"Name",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PlayerData.Instance.Name)},
                            {"Rank",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PlayerData.Instance.Rank.ToString())}
                        })
                };
                Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
                
                if (!lobby.Data.TryGetValue("RelayCode",out var code)) return;
                var relayCode = lobby.Data["RelayCode"].Value;

                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayCode);
                
                var rds = joinAllocation.ToRelayServerData("dtls");
                CurrentLobby = lobby;
                _pollLobby = StartCoroutine(PollLobby());
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(rds);
                NetworkManager.Singleton.StartClient();
                
                // Setup PlayerData
                PlayerData.Instance.TeamId = lobby.Players.Count;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogException(e);
            }
        }

        public async Task LeaveLobby(string playerId)
        {
            try
            {
                if(CurrentLobby == null) return;
                if (CurrentLobby.HostId == playerId)
                {
                    StopCoroutine(_heartbeatLobby);
                    StopCoroutine(_pollLobby);
                    await LobbyService.Instance.DeleteLobbyAsync(CurrentLobby.Id);
                }
                else
                {
                    StopCoroutine(_pollLobby);
                    await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerId);
                }
                
                CurrentLobby = null;
                NetworkManager.Singleton.Shutdown();
                
                // Setup PlayerData
                PlayerData.Instance.TeamId = 0;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogException(e);
            }
        }
        
        public void StartGame()
        {
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }
        
        private IEnumerator HeartbeatLobby()
        {
            while (true)
            {
                var taskWaitSendHeartbeat = WaitSendHeartbeatAsync();
                yield return new WaitUntil(() => taskWaitSendHeartbeat.IsCompleted);
                yield return new WaitForSeconds(15f);
            }
        }

        private async Task WaitSendHeartbeatAsync()
        {
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogException(e);
            }
        }

        private IEnumerator PollLobby()
        {
            while (true)
            {
                if(CurrentLobby == null) yield break;
                var taskGetLobby = WaitGetLobbyAsync();
                yield return new WaitUntil(()=>taskGetLobby.IsCompleted);
                if (taskGetLobby.IsFaulted || CurrentLobby==null || !CheckInRoom())
                {
                    NetworkManager.Singleton.Shutdown();
                    UIController.Instance.ToSceneHome();
                    
                    StopCoroutine(_pollLobby);
                    yield break;
                }
                yield return new WaitForSeconds(1f);
            }
        }

        private async Task WaitGetLobbyAsync()
        {
            try
            {
                CurrentLobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
            }
            catch (LobbyServiceException e)
            {
                CurrentLobby = null;
                Debug.Log("Host đã out phòng");
            }
        }
        
        public async Task<List<Lobby>> FetchLobbies()
        {
            var task = await LobbyService.Instance.QueryLobbiesAsync(new QueryLobbiesOptions());
            return task.Results;
        }

        private bool CheckInRoom()
        {
            foreach (var player in CurrentLobby.Players)
                if (player.Id == AuthenticationService.Instance.PlayerId)
                    return true;
            return false;
        }
    }
}
