using UnityEngine;
using Synthborn.Core.Pool;
using Synthborn.Core.Events;
using Synthborn.Combat.Health;

namespace Synthborn.Progression
{
    /// <summary>
    /// Pooled HP orb: dropped by enemies on death (5% chance).
    /// Magnetizes to player and heals on contact.
    /// Tuning values driven by HPOrbConfig ScriptableObject.
    /// </summary>
    public class HPOrb : MonoBehaviour, IPoolable
    {
        [SerializeField] private HPOrbConfig _config;

        private float _timer;
        private bool _magnetized;
        private Transform _player;
        private EntityHealth _playerHealth;
        private ObjectPool<HPOrb> _pool;

        // Cached squared distances for zero-sqrt comparisons
        private float _sqrPickupRadius;
        private float _sqrCollectDistance;

        public void SetPool(ObjectPool<HPOrb> pool) => _pool = pool;

        public void Init(Transform player, EntityHealth playerHealth)
        {
            _player = player;
            _playerHealth = playerHealth;
            _timer = _config != null ? _config.lifetime : 15f;
            _magnetized = false;

            float pickup = _config != null ? _config.pickupRadius : 1.5f;
            float collect = _config != null ? _config.collectDistance : 0.3f;
            _sqrPickupRadius = pickup * pickup;
            _sqrCollectDistance = collect * collect;
        }

        private void Update()
        {
            if (_player == null || Time.timeScale == 0f) return;

            _timer -= Time.deltaTime;
            if (_timer <= 0f) { ReturnToPool(); return; }

            Vector2 offset = (Vector2)_player.position - (Vector2)transform.position;
            float sqrDist = offset.sqrMagnitude;

            if (!_magnetized && sqrDist < _sqrPickupRadius)
                _magnetized = true;

            if (_magnetized)
            {
                float speed = _config != null ? _config.magnetSpeed : 12f;
                Vector2 dir = offset.normalized;
                transform.Translate(dir * speed * Time.deltaTime);

                if (sqrDist < _sqrCollectDistance)
                {
                    int heal = _config != null ? _config.healAmount : 10;
                    _playerHealth?.Heal(heal);
                    ReturnToPool();
                }
            }
        }

        private void ReturnToPool()
        {
            if (_pool != null) _pool.Return(this);
            else gameObject.SetActive(false);
        }

        public void OnPoolGet()
        {
            _magnetized = false;
            _timer = _config != null ? _config.lifetime : 15f;
            gameObject.SetActive(true);
        }

        public void OnPoolReturn()
        {
            gameObject.SetActive(false);
        }
    }
}
