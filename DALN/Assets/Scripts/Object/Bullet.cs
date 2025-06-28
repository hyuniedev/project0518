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
        private int _damage;

        private float _timeAlive;

        private void OnEnable() => _timeAlive = Time.time + 10f;

        private void Update()
        {
            if (!IsServer) return;
            transform.position += _direction * Time.deltaTime * 10f;
            if (_timeAlive < Time.time) DisableBullet();
        }

        public void Fire(int teamId, Vector3 position ,Vector3 direction, int damage)
        {
            this._teamId = teamId;
            this._direction = direction;
            this._damage = damage;
            transform.position = position;
            _findEnemy = true;
        }
        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer || !_findEnemy) return;
            // if (other.TryGetComponent<Soldier>(out var soldier))
            // {
            //     if (soldier.TeamId.Value == _teamId) return;
            //     if (other.TryGetComponent<IGetDamage>(out IGetDamage enemy))
            //         enemy.GetDamage(1);
            //     DisableBullet();
            // }
            if (other.TryGetComponent<IGetDamage>(out var target))
            {
                if (other.TryGetComponent<Soldier>(out var soldier))
                    if (soldier.TeamId.Value == _teamId) return;
                target.GetDamage(_damage);
                DisableBullet();
            }
            else if(other.gameObject.layer == LayerMask.NameToLayer("House"))
                DisableBullet();
        }

        private void DisableBullet()
        {
            BulletObjectPool.Instance.Enqueue(this.gameObject);
            _findEnemy = false;
        }
    }
}