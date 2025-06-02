using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Controller
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private GameObject homePanel;
        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private GameObject signinPanel;
        [SerializeField] private GameObject signUpPanel;

        #region Setup Singleton

        private static UIController _instance;

        public static UIController Instance
        {
            get
            {
                if (_instance == null)
                {
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

        private void Start()
        {
            ToSceneSignIn();
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
            if (AccountController.Instance.SignedIn)
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
    }
}