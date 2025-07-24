using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Controller;
using Data_Manager;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button exitLobbyButton;
        [SerializeField] private GameObject itemPlayerPrefab;
        [SerializeField] private Transform playersParent;
        [SerializeField] private Text lobbyNameText;
        
        private int _currentNumPlayers = 0;
        private Coroutine _updateUILobbyCoroutine;
        private void Start()
        {
            startGameButton.onClick.AddListener(StartGame);
            
            exitLobbyButton.onClick.AddListener(()=>ExitLobby(AuthenticationService.Instance.PlayerId));
            
        }

        private void OnEnable()
        {
            _updateUILobbyCoroutine = StartCoroutine(UpdateUILobby());
            lobbyNameText.text = LobbyController.Instance.CurrentLobby.Name;
            startGameButton.GetComponent<Button>().interactable = AuthenticationService.Instance.PlayerId ==
                                                                  LobbyController.Instance.CurrentLobby.HostId;
        }

        private void OnDisable()
        {
            StopCoroutine(_updateUILobbyCoroutine);
        }

        private void StartGame()
        {
            LobbyController.Instance.StartGame();
        }

        private IEnumerator UpdateUILobby()
        {
            while (LobbyController.Instance.CurrentLobby != null)
            {
                var players = LobbyController.Instance.CurrentLobby.Players;
                if (players.Count == _currentNumPlayers)
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }
                _currentNumPlayers = players.Count;
                foreach (Transform child in playersParent)
                    Destroy(child.gameObject);
                foreach (var player in players)
                {
                    var item = Instantiate(itemPlayerPrefab, playersParent);
                    if (player.Data.TryGetValue("Name", out var namePlayer))
                    {
                        item.transform.GetChild(0).GetComponent<Text>().text = namePlayer.Value;
                    }
                    if (player.Data.TryGetValue("Rank", out var rankPlayer))
                    {
                        if (player.Id == AuthenticationService.Instance.PlayerId &&
                            int.Parse(rankPlayer.Value) != PlayerData.Instance.Rank)
                        {
                            UpdatePlayerOptions upo = new UpdatePlayerOptions();
                            upo.Data = new Dictionary<string, PlayerDataObject>
                            {
                                {
                                    "Rank",
                                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,
                                        PlayerData.Instance.Rank.ToString())
                                }
                            };
                            _ = LobbyService.Instance.UpdatePlayerAsync(LobbyController.Instance.CurrentLobby.Id, player.Id, upo);
                            rankPlayer.Value = PlayerData.Instance.Rank.ToString();
                        }
                        item.transform.GetChild(1).GetComponent<Text>().text = "Rank: " + rankPlayer.Value;
                    }
                    var button = item.transform.GetChild(2).GetComponent<Button>();
                    if (LobbyController.Instance.CurrentLobby.HostId == AuthenticationService.Instance.PlayerId)
                    {
                        if(player.Id == AuthenticationService.Instance.PlayerId)
                            button.gameObject.SetActive(false);
                    }
                    else
                    {
                        button.gameObject.SetActive(false);
                    }
                    button.onClick.AddListener(async () => {await LobbyService.Instance.RemovePlayerAsync(LobbyController.Instance.CurrentLobby.Id, player.Id);});
                }
                yield return new WaitForSeconds(1f);
            }
        }
        
        private async void ExitLobby(string playerId)
        {
            UIController.Instance.ShowNotificationPanel("Exiting lobby", "Wait a minute...", ()=>{});
            await LobbyController.Instance.LeaveLobby(playerId);
            UIController.Instance.ToSceneHome();
            UIController.Instance.HideNotificationPanel();
        }
    }
}