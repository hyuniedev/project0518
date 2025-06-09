using System;
using System.Collections;
using Controller;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Lobbies;
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
        
        private int _currentNumPlayers = 0;
        private Coroutine _updateUILobbyCoroutine;
        private void Start()
        {
            startGameButton.onClick.AddListener(StartGame);
            exitLobbyButton.onClick.AddListener(()=>ExitLobby(AuthenticationService.Instance.PlayerId));
            _updateUILobbyCoroutine = StartCoroutine(UpdateUILobby());
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
            while (true)
            {
                if (LobbyController.Instance.CurrentLobby != null)
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
                            item.transform.GetChild(1).GetComponent<Text>().text = rankPlayer.Value;
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
                }
                else
                {
                    Debug.LogWarning("No lobby selected");
                    yield break;
                }
                Debug.Log("Updating lobby");
                yield return new WaitForSeconds(1f);
            }
        }
        
        private async void ExitLobby(string playerId)
        {
            await LobbyController.Instance.LeaveLobby(playerId);
            UIController.Instance.ToSceneHome();
        }
    }
}