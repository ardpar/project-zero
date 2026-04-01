using UnityEngine;
using Synthborn.Core.Events;
using Synthborn.Core.Data;
using Synthborn.Core.Pool;

namespace Synthborn.Enemies
{
    /// <summary>
    /// A poison trail segment left by PoisonerBrain.
    /// Deals DPS to the player while overlapping, then returns to pool after duration.
    /// Implements IPoolable for zero-allocation reuse.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class PoisonTrailSegment : MonoBehaviour, IPoolable
    {
        private float _duration;
        private int _damagePerSecond;
        private float _damageTickInterval;
        private float _damageTimer;
        private float _lifetime;
        private bool _initialized;
        private SpriteRenderer _sr;
        private CircleCollider2D _col;
        private ObjectPool<PoisonTrailSegment> _pool;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _col = GetComponent<CircleCollider2D>();
            _col.isTrigger = true;
        }

        /// <summary>Set pool reference for self-return.</summary>
        public void SetPool(ObjectPool<PoisonTrailSegment> pool) => _pool = pool;

        /// <summary>Configure this trail segment at spawn time.</summary>
        public void Initialize(float duration, int damagePerSecond, float radius, float damageTickInterval = 0.5f)
        {
            _duration = duration;
            _damagePerSecond = damagePerSecond;
            _damageTickInterval = damageTickInterval;
            _lifetime = 0f;
            _damageTimer = 0f;
            _initialized = true;

            _col.radius = radius;

            // Reset visual
            if (_sr != null)
            {
                var c = _sr.color;
                _sr.color = new Color(c.r, c.g, c.b, 0.6f);
            }
        }

        private void Update()
        {
            if (!_initialized) return;

            _lifetime += Time.deltaTime;
            if (_lifetime >= _duration)
            {
                ReturnToPool();
                return;
            }

            // Fade out visual over last 30% of lifetime
            if (_sr != null)
            {
                float fadeStart = _duration * 0.7f;
                if (_lifetime > fadeStart)
                {
                    float alpha = 1f - (_lifetime - fadeStart) / (_duration - fadeStart);
                    var c = _sr.color;
                    _sr.color = new Color(c.r, c.g, c.b, alpha * 0.6f);
                }
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!_initialized) return;
            if (!other.CompareTag("Player")) return;

            _damageTimer += Time.deltaTime;
            if (_damageTimer >= _damageTickInterval)
            {
                _damageTimer = 0f;
                int damage = Mathf.RoundToInt(_damagePerSecond * _damageTickInterval);
                var info = new DamageInfo(damage, DamageSource.EnemyContact, false, transform.position);
                GameEvents.RaisePlayerDamageRequested(info);
            }
        }

        private void ReturnToPool()
        {
            if (_pool != null)
                _pool.Return(this);
            else
                Destroy(gameObject);
        }

        public void OnPoolGet()
        {
            _lifetime = 0f;
            _damageTimer = 0f;
            _initialized = false;
            gameObject.SetActive(true);
        }

        public void OnPoolReturn()
        {
            _initialized = false;
            gameObject.SetActive(false);
        }
    }
}
