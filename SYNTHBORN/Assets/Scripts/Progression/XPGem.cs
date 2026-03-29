using UnityEngine;
using Synthborn.Core.Pool;
using Synthborn.Core.Events;
using Synthborn.Core.Persistence;

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
            _lifetime = _config != null ? _config.gemLifetime : 30f;
            _magnetized = false;

            // Fallback: find XPManager if not set
            if (_xpManager == null)
                _xpManager = Object.FindFirstObjectByType<XPManager>();
        }

        private void Update()
        {
            if (_player == null || Time.timeScale == 0f) return;

            _lifetime -= Time.deltaTime;
            if (_lifetime <= 0f)
            {
                ReturnToPool();
                return;
            }

            Vector2 offset = (Vector2)_player.position - (Vector2)transform.position;
            float sqrDist = offset.sqrMagnitude;

            // Enter magnet range
            if (!_magnetized && sqrDist < _config.pickupRadius * _config.pickupRadius)
                _magnetized = true;

            // Magnetized: pull toward player
            if (_magnetized)
            {
                Vector2 dir = offset.normalized;
                transform.Translate(dir * _config.magnetSpeed * Time.deltaTime);

                // Collect
                if (sqrDist < _config.collectDistance * _config.collectDistance)
                {
                    int scaledXP = Mathf.RoundToInt(_xpValue * UpgradeManager.XPGainMultiplier);
                    _xpManager?.AddXP(scaledXP);
                    GameEvents.XPGemCollected(scaledXP);
                    ReturnToPool();
                }
            }
        }

        private void ReturnToPool()
        {
            if (_pool != null)
                _pool.Return(this);
            else
                gameObject.SetActive(false);
        }

        public void OnPoolGet()
        {
            _magnetized = false;
            _lifetime = _config != null ? _config.gemLifetime : 30f;
            gameObject.SetActive(true);
        }

        public void OnPoolReturn()
        {
            gameObject.SetActive(false);
        }
    }
}
