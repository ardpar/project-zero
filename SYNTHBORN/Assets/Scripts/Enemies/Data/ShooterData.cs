using UnityEngine;
using Synthborn.Combat.Projectile;

namespace Synthborn.Enemies
{
    /// <summary>
    /// EnemyData extension for the Shooter enemy type.
    /// Adds range, fire-rate, and projectile definition on top of the base stats.
    /// Default values: shoot_range 6.0, shoot_interval 2.0 s (GDD tuning knobs).
    /// </summary>
    [CreateAssetMenu(fileName = "NewShooterData", menuName = "Synthborn/Enemies/Shooter Data")]
    public class ShooterData : EnemyData
    {
        /// <summary>
        /// Distance (world units) at which the shooter stops chasing and begins firing.
        /// Default 6.0. Safe range 4.0 – 8.0.
        /// </summary>
        [field: SerializeField, Tooltip("Stop-and-shoot distance from the player (units). Default 6.")]
        public float ShootRange { get; private set; } = 6f;

        /// <summary>
        /// Base seconds between shots.
        /// Scaled at runtime: shoot_interval * (1 - wave_number * 0.02), min 0.5 s.
        /// Default 2.0. Safe range 1.0 – 3.0.
        /// </summary>
        [field: SerializeField, Tooltip("Base seconds between shots (wave-scaled down to 0.5 s min).")]
        public float ShootInterval { get; private set; } = 2f;

        /// <summary>
        /// Projectile definition (speed, damage, on-hit behaviour) used when firing.
        /// Must be assigned — ShooterBrain will log an error if null.
        /// </summary>
        [field: SerializeField, Tooltip("Projectile ScriptableObject fired by this shooter.")]
        public ProjectileData ProjectileData { get; private set; }
    }
}
