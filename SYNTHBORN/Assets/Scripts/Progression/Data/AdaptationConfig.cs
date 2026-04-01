using UnityEngine;

namespace Synthborn.Progression
{
    /// <summary>
    /// ScriptableObject defining per-point stat gains for each adaptation parameter.
    /// Each point allocated to a parameter adds these values to the CombatStatBlock.
    ///
    /// Parameters (Arena terminology):
    ///   MASS      — damage modifier per point
    ///   RESILIENCE — HP modifier per point
    ///   VELOCITY  — speed modifier per point
    ///   VARIANCE  — crit chance per point
    ///   YIELD     — XP gain multiplier per point
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Progression/Adaptation Config", fileName = "AdaptationConfig")]
    public class AdaptationConfig : ScriptableObject
    {
        [Header("MASS (Damage)")]
        [Tooltip("Damage modifier added per MASS point. Default 0.05 = +5% per point.")]
        [Range(0.01f, 0.15f)]
        public float massPerPoint = 0.05f;

        [Header("RESILIENCE (HP)")]
        [Tooltip("HP modifier added per RESILIENCE point. Default 0.06 = +6% per point.")]
        [Range(0.02f, 0.15f)]
        public float resiliencePerPoint = 0.06f;

        [Header("VELOCITY (Speed)")]
        [Tooltip("Speed modifier added per VELOCITY point. Default 0.04 = +4% per point.")]
        [Range(0.01f, 0.10f)]
        public float velocityPerPoint = 0.04f;

        [Header("VARIANCE (Crit)")]
        [Tooltip("Crit chance added per VARIANCE point. Default 0.02 = +2% per point.")]
        [Range(0.005f, 0.05f)]
        public float variancePerPoint = 0.02f;

        [Header("YIELD (XP Gain)")]
        [Tooltip("XP gain multiplier added per YIELD point. Default 0.08 = +8% per point.")]
        [Range(0.02f, 0.20f)]
        public float yieldPerPoint = 0.08f;

        [Header("Points Per Level")]
        [Tooltip("Number of adaptation points awarded per level-up. Default 1.")]
        [Range(1, 3)]
        public int pointsPerLevel = 1;
    }
}
