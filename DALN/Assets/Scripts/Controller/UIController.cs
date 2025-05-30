using System;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Controller
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private GameObject itemLobbyPrefab;
        [SerializeField] private Transform lobbiesParent;
        [SerializeField] private Button createNewLobbyButton;
        [SerializeField] private GameObject inputLabel;
        [SerializeField] private Button reloadButton;

        private void Start()
        {
            inputLabel.SetActive(false);
            createNewLobbyButton.onClick.AddListener(UpdateStateCreateNewLobbyButton);
            inputLabel.GetComponentInChildren<Button>().onClick.AddListener(()=>
            {
                UpdateStateCreateNewLobbyButton();
                CreateNewLobby(inputLabel.GetComponentInChildren<InputField>().text);
                inputLabel.GetComponentInChildren<InputField>().text = "";
            });
            reloadButton.onClick.AddListener(ReloadLobbies);
        }

        private void UpdateStateCreateNewLobbyButton()
        {
            inputLabel.SetActive(!inputLabel.activeSelf);
            createNewLobbyButton.GetComponentInChildren<Text>().text = inputLabel.activeSelf?"Cancel":"New Lobby";
        }
        public void StartGame()
        {
            if (LobbyController.Instance.CurrentLobby.Id != AuthenticationService.Instance.PlayerId) return;
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }

        private async void CreateNewLobby(string lobbyName)
        {
            var item = Instantiate(itemLobbyPrefab, lobbiesParent);
            item.GetComponentInChildren<Text>().text = lobbyName;
            var lobby = await LobbyController.Instance.CreateLobby(lobbyName);
            if(lobby == null) return;
            item.GetComponentInChildren<Button>().onClick.AddListener(()=>JoinLobby(lobby.Id));
        }

        private void JoinLobby(string lobbyId)
        {
            LobbyController.Instance.JoinLobby(lobbyId);
        }

        private async void ReloadLobbies()
        {
            var ls = await LobbyController.Instance.FetchLobbies();
            foreach (Transform child in lobbiesParent)
                Destroy(child.gameObject);
            foreach (var l in ls)
            {
                var item = Instantiate(itemLobbyPrefab, lobbiesParent);
                item.GetComponentInChildren<Text>().text = l.Name;
                item.GetComponentInChildren<Button>().onClick.AddListener(()=>JoinLobby(l.Id));
            }
        }
    }    
}
