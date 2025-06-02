using System;
using Controller;
using Unity.Netcode;
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

        private void Start()
        {
            startGameButton.onClick.AddListener(StartGame);
            exitLobbyButton.onClick.AddListener(ExitLobby);
        }

        private void StartGame()
        {
            LobbyController.Instance.StartGame();
        }

        private async void ExitLobby()
        {
            await LobbyController.Instance.LeaveLobby();
        }
    }
}