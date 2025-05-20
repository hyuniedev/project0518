using Unity.Netcode;
using UnityEngine;

public class GameController : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    // public override void OnNetworkSpawn()
    // {
    //     if (IsServer)
    //     {
    //         NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    //     }
    // }
    //
    // private void HandleClientConnected(ulong clientId)
    // {
    //     GameObject playerObj = Instantiate(playerPrefab);
    //     playerObj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
    // }
}