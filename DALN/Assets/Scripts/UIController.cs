using Unity.Netcode;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject ui;
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        // ui.SetActive(false);
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        // ui.SetActive(false);
    }
    public void StartServer() => NetworkManager.Singleton.StartServer();
}