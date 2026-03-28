using UnityEngine;

namespace Synthborn.Enemies
{
    /// <summary>
    /// EnemyData extension for the Exploder enemy type.
    /// Adds prime duration, explosion radius/damage, and the warning-circle prefab.
    /// Default values from GDD: explode_range 1.5, prime_duration 1.0 s,
    /// explode_radius 2.0, explode_damage 20.
    /// </summary>
    [CreateAssetMenu(fileName = "NewExploderData", menuName = "Synthborn/Enemies/Exploder Data")]
    public class ExploderData : EnemyData
    {
        /// <summary>
        /// Distance (world units) from the player that triggers the priming state.
        /// Default 1.5. Safe range 1.0 – 2.5.
        /// </summary>
        [field: SerializeField, Tooltip("Distance to player that triggers Priming. Default 1.5.")]
        public float ExplodeRange { get; private set; } = 1.5f;

        /// <summary>
        /// Seconds the exploder spends in the Priming state before detonating.
        /// Default 1.0. Safe range 0.5 – 2.0.
        /// </summary>
        [field: SerializeField, Tooltip("Seconds in Priming before explosion. Default 1.0.")]
        public float PrimeDuration { get; private set; } = 1f;

        /// <summary>
        /// AoE blast radius (world units). Damage falls off 50% from centre to edge:
        /// explode_damage * (1 - (distance / explode_radius) * 0.5).
        /// Default 2.0. Safe range 1.0 – 3.0.
        /// </summary>
        [field: SerializeField, Tooltip("AoE explosion radius (units). Default 2.0.")]
        public float ExplodeRadius { get; private set; } = 2f;

        /// <summary>
        /// Base damage at the centre of the explosion.
        /// Edge damage = explode_damage * 0.5 (50% falloff at radius edge).
        /// </summary>
        [field: SerializeField, Tooltip("Damage at explosion centre. Edge = 50% of this value.")]
        public int ExplodeDamage { get; private set; } = 20;

        /// <summary>
        /// Warning-circle prefab shown on the ground during Priming.
        /// Instantiated by ExploderBrain and destroyed after detonation.
        /// </summary>
        [field: SerializeField, Tooltip("Ground warning circle prefab shown during Priming state.")]
        public GameObject WarningPrefab { get; private set; }
    }
}
