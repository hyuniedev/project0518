using System;
using System.Threading.Tasks;
using Data_Manager;
using DesignPattern;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Controller
{
    public class AccountController : Singleton<AccountController>
    {
        public bool SignedIn => AuthenticationService.Instance.IsSignedIn;

        public bool SessionTokenExists => AuthenticationService.Instance.SessionTokenExists;

        public bool Initialized { get; private set; }
        
        private async void Start()
        {
            Initialized = false;
            await UnityServices.InitializeAsync();
            Initialized = true;
        }

        public async Task SignOut()
        {
            await PlayerData.Instance.SaveData();
            AuthenticationService.Instance.SignOut();
        }

        public async Task<bool> SignUp(string username, string password)
        {
            try
            {
                await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
                GameData.Instance.SaveSessionData(false);
                PlayerData.Instance.Name = username;
                await PlayerData.Instance.SaveData();
                return true;
            }
            catch (Exception e)
            {
                UIController.Instance.ShowNotificationPanel("Error: ", e.Message , ()=>{UIController.Instance.HideNotificationPanel();});
                return false;
            }
        }

        public async Task<bool> SignIn(string username, string password)
        {
            try
            {
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
                await PlayerData.Instance.LoadData();
                GameData.Instance.SaveSessionData(false);
                return true;
            }
            catch (Exception e)
            {
                UIController.Instance.ShowNotificationPanel("Error: ", e.Message , ()=>{UIController.Instance.HideNotificationPanel();});
                return false;
            }
        }

        public async Task SignInWithAnonymous()
        {
            if (!GameData.Instance.PreviousSessionIsAnonymous()) AuthenticationService.Instance.ClearSessionToken();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await PlayerData.Instance.LoadData();
            GameData.Instance.SaveSessionData(true);
        }

        public async Task SignInWithPreviousSession()
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await PlayerData.Instance.LoadData();
        }
    }
}