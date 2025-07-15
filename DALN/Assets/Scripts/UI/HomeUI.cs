using System;
using Controller;
using Data_Manager;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace UI
{
    public class HomeUI : MonoBehaviour
    {
        [SerializeField] private GameObject itemLobbyPrefab;
        [SerializeField] private Transform lobbiesParent;
        [SerializeField] private Button createNewLobbyButton;
        [SerializeField] private GameObject inputLabel;
        [SerializeField] private Button reloadButton;
        [SerializeField] private Button signOutButton;
        [SerializeField] private InputField nameInputField;
        [SerializeField] private Button changeNameButton;

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
            signOutButton.onClick.AddListener(async () =>
            {
                await AccountController.Instance.SignOut();
                UIController.Instance.ToSceneSignIn();
            });
            changeNameButton.onClick.AddListener(async () =>
            {
                nameInputField.readOnly = !nameInputField.readOnly;
                if (nameInputField.readOnly)
                {
                    Debug.Log($"Pre-Name: {PlayerData.Instance.Name}");
                    PlayerData.Instance.Name = nameInputField.text;
                    Debug.Log($"Cur-Name: {PlayerData.Instance.Name} - nameInputField.text: {nameInputField.text}");
                    await PlayerData.Instance.SaveData();
                }
            });
        }

        private void OnEnable()
        {
            ReloadLobbies();
            nameInputField.text = PlayerData.Instance.Name;
        }

        private void UpdateStateCreateNewLobbyButton()
        {
            inputLabel.SetActive(!inputLabel.activeSelf);
            createNewLobbyButton.GetComponentInChildren<Text>().text = inputLabel.activeSelf?"Cancel":"New Lobby";
        }
        
        private async void CreateNewLobby(string lobbyName)
        {
            if (lobbyName.IsNullOrEmpty()) return;
            var item = Instantiate(itemLobbyPrefab, lobbiesParent);
            item.GetComponentInChildren<Text>().text = lobbyName;
            var lobby = await LobbyController.Instance.CreateLobby(lobbyName);
            if(lobby == null) return;
            item.GetComponentInChildren<Button>().onClick.AddListener(()=>JoinLobby(lobby.Id));
            UIController.Instance.ToSceneLobby();
        }

        private async void JoinLobby(string lobbyId)
        {
            await LobbyController.Instance.JoinLobby(lobbyId);
            UIController.Instance.ToSceneLobby();
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
