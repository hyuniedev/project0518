using System;
using System.Collections.Generic;
using Object;
using Unity.Netcode;
using UnityEngine;

namespace Data_Manager
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
            soldier.GetComponent<NetworkObject>().Despawn();
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
                var go = Instantiate(prefab, GameData.TeamInitialPosition[teamId], Quaternion.identity);
                soldier = go.GetComponent<Soldier>();
            }

            soldier.GetComponent<NetworkObject>().SpawnWithOwnership(ownerId);
            return soldier.GetComponent<NetworkObject>().NetworkObjectId;
        }
    }
}