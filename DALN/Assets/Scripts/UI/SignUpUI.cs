using System;
using Controller;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SignUpUI : MonoBehaviour
    {
        [SerializeField] private InputField usernameInput;
        [SerializeField] private InputField passwordInput;
        [SerializeField] private InputField repeatPasswordInput;
        [SerializeField] private Button signUpButton;
        [SerializeField] private Button backToLoginButton;

        private void Start()
        {
            signUpButton.onClick.AddListener(SignUp);
            backToLoginButton.onClick.AddListener(BackToLogin);
        }

        private async void SignUp()
        {
            if (passwordInput.text != repeatPasswordInput.text) return;
            await AccountController.Instance.SignUp(usernameInput.text, passwordInput.text);
            UIController.Instance.ToSceneHome();
        }

        private void BackToLogin()
        {
            UIController.Instance.ToSceneSignIn();
        }
    }
}