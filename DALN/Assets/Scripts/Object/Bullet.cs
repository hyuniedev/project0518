using System;
using DesignPattern;
using Unity.Netcode;
using UnityEngine;

namespace Object
{
    public class Bullet : NetworkBehaviour
    {
        private bool _findEnemy = false;
        private NetworkVariable<Vector3> _direction = new NetworkVariable<Vector3>(Vector3.zero,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
        private int _teamId;
        private int _damage;
        private LineRenderer _lineRenderer;
        
        private float _timeAlive;

        private void OnEnable() => _timeAlive = Time.time + 10f;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if(IsClient)
                _lineRenderer = GetComponent<LineRenderer>();
        }

        private void Update()
        {
            if (IsClient)
            {
                _lineRenderer.SetPosition(0, transform.position);
                _lineRenderer.SetPosition(1, transform.position + _direction.Value * -0.5f);
            }
            
            if (!IsServer) return;
            transform.position += _direction.Value * Time.deltaTime * 10f;
            if (_timeAlive < Time.time) DisableBullet();
        }

        public void Fire(int teamId, Vector3 position ,Vector3 direction, int damage)
        {
            Debug.Log($"IsServer: {IsServer}");
            if (!IsServer) return;
            this._teamId = teamId;
            this._direction.Value = direction;
            this._damage = damage;
            transform.position = position;
            transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
            _findEnemy = true;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer || !_findEnemy) return;
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