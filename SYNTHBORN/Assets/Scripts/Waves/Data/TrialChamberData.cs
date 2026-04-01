using UnityEngine;

namespace Synthborn.Waves
{
    /// <summary>
    /// Defines a single Trial Chamber (room) in the Arena.
    /// Extends the concept of LevelData with biome layering,
    /// pressure scaling, and adjacency for map unlocking.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Waves/TrialChamberData")]
    public class TrialChamberData : LevelData
    {
        [Header("Trial Chamber")]
        [Tooltip("Chamber number in the Arena map (1-100).")]
        [Range(1, 100)]
        public int chamberNumber = 1;

        [Tooltip("Biome layer this chamber belongs to.")]
        public BiomeLayer biomeLayer = BiomeLayer.Atrium;

        [Tooltip("Pressure rating (1-5). Higher = harder enemies.")]
        [Range(1, 5)]
        public int pressureRating = 1;

        [Tooltip("Chamber numbers that unlock when this chamber is completed.")]
        public int[] adjacentChambers = new int[0];

        [Header("Boss Room")]
        [Tooltip("True if this is a biome boss room (rooms 16, 33, 50, 67, 84, 100). Triggers special music/rewards.")]
        public bool isBossRoom;

        [Header("Pressure Scaling")]
        [Tooltip("Override pressure multiplier. 0 = use default formula.")]
        public float pressureMultiplierOverride;

        /// <summary>
        /// Effective pressure multiplier for enemy stat scaling.
        /// Formula: base * (1 + chamberNumber * 0.3) unless overridden.
        /// </summary>
        public float EffectivePressureMultiplier =>
            pressureMultiplierOverride > 0f
                ? pressureMultiplierOverride
                : 1f + chamberNumber * 0.03f;
    }
}
