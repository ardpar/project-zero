using UnityEngine;
using Synthborn.Core.Pool;
using Synthborn.Core.Events;
using Synthborn.Combat.Health;

namespace Synthborn.Enemies
{
    /// <summary>
    /// Boss brain with phase transition support.
    ///
    /// Behaviour:
    ///   Chase — moves toward the player. Checks HP thresholds each tick.
    ///           When HP drops below a BossPhase.Threshold, activates that phase:
    ///           applies SpeedMultiplier, fires TransitionVfx.
    ///   Dead  — raises GameEvents.OnBossDefeated.
    ///
    /// Phase data is defined in BossData.Phases[] (ordered highest→lowest threshold).
    /// </summary>
    public class BossBrain : EnemyBrain
    {
        private BossData _bossData;
        private int _currentPhaseIndex = -1;
        private float _baseEffectiveSpeed;

        public override void Initialize(Transform player, int waveNumber, ObjectPool<EnemyBrain> pool, EnemyData overrideData = null)
        {
            _bossData = data as BossData;
            if (_bossData == null)
                Debug.LogWarning($"[BossBrain] EnemyData on {name} should be a BossData asset.", this);

            _currentPhaseIndex = -1;

            base.Initialize(player, waveNumber, pool, overrideData);
            _baseEffectiveSpeed = effectiveSpeed;

            if (_bossData?.IntroVfx != null)
                GameEvents.RaiseVfxRequested(_bossData.IntroVfx, transform.position);
        }

        protected override void EnterState(EnemyState newState)
        {
            // BossDefeated is raised by WaveSpawner.HandleEnemyDied (single source)
            // Do NOT raise it here — causes double XP/gold/save
        }

        protected override void Tick()
        {
            if (CurrentState != EnemyState.Chase) return;

            CheckPhaseTransition();
            ChasePlayer();
            TickContactDamage();
        }

        private void CheckPhaseTransition()
        {
            if (_bossData == null || _bossData.Phases == null || _bossData.Phases.Length == 0) return;
            if (health == null) return;

            float hpFraction = (float)health.CurrentHp / health.MaxHp;

            for (int i = 0; i < _bossData.Phases.Length; i++)
            {
                if (i <= _currentPhaseIndex) continue;

                var phase = _bossData.Phases[i];
                if (hpFraction <= phase.Threshold)
                {
                    ActivatePhase(i, phase);
                }
            }
        }

        private void ActivatePhase(int index, BossPhase phase)
        {
            _currentPhaseIndex = index;

            // Apply speed multiplier
            effectiveSpeed = _baseEffectiveSpeed * phase.SpeedMultiplier;

            // Fire transition VFX
            if (phase.TransitionVfx != null)
                GameEvents.RaiseVfxRequested(phase.TransitionVfx, transform.position);

            Debug.Log($"[BossBrain] Phase {index + 1} activated at {(phase.Threshold * 100f):F0}% HP. Speed x{phase.SpeedMultiplier}");
        }
    }
}
