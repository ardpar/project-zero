using UnityEngine;
using Synthborn.Core.Pool;
using Synthborn.Core.Events;
using Synthborn.Combat.Projectile;

namespace Synthborn.Enemies
{
    /// <summary>
    /// Three-state brain: Chase → Shooting → (Chase) → Dead.
    ///
    /// Behaviour:
    ///   Chase    — moves toward the player until within ShootRange.
    ///   Shooting — stops, fires a projectile every shootInterval seconds
    ///              (wave-scaled: interval * (1 - wave * 0.02), min 0.5 s).
    ///              Returns to Chase if the player escapes ShootRange.
    ///   Dead     — base class handles this.
    ///
    /// Projectiles are fired by raising <see cref="GameEvents.OnProjectileRequested"/>
    /// (loose coupling — the ProjectilePool subscribes and handles instantiation).
    /// </summary>
    public class ShooterBrain : EnemyBrain
    {
        // ------------------------------------------------------------------ //
        // Private State
        // ------------------------------------------------------------------ //

        private ShooterData _shooterData;
        private float       _shootTimer;
        private float       _effectiveShootInterval;

        // ------------------------------------------------------------------ //
        // EnemyBrain overrides
        // ------------------------------------------------------------------ //

        /// <inheritdoc/>
        public override void Initialize(Transform player, int waveNumber, ObjectPool<EnemyBrain> pool)
        {
            _shooterData = data as ShooterData;
            if (_shooterData == null)
                Debug.LogError($"[ShooterBrain] EnemyData on {name} must be a ShooterData asset.", this);

            // shoot_interval * (1 - wave * 0.02), minimum 0.5 s  (GDD formula)
            if (_shooterData != null)
            {
                _effectiveShootInterval = Mathf.Max(
                    _shooterData.ShootInterval * (1f - waveNumber * 0.02f),
                    0.5f);
            }

            base.Initialize(player, waveNumber, pool);
        }

        /// <inheritdoc/>
        protected override void EnterState(EnemyState newState)
        {
            switch (newState)
            {
                case EnemyState.Shooting:
                    // Stop movement immediately
                    rb.linearVelocity = Vector2.zero;
                    _shootTimer = 0f; // Fire first shot after one interval
                    break;
            }
        }

        /// <inheritdoc/>
        protected override void Tick()
        {
            if (_shooterData == null) return;

            switch (CurrentState)
            {
                case EnemyState.Chase:
                    TickChase();
                    break;
                case EnemyState.Shooting:
                    TickShooting();
                    break;
            }
        }

        // ------------------------------------------------------------------ //
        // State Handlers
        // ------------------------------------------------------------------ //

        private void TickChase()
        {
            float dist = DistanceToPlayer();
            if (dist <= _shooterData.ShootRange)
            {
                SetState(EnemyState.Shooting);
                return;
            }

            ChasePlayer();
            TickContactDamage();
        }

        private void TickShooting()
        {
            // Return to chase if player moves out of range
            float dist = DistanceToPlayer();
            if (dist > _shooterData.ShootRange)
            {
                SetState(EnemyState.Chase);
                return;
            }

            // Stay still while shooting
            rb.linearVelocity = Vector2.zero;

            _shootTimer += Time.fixedDeltaTime;
            if (_shootTimer >= _effectiveShootInterval)
            {
                _shootTimer = 0f;
                FireProjectile();
            }
        }

        private void FireProjectile()
        {
            if (_shooterData.ProjectileData == null)
            {
                Debug.LogWarning($"[ShooterBrain] {name}: ProjectileData is null — cannot fire.", this);
                return;
            }

            if (playerTransform == null) return;

            Vector2 dir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;

            // Loose coupling: raise event; ProjectilePool handles spawning
            GameEvents.RaiseProjectileRequested(
                transform.position,
                dir,
                _shooterData.ProjectileData);
        }

        // ------------------------------------------------------------------ //
        // Helpers
        // ------------------------------------------------------------------ //

        private float DistanceToPlayer()
        {
            if (playerTransform == null) return float.MaxValue;
            return Vector2.Distance(rb.position, (Vector2)playerTransform.position);
        }
    }
}
