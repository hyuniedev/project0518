using System.Collections.Generic;
using Data_Manager;
using Object;
using Unity.Netcode;
using UnityEngine;

namespace DesignPattern
{
    public class SoldierObjectPool : NetworkBehaviour
    {

        #region Setup Singleton

        private static SoldierObjectPool instance;

        public static SoldierObjectPool Singleton
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject("SoldierObjectPool").AddComponent<SoldierObjectPool>();
                }

                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        #endregion

        [SerializeField] private GameObject prefab;
        private Queue<Soldier> pool = new Queue<Soldier>();
        public void Enqueue(Soldier soldier)
        {
            if (!IsServer) return;
            soldier.gameObject.SetActive(false);
            soldier.GetComponent<NetworkObject>().Despawn(false);
            pool.Enqueue(soldier);
        }

        public ulong Dequeue(ulong ownerId, int teamId)
        {
            Soldier soldier;
            if (pool.Count > 0)
            {
                soldier = pool.Dequeue();
                soldier.gameObject.SetActive(true);
            }
            else
            {
                var go = Instantiate(prefab, GameData.Instance.gameData.TeamInitialPosition[teamId], Quaternion.identity);
                soldier = go.GetComponent<Soldier>();
            }
            soldier.GetComponent<NetworkObject>().SpawnWithOwnership(ownerId);
            soldier.TeamId.Value = teamId;
            soldier.gameObject.layer = LayerMask.NameToLayer($"Soldier{teamId}");
            return soldier.GetComponent<NetworkObject>().NetworkObjectId;
        }
    }
}