using System;
using System.Collections.Generic;
using Object;
using Unity.Netcode;
using UnityEngine;

namespace DesignPattern
{
    public class BulletObjectPool : NetworkBehaviour
    {
        #region Setup Singleton

        private static BulletObjectPool _instance;

        public static BulletObjectPool Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("BulletObjectPool").AddComponent<BulletObjectPool>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if(_instance!=null && _instance != this)
                Destroy(this.gameObject);
            _instance = this;
        }

        #endregion
        
        [SerializeField] private GameObject bulletPrefab;
        private Queue<GameObject> pool = new Queue<GameObject>();

        public void Enqueue(GameObject obj)
        {
            obj.SetActive(false);
            obj.GetComponent<NetworkObject>().Despawn(false);
            pool.Enqueue(obj);
        }

        public void Dequeue(int teamId, Transform gunBarrelPosition, Vector3 direction)
        {
            GameObject bullet;
            Debug.Log($"Bullet Pool Count: {pool.Count}");
            if (pool.Count > 0)
            {
                bullet = pool.Dequeue();
                bullet.SetActive(true);
            }
            else
            {
                bullet = Instantiate(bulletPrefab, gunBarrelPosition.position, Quaternion.identity);
            }
            bullet.GetComponent<Bullet>().Fire(teamId, gunBarrelPosition.position, direction);
            bullet.GetComponent<NetworkObject>().Spawn();
        }
    }
}