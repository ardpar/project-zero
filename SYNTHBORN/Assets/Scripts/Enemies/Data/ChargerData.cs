using UnityEngine;

namespace Synthborn.Enemies
{
    /// <summary>
    /// EnemyData extension for the Charger enemy type.
    /// Adds charge range, wind-up duration, dash speed, and cooldown.
    /// </summary>
    [CreateAssetMenu(fileName = "NewChargerData", menuName = "Synthborn/Enemies/Charger Data")]
    public class ChargerData : EnemyData
    {
        /// <summary>Distance to player that triggers a charge wind-up.</summary>
        [field: SerializeField, Tooltip("Distance to trigger charge. Default 8.")]
        public float ChargeRange { get; private set; } = 8f;

        /// <summary>Seconds spent winding up before the dash (warning window).</summary>
        [field: SerializeField, Tooltip("Wind-up time before dash. Default 0.8s.")]
        public float WindUpDuration { get; private set; } = 0.8f;

        /// <summary>Speed multiplier during dash (relative to base MoveSpeed).</summary>
        [field: SerializeField, Tooltip("Dash speed multiplier. Default 4x.")]
        public float DashSpeedMultiplier { get; private set; } = 4f;

        /// <summary>Duration of the dash in seconds.</summary>
        [field: SerializeField, Tooltip("Dash duration. Default 0.4s.")]
        public float DashDuration { get; private set; } = 0.4f;

        /// <summary>Cooldown between charges.</summary>
        [field: SerializeField, Tooltip("Seconds between charges. Default 3.0.")]
        public float ChargeCooldown { get; private set; } = 3f;

        /// <summary>Damage dealt during dash (replaces contact damage).</summary>
        [field: SerializeField, Tooltip("Damage dealt on dash hit. Default 15.")]
        public int DashDamage { get; private set; } = 15;

        /// <summary>Warning prefab shown during wind-up.</summary>
        [field: SerializeField, Tooltip("Warning indicator prefab.")]
        public GameObject WarningPrefab { get; private set; }
    }
}
