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

        public async Task SignUp(string username, string password)
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
        }

        public async Task SignIn(string username, string password)
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            await PlayerData.Instance.LoadData();
        }

        public async Task SignInWithAnonymous()
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await PlayerData.Instance.LoadData();
        }
    }
}