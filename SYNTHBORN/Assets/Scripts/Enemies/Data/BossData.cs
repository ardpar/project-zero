using System;
using UnityEngine;

namespace Synthborn.Enemies
{
    /// <summary>
    /// EnemyData extension for Boss enemies.
    /// MVP: large, slow Chaser with high HP (10x) and high contact damage.
    /// Vertical Slice+: phases array drives HP-threshold behaviour changes.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBossData", menuName = "Synthborn/Enemies/Boss Data")]
    public class BossData : EnemyData
    {
        /// <summary>
        /// HP threshold phases, evaluated in order from highest to lowest.
        /// Vertical Slice+: BossBrain activates the first phase whose Threshold
        /// the boss HP has dropped below. MVP: array can be left empty.
        /// </summary>
        [field: SerializeField, Tooltip("HP% thresholds for phase transitions (0.75, 0.50, 0.25). Leave empty for MVP.")]
        public BossPhase[] Phases { get; private set; } = Array.Empty<BossPhase>();
        /// VFX prefab played during the boss cinematic entrance (2 s slow entrance).
        /// Optional — null disables the entrance effect.
        [field: SerializeField, Tooltip("Boss entrance VFX prefab. Optional.")]
        public GameObject IntroVfx { get; private set; }
    }
    /// Describes one phase of a boss fight, unlocked when HP falls below
    /// the specified threshold.
    [Serializable]
    public struct BossPhase
        /// <summary>HP fraction [0–1] that triggers this phase (e.g. 0.5 = 50% HP).</summary>
        [Tooltip("HP fraction that triggers this phase (e.g. 0.5 = 50% HP).")]
        public float Threshold;
        /// <summary>Speed multiplier applied to MoveSpeed when this phase activates.</summary>
        [Tooltip("Move speed multiplier for this phase (e.g. 1.5 = 50% faster).")]
        public float SpeedMultiplier;
        /// <summary>Optional VFX prefab to play at the phase transition point.</summary>
        [Tooltip("Optional VFX to play at phase transition.")]
        public GameObject TransitionVfx;
}
