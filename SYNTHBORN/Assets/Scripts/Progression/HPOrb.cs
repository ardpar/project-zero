using UnityEngine;
using Synthborn.Core.Pool;
using Synthborn.Core.Events;
using Synthborn.Combat.Health;

namespace Synthborn.Progression
{
    /// <summary>
    /// Pooled HP orb: dropped by enemies on death (5% chance).
    /// Magnetizes to player and heals on contact.
    /// </summary>
    public class HPOrb : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _pickupRadius = 1.5f;
        [SerializeField] private float _magnetSpeed = 12f;
        [SerializeField] private float _collectDistance = 0.3f;
        [SerializeField] private float _lifetime = 15f;
        [SerializeField] private int _healAmount = 10;

        private float _timer;
        private bool _magnetized;
        private Transform _player;
        private EntityHealth _playerHealth;
        private ObjectPool<HPOrb> _pool;

        public void SetPool(ObjectPool<HPOrb> pool) => _pool = pool;

        public void Init(Transform player, EntityHealth playerHealth)
        {
            _player = player;
            _playerHealth = playerHealth;
            _timer = _lifetime;
            _magnetized = false;
        }

        private void Update()
        {
            if (_player == null || Time.timeScale == 0f) return;

            _timer -= Time.deltaTime;
            if (_timer <= 0f) { ReturnToPool(); return; }

            float dist = Vector2.Distance(transform.position, _player.position);

            if (!_magnetized && dist < _pickupRadius)
                _magnetized = true;

            if (_magnetized)
            {
                Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
                transform.Translate(dir * _magnetSpeed * Time.deltaTime);

                if (dist < _collectDistance)
                {
                    _playerHealth?.Heal(_healAmount);
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
            _timer = _lifetime;
            gameObject.SetActive(true);
        }

        public void OnPoolReturn()
        {
            gameObject.SetActive(false);
        }
    }
}
