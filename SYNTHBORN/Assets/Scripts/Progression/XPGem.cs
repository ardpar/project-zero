using UnityEngine;
using Synthborn.Core;

namespace Synthborn.Progression
{
    /// <summary>
    /// Pooled XP gem: sits on ground, magnetizes when player is near, collected on contact.
    /// </summary>
    public class XPGem : MonoBehaviour, IPoolable
    {
        [SerializeField] private XPConfig _config;
        private int _xpValue;
        private float _lifetime;
        private bool _magnetized;
        private Transform _player;
        private XPManager _xpManager;
        private ObjectPool<XPGem> _pool;
        /// <summary>Set pool reference for return.</summary>
        public void SetPool(ObjectPool<XPGem> pool) => _pool = pool;
        /// <summary>Set XP manager reference.</summary>
        public void SetXPManager(XPManager manager) => _xpManager = manager;
        /// <summary>Initialize gem with value after pool get.</summary>
        public void Init(int xpValue, Transform player)
        {
            _xpValue = xpValue;
            _player = player;
            _lifetime = _config.gemLifetime;
            _magnetized = false;
        }
        private void Update()
            if (_player == null || Time.timeScale == 0f) return;
            _lifetime -= Time.deltaTime;
            if (_lifetime <= 0f)
            {
                ReturnToPool();
                return;
            }
            float dist = Vector2.Distance(transform.position, _player.position);
            // Enter magnet range
            if (!_magnetized && dist < _config.pickupRadius)
                _magnetized = true;
            // Magnetized: pull toward player
            if (_magnetized)
                Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
                transform.Translate(dir * _config.magnetSpeed * Time.deltaTime);
                // Collect
                if (dist < _config.collectDistance)
                {
                    _xpManager?.AddXP(_xpValue);
                    GameEvents.XPGemCollected(_xpValue);
                    ReturnToPool();
                }
        private void ReturnToPool()
            if (_pool != null)
                _pool.Return(this);
            else
                gameObject.SetActive(false);
        public void OnPoolGet()
            _lifetime = _config != null ? _config.gemLifetime : 30f;
            gameObject.SetActive(true);
        public void OnPoolReturn()
            gameObject.SetActive(false);
    }
}
