using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Controller
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private GameObject homePanel;
        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private GameObject signinPanel;
        [SerializeField] private GameObject signUpPanel;
        [SerializeField] private GameObject notificationPanel;

        private Transform _parent;
        private Text _title;
        private Text _message;
        private Button _confirmButton;
        
        #region Setup Singleton

        private static UIController _instance;

        public static UIController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<UIController>();
                    if(_instance==null)
                        _instance = new GameObject("UI Controller").AddComponent<UIController>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }

            _instance = this;
        }

        #endregion

        private void OnEnable()
        {
            StartCoroutine(WaitUnityServicesInitialized());
        }

        private IEnumerator WaitUnityServicesInitialized()
        {
            while (!AccountController.Instance.Initialized)
            {
                yield return null;
            }
            _ = InitScene();
        }

        private async Task InitScene()
        {
            if (LobbyController.Instance.CurrentLobby != null)
            {
                ToSceneLobby();
            }
            else if (AccountController.Instance.SignedIn)
            {
                ToSceneHome();
            }else if (AccountController.Instance.SessionTokenExists)
            {
                await AccountController.Instance.SignInWithAnonymous();
                ToSceneHome();
            }
            else
            {
                ToSceneSignIn();
            }
            AudioController.Instance.Play("BackgroundMusic_HomeScene", Vector3.zero);
            _parent = notificationPanel.transform.GetChild(0);
            _title = _parent.GetChild(0).GetComponent<Text>();
            _message = _parent.GetChild(1).GetComponent<Text>();
            _confirmButton = _parent.GetChild(2).GetComponent<Button>();
        }

        public void ToSceneSignIn()
        {
            signinPanel.SetActive(true);
            homePanel.SetActive(false);
            lobbyPanel.SetActive(false);
            signUpPanel.SetActive(false);
        }

        public void ToSceneSignUp()
        {
            signinPanel.SetActive(false);
            homePanel.SetActive(false);
            lobbyPanel.SetActive(false);
            signUpPanel.SetActive(true);
        }

        public void ToSceneHome()
        {
            signinPanel.SetActive(false);
            homePanel.SetActive(true);
            lobbyPanel.SetActive(false);
            signinPanel.SetActive(false);
        }

        public void ToSceneLobby()
        {
            signinPanel.SetActive(false);
            homePanel.SetActive(false);
            lobbyPanel.SetActive(true);
            signinPanel.SetActive(false);
        }

        public void ShowNotificationPanel(string title, string message, Action onConfirm)
        {
            notificationPanel.SetActive(true);
            try
            {
                _title.text = title;
                _message.text = message;
                _confirmButton.onClick.RemoveAllListeners();
                _confirmButton.onClick.AddListener(() =>
                {
                    onConfirm?.Invoke();
                });
            }
            catch (Exception _)
            {
                _parent = notificationPanel.transform.GetChild(0);
                _title = _parent.GetChild(0).GetComponent<Text>();
                _message = _parent.GetChild(1).GetComponent<Text>();
                _confirmButton = _parent.GetChild(2).GetComponent<Button>();
                
                _title.text = title;
                _message.text = message;
                _confirmButton.onClick.RemoveAllListeners();
                _confirmButton.onClick.AddListener(() =>
                {
                    onConfirm?.Invoke();
                    notificationPanel.SetActive(false);
                });
            }
        }

        public void HideNotificationPanel()
        {
            notificationPanel.SetActive(false);
        }
    }
}