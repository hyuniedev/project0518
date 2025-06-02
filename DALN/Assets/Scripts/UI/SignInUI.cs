using System;
using Controller;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SignInUI : MonoBehaviour
    {
        [SerializeField] private InputField usernameInput;
        [SerializeField] private InputField passwordInput;
        [SerializeField] private Button signinButton;
        [SerializeField] private Button signupButton;
        [SerializeField] private Button signinWithAnonymousButton;

        private void Start()
        {
            signinButton.onClick.AddListener(SignIn);
            signupButton.onClick.AddListener(Signup);
            signinWithAnonymousButton.onClick.AddListener(SigninWithAnonymous);
        }

        private async void SignIn()
        {
            await AccountController.Instance.SignIn(usernameInput.text, passwordInput.text);
            UIController.Instance.ToSceneHome();
        }

        private void Signup()
        {
            UIController.Instance.ToSceneSignUp();
        }

        private async void SigninWithAnonymous()
        {
            await AccountController.Instance.SignInWithAnonymous();
            UIController.Instance.ToSceneHome();
        }
    }
}