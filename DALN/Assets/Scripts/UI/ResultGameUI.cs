using System;
using Controller;
using Data_Manager;
using Object;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class ResultGameUI : MonoBehaviour
    {
        [SerializeField] private Transform parentContent;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private Button backToLobbyButton;
        [SerializeField] private Button backToHomeButton;

        private void Start()
        {
            backToLobbyButton.onClick.AddListener(() => SceneManager.LoadScene("HomeScene",LoadSceneMode.Single));
            backToHomeButton.onClick.AddListener(async () =>
            {
                await LobbyController.Instance.LeaveLobby(AuthenticationService.Instance.PlayerId);
                SceneManager.LoadScene("HomeScene", LoadSceneMode.Single);
            });
        }

        private void OnEnable()
        {
            var lobby = LobbyController.Instance.CurrentLobby;
            var winnerId = FindFirstObjectByType<AngelStatue>().WinnerId;
            foreach (var player in lobby.Players)
            {
                if (int.Parse(player.Data["TeamId"].Value) == winnerId)
                {
                    if(player.Id==AuthenticationService.Instance.PlayerId)
                        PlayerData.Instance.Rank = int.Parse(player.Data["Rank"].Value) + 10;
                    _ = PlayerData.Instance.SaveData();
                    break;
                }
            }
            LoadTableData(winnerId);
        }

        private void LoadTableData(int winnerId)
        {
            var lobby = LobbyController.Instance.CurrentLobby;
            foreach (var player in lobby.Players)
            {
                var item = Instantiate(itemPrefab, parentContent);
                if (player.Data.TryGetValue("Name", out var namePlayer))
                {
                    item.transform.GetChild(0).GetComponent<Text>().text = namePlayer.Value;
                }
                if (player.Data.TryGetValue("Rank", out var rankPlayer))
                {
                    if(int.Parse(player.Data["TeamId"].Value) == winnerId)
                        item.transform.GetChild(1).GetComponent<Text>().text = (int.Parse(rankPlayer.Value) + 10).ToString();
                    else
                        item.transform.GetChild(1).GetComponent<Text>().text = rankPlayer.Value;
                }
            }
        }
    }
}