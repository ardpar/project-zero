using UnityEngine;
using Synthborn.Core.Pool;
using Synthborn.Core.Events;

namespace Synthborn.Enemies
{
    /// <summary>
    /// MVP Boss brain — behaves identically to <see cref="ChaserBrain"/> (Chase → Dead)
    /// but is a distinct class so Vertical Slice can add phase transitions, special
    /// attacks, and minion spawning without touching the Chaser hierarchy.
    ///
    /// MVP behaviour:
    ///   - Large, slow Chaser with high HP (10x Normal), high contact damage.
    ///   - All tuning via <see cref="BossData"/> ScriptableObject.
    ///   - On death raises <see cref="GameEvents.OnBossDefeated"/> in addition
    ///     to the standard OnEnemyDied event.
    ///
    /// Vertical Slice hook (not implemented here):
    ///   Phase transitions: override EnterState(Chase) to check BossData.Phases
    ///   when HP crosses a threshold.
    /// </summary>
    public class BossBrain : EnemyBrain
    {
        // ------------------------------------------------------------------ //
        // Private State
        // ------------------------------------------------------------------ //

        private BossData _bossData;

        // ------------------------------------------------------------------ //
        // EnemyBrain overrides
        // ------------------------------------------------------------------ //

        /// <inheritdoc/>
        public override void Initialize(Transform player, int waveNumber, ObjectPool<EnemyBrain> pool, EnemyData overrideData = null)
        {
            _bossData = data as BossData;
            if (_bossData == null)
                Debug.LogWarning($"[BossBrain] EnemyData on {name} should be a BossData asset for VS+ features.", this);

            base.Initialize(player, waveNumber, pool, overrideData);

            // Cinematic entrance: fire intro VFX event if configured
            if (_bossData?.IntroVfx != null)
                GameEvents.RaiseVfxRequested(_bossData.IntroVfx, transform.position);
        }

        /// <inheritdoc/>
        protected override void EnterState(EnemyState newState)
        {
            if (newState == EnemyState.Dead)
            {
                // Boss-specific: raise global boss-defeated event so WaveSpawner
                // can transition to Complete state and Run Manager can react.
                GameEvents.RaiseBossDefeated();
            }
        }

        /// <inheritdoc/>
        protected override void Tick()
        {
            if (CurrentState != EnemyState.Chase) return;

            ChasePlayer();
            TickContactDamage();
        }
    }
}
