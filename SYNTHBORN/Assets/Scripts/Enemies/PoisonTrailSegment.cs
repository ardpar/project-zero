using UnityEngine;
using Synthborn.Core.Events;
using Synthborn.Core.Data;

namespace Synthborn.Enemies
{
    /// <summary>
    /// A poison trail segment left by PoisonerBrain.
    /// Deals DPS to the player while overlapping, then self-destructs after duration.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class PoisonTrailSegment : MonoBehaviour
    {
        private float _duration;
        private int _damagePerSecond;
        private float _damageTimer;
        private float _lifetime;
        private bool _initialized;
        private SpriteRenderer _sr;

        /// <summary>Configure this trail segment at spawn time.</summary>
        public void Initialize(float duration, int damagePerSecond, float radius)
        {
            _duration = duration;
            _damagePerSecond = damagePerSecond;
            _lifetime = 0f;
            _damageTimer = 0f;
            _initialized = true;
            _sr = GetComponent<SpriteRenderer>();

            var col = GetComponent<CircleCollider2D>();
            col.radius = radius;
            col.isTrigger = true;
        }

        private void Update()
        {
            if (!_initialized) return;

            _lifetime += Time.deltaTime;
            if (_lifetime >= _duration)
            {
                Destroy(gameObject);
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
            if (_damageTimer >= 0.5f)
            {
                _damageTimer = 0f;
                int damage = Mathf.RoundToInt(_damagePerSecond * 0.5f);
                var info = new DamageInfo(damage, DamageSource.EnemyContact, false, transform.position);
                GameEvents.RaisePlayerDamageRequested(info);
            }
        }
    }
}
