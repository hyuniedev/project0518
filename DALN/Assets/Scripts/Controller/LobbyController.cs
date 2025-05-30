using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data_Manager;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Controller
{
    public class LobbyController : MonoBehaviour
    {
        #region Setup Singleton

        private static LobbyController _instance;

        public static LobbyController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("LobbyController").AddComponent<LobbyController>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            _instance = this;
            DontDestroyOnLoad(this);
        }

        #endregion
        
        private bool _initialized = false;
        public Lobby CurrentLobby { get; private set; }
        private Coroutine _heartbeatLobby;
        private Coroutine _pollLobby;
        
        private async void Start()
        {
            _initialized = false;
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            _initialized = true;
        }

        public async Task<Lobby> CreateLobby(string lobbyName)
        {
            if (!_initialized) return null;
            try
            {
                CreateLobbyOptions options = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Player = new Player(id: AuthenticationService.Instance.PlayerId)
                };
                Lobby lobby =
                    await LobbyService.Instance.CreateLobbyAsync(lobbyName: lobbyName, GameData.MaxPlayersPerLobby,
                        options);

                var allocation = await RelayService.Instance.CreateAllocationAsync(GameData.MaxPlayersPerLobby);
                var relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                
                await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject> { { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, relayCode) } }
                });

                var rsd = allocation.ToRelayServerData("dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(rsd);
                CurrentLobby = lobby;
                _heartbeatLobby = StartCoroutine(HeartbeatLobby(lobby.Id));
                NetworkManager.Singleton.StartHost();
                return lobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogException(e);
            }
            return null;
        }

        public async void JoinLobby(string lobbyId)
        {
            if (!_initialized) return;
            try
            {
                JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
                {
                    Player = new Player(id: AuthenticationService.Instance.PlayerId)
                };
                Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);

                if (!lobby.Data.ContainsKey("RelayCode")) return;
                var relayCode = lobby.Data["RelayCode"].Value;

                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayCode);

                var rds = joinAllocation.ToRelayServerData("dtls");
                CurrentLobby = lobby;
                _pollLobby = StartCoroutine(PollLobby());
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(rds);
                NetworkManager.Singleton.StartClient();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogException(e);
            }
        }

        public async void LeaveLobby()
        {
            try
            {
                if(CurrentLobby == null) return;
                if (CurrentLobby.HostId != AuthenticationService.Instance.PlayerId)
                {
                    StopCoroutine(_heartbeatLobby);
                    await LobbyService.Instance.DeleteLobbyAsync(CurrentLobby.Id);
                }
                else
                {
                    StopCoroutine(_pollLobby);
                    await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, AuthenticationService.Instance.PlayerId);
                }
                CurrentLobby = null;
                NetworkManager.Singleton.Shutdown();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogException(e);
            }
        }
        
        private IEnumerator HeartbeatLobby(string lobbyId)
        {
            while (true)
            {
                LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
                yield return new WaitForSeconds(15f);
            }
        }

        private IEnumerator PollLobby()
        {
            while (true)
            {
                if(CurrentLobby == null) yield break;
                try
                {
                    LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log($"Host leave lobby {e}");
                    NetworkManager.Singleton.Shutdown();
                    yield break;
                }
                yield return new WaitForSeconds(5f);
            }
        }

        public async Task<List<Lobby>> FetchLobbies()
        {
            if (!_initialized) return null;
            Debug.Log("Fetching lobbies");
            var task = await LobbyService.Instance.QueryLobbiesAsync(new QueryLobbiesOptions());
            return task.Results;
        }
    }
}
