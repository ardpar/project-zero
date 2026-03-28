using System.Collections.Generic;
using UnityEngine;
using Synthborn.Core.Pool;
using Synthborn.Core.Data;
using Synthborn.Core.Events;
using Synthborn.Combat.Projectile;
using Synthborn.Combat.Projectile.HitBehavior;
using Synthborn.Combat.Health;

namespace Synthborn.Combat
{
    /// <summary>
    /// Projectile movement, collision, hit behavior dispatch, and pool lifecycle.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class ProjectileController : MonoBehaviour, IPoolable
    {
        [SerializeField] private ProjectileData _defaultData;

        private ProjectileData _data;
        private Vector2 _direction;
        private int _damage;
        private float _lifetime;
        private float _distanceTravelled;
        private bool _isCritCapable;
        private float _critChance;
        private float _critMultiplier;
        private float _damageDecayAccumulated;
        private ObjectPool<ProjectileController> _pool;
        private HashSet<int> _hitIds = new();
        private IHitBehavior _hitBehavior;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.gravityScale = 0f;
        }

        public void SetPool(ObjectPool<ProjectileController> pool) => _pool = pool;

        /// <summary>Initialize projectile after getting from pool.</summary>
        public void Fire(Vector2 direction, ProjectileData data, int baseDamage,
            float critChance, float critMultiplier)
        {
            _data = data != null ? data : _defaultData;
            _direction = direction.normalized;
            _damage = baseDamage;
            _critChance = critChance;
            _critMultiplier = critMultiplier;
            _lifetime = _data.maxLifetime;
            _distanceTravelled = 0f;
            _damageDecayAccumulated = 0f;
            _hitIds.Clear();

            _hitBehavior = _data.onHitBehavior switch
            {
                OnHitBehaviorType.Pierce => new PierceOnHit(_data.pierceCount, _data.pierceDecay),
                _ => new DestroyOnHit()
            };
        }

        /// <summary>Called by PierceOnHit to reduce damage per pierce.</summary>
        public void ApplyDamageDecay(float decayRate)
        {
            _damageDecayAccumulated += decayRate;
        }

        private void FixedUpdate()
        {
            if (_data == null) return;

            float dt = Time.fixedDeltaTime;
            float moveAmount = _data.projectileSpeed * dt;

            _rb.MovePosition(_rb.position + _direction * moveAmount);
            _distanceTravelled += moveAmount;
            _lifetime -= dt;

            if (_lifetime <= 0f || _distanceTravelled >= _data.maxRange)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var damageable = other.GetComponent<IDamageable>();
            if (damageable == null || damageable.IsDead) return;

            int instanceId = other.gameObject.GetInstanceID();
            if (_hitIds.Contains(instanceId)) return;
            _hitIds.Add(instanceId);

            // Calculate damage
            bool isCrit = Random.value < _critChance;
            float critMult = isCrit ? _critMultiplier : 1f;
            float decayMult = 1f - _damageDecayAccumulated;
            int finalDamage = Mathf.Max(Mathf.RoundToInt(_damage * critMult * decayMult), 1);

            var damageInfo = new DamageInfo(_damage, DamageSource.PlayerProjectile, isCrit, transform.position);

            GameEvents.RaiseDamageDealt(transform.position, finalDamage, isCrit);
            damageable.TakeDamage(damageInfo);

            bool shouldDestroy = _hitBehavior.OnHit(this, damageable, damageInfo);
            if (shouldDestroy)
            {
                ReturnToPool();
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
            _hitIds.Clear();
            _damageDecayAccumulated = 0f;
            gameObject.SetActive(true);
        }

        public void OnPoolReturn()
        {
            _data = null;
            gameObject.SetActive(false);
        }
    }
}
