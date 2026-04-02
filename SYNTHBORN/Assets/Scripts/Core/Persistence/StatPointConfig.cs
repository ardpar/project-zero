using UnityEngine;

namespace Synthborn.Core.Persistence
{
    /// <summary>
    /// Per-stat-point multipliers for the meta-progression stat system.
    /// Used by GameBootstrap to apply persistent stat points from CharacterSaveData.
    /// Separate from AdaptationConfig which is run-scoped.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Progression/Stat Point Config", fileName = "StatPointConfig")]
    public class StatPointConfig : ScriptableObject
    {
        [Header("Per-Point Multipliers (Meta-Progression)")]
        [Tooltip("Damage modifier per MASS point. Default 0.02 = +2%/point.")]
        [Range(0.01f, 0.10f)]
        public float massMultiplier = 0.02f;

        [Tooltip("HP modifier per RESILIENCE point. Default 0.03 = +3%/point.")]
        [Range(0.01f, 0.10f)]
        public float resilienceMultiplier = 0.03f;

        [Tooltip("Speed modifier per VELOCITY point. Default 0.02 = +2%/point.")]
        [Range(0.01f, 0.10f)]
        public float velocityMultiplier = 0.02f;

        [Tooltip("Crit chance per VARIANCE point. Default 0.01 = +1%/point.")]
        [Range(0.005f, 0.05f)]
        public float varianceMultiplier = 0.01f;

        [Tooltip("XP gain multiplier per YIELD point. Default 0.05 = +5%/point.")]
        [Range(0.01f, 0.10f)]
        public float yieldMultiplier = 0.05f;
    }
}
