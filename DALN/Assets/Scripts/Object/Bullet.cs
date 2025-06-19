using System;
using DesignPattern;
using Unity.Netcode;
using UnityEngine;

namespace Object
{
    public class Bullet : NetworkBehaviour
    {
        private bool _findEnemy = false;
        private Vector3 _direction;
        private int _teamId;
        private void Update()
        {
            if (!IsServer) return;
            transform.position += _direction * Time.deltaTime * 10f;
        }

        public void Fire(int teamId, Vector3 position ,Vector3 direction)
        {
            this._teamId = teamId;
            this._direction = direction;
            transform.position = position;
            _findEnemy = true;
        }
        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer || !_findEnemy) return;
            if (other.TryGetComponent(out IGetDamage enemy))
                enemy.GetDamage(1);
            else if (other.TryGetComponent<Soldier>(out var soldier))
            {
                if (soldier.TryGetComponent<NetworkObject>(out var networkObject))
                {
                    Debug.Log($"Soldier Id: {networkObject.NetworkObjectId}");
                }
                if (soldier.TeamId.Value == _teamId) return;
            }
            BulletObjectPool.Instance.Enqueue(this.gameObject);
            _findEnemy = false;
        }
    }
}