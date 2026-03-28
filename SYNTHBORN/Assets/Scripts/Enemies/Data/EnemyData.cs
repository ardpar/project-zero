using UnityEngine;

namespace Synthborn.Enemies
{
    /// <summary>
    /// Base ScriptableObject that defines all data for a single enemy type.
    /// Derived types (ShooterData, ExploderData, BossData) extend this with
    /// type-specific fields. All values are read-only at runtime — never mutate.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "Synthborn/Enemies/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        // ------------------------------------------------------------------ //
        // Identity
        // ------------------------------------------------------------------ //

        /// <summary>Unique string identifier used for pooling and analytics (e.g. "chaser_basic").</summary>
        [field: SerializeField, Tooltip("Unique string identifier used for pooling and analytics.")]
        public string Id { get; private set; } = "enemy_id";

        /// <summary>Power tier that drives HP and XP multipliers.</summary>
        [field: SerializeField, Tooltip("Normal=1x, Elite=3x HP/5x XP, Boss=10x HP/20x XP.")]
        public EnemyTier Tier { get; private set; } = EnemyTier.Normal;

        // ------------------------------------------------------------------ //
        // Combat Stats
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Base HP before tier and wave scaling.
        /// Scaled formula: base_hp * tier_multiplier * (1 + wave * wave_hp_scale).
        /// </summary>
        [field: SerializeField, Tooltip("Base HP before tier/wave scaling.")]
        public int BaseHp { get; private set; } = 10;

        /// <summary>
        /// Damage dealt to the player per contact interval (default 0.5 s).
        /// </summary>
        [field: SerializeField, Tooltip("Damage per contact hit applied to the player.")]
        public int ContactDamage { get; private set; } = 5;

        // ------------------------------------------------------------------ //
        // Movement
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Movement speed in world-units per second before wave scaling.
        /// Effective: base_speed * (1 + wave_number * speed_scale_per_wave).
        /// Hard cap: player_speed * 0.9 (applied by EnemyBrain).
        /// </summary>
        [field: SerializeField, Tooltip("Base move speed (units/sec) before wave scaling.")]
        public float MoveSpeed { get; private set; } = 2.5f;

        /// <summary>
        /// Per-wave fractional speed bonus applied by EnemyBrain.
        /// Default 0.03 = +3% per wave. Safe range 0.02 – 0.05.
        /// </summary>
        [field: SerializeField, Tooltip("Fractional speed increase per wave number (0.03 = +3%/wave).")]
        public float SpeedScalePerWave { get; private set; } = 0.03f;

        /// <summary>Physics hitbox radius used by the Collider2D on the prefab.</summary>
        [field: SerializeField, Tooltip("Collider radius for physics hitbox.")]
        public float HitboxRadius { get; private set; } = 0.4f;

        // ------------------------------------------------------------------ //
        // Rewards
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Base XP value yielded when this enemy dies.
        /// Gem value: base_xp * tier_multiplier (1 / 5 / 20).
        /// </summary>
        [field: SerializeField, Tooltip("Base XP before tier multiplier (1/5/20).")]
        public int BaseXp { get; private set; } = 3;

        /// <summary>
        /// Probability [0–1] this enemy drops an HP orb on death.
        /// Default 0.05 = 5%. Tuning knob; read by EntityHealth death handler.
        /// </summary>
        [field: SerializeField, Range(0f, 1f), Tooltip("Chance [0-1] to drop an HP orb on death.")]
        public float HpDropChance { get; private set; } = 0.05f;

        // ------------------------------------------------------------------ //
        // Visual / Audio
        // ------------------------------------------------------------------ //

        /// <summary>VFX prefab spawned at death position (pooled by VFX system).</summary>
        [field: SerializeField, Tooltip("Death VFX prefab.")]
        public GameObject DeathVfx { get; private set; }

        /// <summary>AudioClip played at death (handled by Audio system).</summary>
        [field: SerializeField, Tooltip("Death SFX clip.")]
        public AudioClip DeathSfx { get; private set; }
    }
}
