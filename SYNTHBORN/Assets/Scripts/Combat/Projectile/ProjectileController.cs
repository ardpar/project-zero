using System.Collections.Generic;
using UnityEngine;
using Synthborn.Core;

namespace Synthborn.Combat
{
    /// <summary>
    /// Projectile movement, collision, hit behavior dispatch, and pool lifecycle.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
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
            _hitBehavior = _data.onHitType switch
            {
                OnHitType.Pierce => new PierceOnHit(_data.pierceCount, _data.pierceDecay),
                _ => new DestroyOnHit()
            };
        }
        /// <summary>Called by PierceOnHit to reduce damage per pierce.</summary>
        public void ApplyDamageDecay(float decayRate)
            _damageDecayAccumulated += decayRate;
        private void Update()
            if (_data == null) return;
            float dt = Time.deltaTime;
            float moveAmount = _data.speed * dt;
            transform.Translate(_direction * moveAmount);
            _distanceTravelled += moveAmount;
            _lifetime -= dt;
            if (_lifetime <= 0f || _distanceTravelled >= _data.maxRange)
                ReturnToPool();
            }
        private void OnTriggerEnter2D(Collider2D other)
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
            var damageInfo = new DamageInfo
                RawDamage = _damage,
                FinalDamage = finalDamage,
                Source = DamageSource.PlayerProjectile,
                IsCrit = isCrit,
                HitPosition = transform.position
            GameEvents.DamageDealt(transform.position, finalDamage, isCrit);
            damageable.TakeDamage(damageInfo);
            bool shouldDestroy = _hitBehavior.OnHit(this, damageable, damageInfo);
            if (shouldDestroy)
        private void ReturnToPool()
            if (_pool != null)
                _pool.Return(this);
            else
                gameObject.SetActive(false);
        public void OnPoolGet()
            gameObject.SetActive(true);
        public void OnPoolReturn()
            _data = null;
            gameObject.SetActive(false);
    }
}
