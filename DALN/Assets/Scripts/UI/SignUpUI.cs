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
            if (passwordInput.text != repeatPasswordInput.text)
            {
                UIController.Instance.ShowNotificationPanel("Error", "Re-enter password does not match password.", UIController.Instance.HideNotificationPanel);
                return;
            }
            UIController.Instance.ShowNotificationPanel("Signing up", "Wait a minute...", ()=>{});
            var result = await AccountController.Instance.SignUp(usernameInput.text, passwordInput.text);
            if(result)
            {
                UIController.Instance.ToSceneHome();
                UIController.Instance.HideNotificationPanel();
            }
        }

        private void BackToLogin()
        {
            UIController.Instance.ToSceneSignIn();
        }
    }
}