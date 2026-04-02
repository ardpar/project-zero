using UnityEngine;
using Synthborn.Core.Pool;

namespace Synthborn.Enemies
{
    /// <summary>
    /// Three-state brain: Chase → Shielding → (Chase) → Dead.
    ///
    /// Behaviour:
    ///   Chase     — moves toward the player until within ShieldRange.
    ///   Shielding — raises a frontal shield (reduces damage from front by ShieldReduction%).
    ///               Continues moving slowly. Vulnerable from behind.
    ///               Returns to Chase if player escapes range.
    ///   Dead      — base class handles pool return.
    ///
    /// Design intent: forces player to reposition behind the Shielder
    /// or use piercing/AoE attacks. Creates positional gameplay.
    /// </summary>
    public class ShielderBrain : EnemyBrain
    {
        [Header("Shielder Config")]
        [SerializeField] private float _shieldRange = 4f;
        [SerializeField] private float _shieldReduction = 0.75f;
        [SerializeField] private float _shieldedSpeedMultiplier = 0.5f;

        private bool _shieldActive;

        /// <summary>Whether the shield is currently raised.</summary>
        public bool ShieldActive => _shieldActive;

        /// <summary>Damage reduction when attacked from front while shielding (0-1).</summary>
        public float ShieldReduction => _shieldActive ? _shieldReduction : 0f;

        public override void Initialize(Transform player, int waveNumber, ObjectPool<EnemyBrain> pool, EnemyData overrideData = null)
        {
            _shieldActive = false;
            base.Initialize(player, waveNumber, pool, overrideData);
        }

        protected override void EnterState(EnemyState newState)
        {
            switch (newState)
            {
                case EnemyState.Shielding:
                    _shieldActive = true;
                    break;
                case EnemyState.Chase:
                    _shieldActive = false;
                    break;
            }
        }

        protected override void Tick()
        {
            switch (CurrentState)
            {
                case EnemyState.Chase:
                    TickChase();
                    break;
                case EnemyState.Shielding:
                    TickShielding();
                    break;
            }
        }

        private void TickChase()
        {
            float sqrDist = SqrDistanceToPlayer();
            if (sqrDist <= _shieldRange * _shieldRange)
            {
                SetState(EnemyState.Shielding);
                return;
            }

            ChasePlayer();
            TickContactDamage();
        }

        private void TickShielding()
        {
            float sqrDist = SqrDistanceToPlayer();
            float escapeRange = _shieldRange * 1.5f;
            if (sqrDist > escapeRange * escapeRange)
            {
                SetState(EnemyState.Chase);
                return;
            }

            // Move slowly toward player while shielding
            if (playerTransform != null)
            {
                Vector2 dir = ((Vector2)playerTransform.position - rb.position).normalized;
                rb.linearVelocity = dir * effectiveSpeed * _shieldedSpeedMultiplier;
            }

            TickContactDamage();
        }

        private float SqrDistanceToPlayer()
        {
            if (playerTransform == null) return float.MaxValue;
            return (rb.position - (Vector2)playerTransform.position).sqrMagnitude;
        }
    }
}
