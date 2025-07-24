using System;
using System.Collections;
using Data_Manager;
using Object;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Controller
{
    public class GameController : NetworkBehaviour
    {
        [SerializeField] private GameObject playerPrefab;

        public void Start()
        {
            StartCoroutine(AddListenerOnLoadComplete());
        }
        
        private IEnumerator AddListenerOnLoadComplete()
        {
            while (NetworkManager.Singleton == null || NetworkManager.Singleton.SceneManager == null)
                yield return null;
            NetworkManager.Singleton.SceneManager.OnLoadComplete += SpawnPlayerPrefabForPerPlayerConnected;
        }

        private void SpawnPlayerPrefabForPerPlayerConnected(ulong playerId, string sceneName, LoadSceneMode mode)
        {
            if (!IsServer) return;
            if(sceneName != "GameScene") return;
            SpawnPlayerPrefab(playerId);
        }

        private void SpawnPlayerPrefab(ulong playerId)
        {
            var player = Instantiate(playerPrefab);
            player.GetComponent<NetworkObject>().SpawnWithOwnership(playerId);
        }
        
        public override void OnDestroy()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
                NetworkManager.Singleton.SceneManager.OnLoadComplete -= SpawnPlayerPrefabForPerPlayerConnected;
            base.OnDestroy();
        }
    }
}