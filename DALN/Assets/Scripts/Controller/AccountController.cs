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
                return true;
            }
            catch (Exception e)
            {
                UIController.Instance.ShowNotificationPanel("Error: ", e.Message , ()=>{});
                return false;
            }
        }

        public async Task<bool> SignIn(string username, string password)
        {
            try
            {
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
                await PlayerData.Instance.LoadData();
                return true;
            }
            catch (Exception e)
            {
                UIController.Instance.ShowNotificationPanel("Error: ", e.Message , ()=>{});
                return false;
            }
        }

        public async Task SignInWithAnonymous()
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await PlayerData.Instance.LoadData();
        }
    }
}