using UnityEngine;

namespace Synthborn.Enemies
{
    /// <summary>
    /// Global scaling parameters applied to all enemies each wave.
    /// Referenced by EnemyBrain (speed cap) and WaveSpawner (HP calculation).
    /// All defaults match GDD tuning-knob table.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyScalingConfig", menuName = "Synthborn/Enemies/Enemy Scaling Config")]
    public class EnemyScalingConfig : ScriptableObject
    {
        // ------------------------------------------------------------------ //
        // HP Scaling
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Fractional HP increase per wave number.
        /// Formula: base_hp * tier_mult * (1 + wave * waveHpScale).
        /// Default 0.15 = +15%/wave. Safe range 0.10 – 0.30.
        /// Clamped at 0.30 to prevent HP-sponge enemies.
        /// </summary>
        [field: SerializeField, Range(0.10f, 0.30f),
            Tooltip("Fractional HP increase per wave (0.15 = +15%/wave). Clamped 0.10–0.30.")]
        public float WaveHpScale { get; private set; } = 0.15f;

        // ------------------------------------------------------------------ //
        // Tier HP Multipliers  (Entity Health formula)
        // ------------------------------------------------------------------ //

        /// <summary>HP multiplier for Normal tier (= 1x, effectively the base).</summary>
        [field: SerializeField, Tooltip("HP multiplier for Normal tier. Default 1.0.")]
        public float NormalHpMultiplier { get; private set; } = 1f;

        /// <summary>HP multiplier for Elite tier. Default 3x (GDD: 2.0–5.0 safe range).</summary>
        [field: SerializeField, Range(2f, 5f), Tooltip("HP multiplier for Elite tier. Default 3.")]
        public float EliteHpMultiplier { get; private set; } = 3f;

        /// <summary>HP multiplier for Boss tier. Default 10x (GDD: 5.0–20.0 safe range).</summary>
        [field: SerializeField, Range(5f, 20f), Tooltip("HP multiplier for Boss tier. Default 10.")]
        public float BossHpMultiplier { get; private set; } = 10f;

        // ------------------------------------------------------------------ //
        // XP Tier Multipliers  (XP GDD)
        // ------------------------------------------------------------------ //

        /// <summary>XP multiplier for Normal tier. Always 1.</summary>
        [field: SerializeField, Tooltip("XP multiplier for Normal tier. Default 1.")]
        public float NormalXpMultiplier { get; private set; } = 1f;

        /// <summary>XP multiplier for Elite tier. Default 5.</summary>
        [field: SerializeField, Tooltip("XP multiplier for Elite tier. Default 5.")]
        public float EliteXpMultiplier { get; private set; } = 5f;

        /// <summary>XP multiplier for Boss tier. Default 20.</summary>
        [field: SerializeField, Tooltip("XP multiplier for Boss tier. Default 20.")]
        public float BossXpMultiplier { get; private set; } = 20f;

        // ------------------------------------------------------------------ //
        // Speed Cap
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Hard cap on effective enemy speed as a fraction of player speed.
        /// Default 0.9 = enemies are always slower than the player.
        /// EnemyBrain reads the player's current speed and clamps to this ratio.
        /// </summary>
        [field: SerializeField, Range(0.5f, 1f),
            Tooltip("Max enemy speed as fraction of player speed. Default 0.9.")]
        public float SpeedCapFraction { get; private set; } = 0.9f;

        // ------------------------------------------------------------------ //
        // Helpers
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns the HP tier multiplier for the given <see cref="EnemyTier"/>.
        /// </summary>
        public float GetHpMultiplier(EnemyTier tier)
        {
            return tier switch
            {
                EnemyTier.Elite => EliteHpMultiplier,
                EnemyTier.Boss  => BossHpMultiplier,
                _               => NormalHpMultiplier,
            };
        }

        /// <summary>
        /// Returns the XP tier multiplier for the given <see cref="EnemyTier"/>.
        /// </summary>
        public float GetXpMultiplier(EnemyTier tier)
        {
            return tier switch
            {
                EnemyTier.Elite => EliteXpMultiplier,
                EnemyTier.Boss  => BossXpMultiplier,
                _               => NormalXpMultiplier,
            };
        }
    }
}
