using UnityEngine;
using Synthborn.Core.Pool;

namespace Synthborn.Enemies
{
    /// <summary>
    /// Two-state brain: Chase → Dead.
    /// Like ChaserBrain but designed for small, fast, weak enemies
    /// that spawn in groups (3-5 per spawn entry).
    ///
    /// Behaviour:
    ///   Chase — moves at high speed toward the player with slight random offset
    ///           to prevent all swarm members from stacking on the same pixel.
    ///           Low HP, low damage, but overwhelming in numbers.
    ///   Dead  — base class handles pool return.
    ///
    /// Design intent: screen-filling quantity over individual threat.
    /// Countered by AoE/pierce attacks. Synergizes with waves that
    /// also contain tough enemies (player must choose targets).
    /// </summary>
    public class SwarmBrain : EnemyBrain
    {
        private Vector2 _jitterOffset;
        private float _jitterTimer;
        private const float JitterInterval = 0.5f;
        private const float JitterRadius = 0.8f;

        public override void Initialize(Transform player, int waveNumber, ObjectPool<EnemyBrain> pool, EnemyData overrideData = null)
        {
            // Assign initial random offset so swarm members spread out
            RandomizeJitter();
            base.Initialize(player, waveNumber, pool, overrideData);
        }

        protected override void EnterState(EnemyState newState) { }

        protected override void Tick()
        {
            if (CurrentState != EnemyState.Chase) return;

            // Periodically change jitter offset for organic movement
            _jitterTimer += Time.fixedDeltaTime;
            if (_jitterTimer >= JitterInterval)
            {
                _jitterTimer = 0f;
                RandomizeJitter();
            }

            ChasePlayerWithJitter();
            TickContactDamage();
        }

        private void ChasePlayerWithJitter()
        {
            if (playerTransform == null) return;

            Vector2 targetPos = (Vector2)playerTransform.position + _jitterOffset;
            Vector2 dir = (targetPos - rb.position).normalized;
            rb.linearVelocity = dir * effectiveSpeed;
        }

        private void RandomizeJitter()
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            _jitterOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(0f, JitterRadius);
        }
    }
}
