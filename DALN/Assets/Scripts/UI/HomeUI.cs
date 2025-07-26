using System;
using System.Collections;
using Controller;
using Data_Manager;
using Unity.Services.Authentication;
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
        [SerializeField] private Text rankText;
        
        [SerializeField] private Sprite pencilSprite;
        [SerializeField] private Sprite saveSprite;

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
                UIController.Instance.ShowNotificationPanel("Signing out", "Wait a minute...", ()=>{});
                await AccountController.Instance.SignOut();
                UIController.Instance.ToSceneSignIn();
                UIController.Instance.HideNotificationPanel();
            });
            changeNameButton.onClick.AddListener(async () =>
            {
                if (nameInputField.interactable)
                {
                    UIController.Instance.ShowNotificationPanel("Changing name", "Wait a minute...", ()=>{});
                    PlayerData.Instance.Name = nameInputField.text;
                    await PlayerData.Instance.SaveData();
                    UIController.Instance.HideNotificationPanel();
                }
                nameInputField.interactable = !nameInputField.interactable;
                changeNameButton.transform.GetChild(0).GetComponent<Image>().sprite = nameInputField.interactable?saveSprite:pencilSprite;
            });
        }

        private void OnEnable()
        {
            InitShowNameAndRank();
            if(AccountController.Instance.Initialized)
                ReloadLobbies();
        }

        public void InitShowNameAndRank()
        {
            nameInputField.text = PlayerData.Instance.Name;
            rankText.text = "Rank: " + PlayerData.Instance.Rank;
        }
        
        private void UpdateStateCreateNewLobbyButton()
        {
            inputLabel.SetActive(!inputLabel.activeSelf);
            createNewLobbyButton.GetComponentInChildren<Text>().text = inputLabel.activeSelf?"Cancel":"New Lobby";
        }
        
        private async void CreateNewLobby(string lobbyName)
        {
            if (lobbyName.IsNullOrEmpty())
            {
                UIController.Instance.ShowNotificationPanel("Error", "Please enter a lobby name.", UIController.Instance.HideNotificationPanel);
                return;
            }
            UIController.Instance.ShowNotificationPanel("Creating lobby", "Wait a minute...", ()=>{});
            lobbyName = lobbyName.Trim();
            var item = Instantiate(itemLobbyPrefab, lobbiesParent);
            item.GetComponentInChildren<Text>().text = lobbyName;
            var lobby = await LobbyController.Instance.CreateLobby(lobbyName);
            if(lobby == null) return;
            item.GetComponentInChildren<Button>().onClick.AddListener(()=>JoinLobby(lobby.Id));
            UIController.Instance.ToSceneLobby();
            UIController.Instance.HideNotificationPanel();
        }

        private async void JoinLobby(string lobbyId)
        {
            UIController.Instance.ShowNotificationPanel("Joining lobby", "Wait a minute...", ()=>{});
            await LobbyController.Instance.JoinLobby(lobbyId);
            UIController.Instance.HideNotificationPanel();
            if(LobbyController.Instance.CurrentLobby != null) UIController.Instance.ToSceneLobby();
            else
            {
                UIController.Instance.ShowNotificationPanel("Error", "Lobby is full.", () =>
                {
                    UIController.Instance.HideNotificationPanel();
                    ReloadLobbies();
                });
            }
        }

        private async void ReloadLobbies()
        {
            UIController.Instance.ShowNotificationPanel("Loading lobbies", "Wait a minute...", ()=>{});
            var ls = await LobbyController.Instance.FetchLobbies();
            foreach (Transform child in lobbiesParent)
                Destroy(child.gameObject);
            foreach (var l in ls)
            {
                var item = Instantiate(itemLobbyPrefab, lobbiesParent);
                item.GetComponentInChildren<Text>().text = l.Name;
                item.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    if (l.Players.Count >= l.MaxPlayers)
                    {
                        UIController.Instance.ShowNotificationPanel("Error", "Lobby is full.", UIController.Instance.HideNotificationPanel);
                        return;
                    }
                    JoinLobby(l.Id);
                });
                item.transform.GetChild(1).GetChild(1).GetComponent<Text>().text = $"{l.Players.Count}/{GameData.Instance.gameData.maxPlayersPerLobby}";
            }
            UIController.Instance.HideNotificationPanel();
        }
    }    
}
