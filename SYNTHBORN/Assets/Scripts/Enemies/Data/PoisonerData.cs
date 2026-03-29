using UnityEngine;

namespace Synthborn.Enemies
{
    /// <summary>
    /// EnemyData extension for the Poisoner enemy type.
    /// Adds poison trail interval, trail duration, and trail damage.
    /// </summary>
    [CreateAssetMenu(fileName = "NewPoisonerData", menuName = "Synthborn/Enemies/Poisoner Data")]
    public class PoisonerData : EnemyData
    {
        /// <summary>Seconds between poison trail drops.</summary>
        [field: SerializeField, Tooltip("Interval between trail drops. Default 0.5s.")]
        public float TrailInterval { get; private set; } = 0.5f;

        /// <summary>How long each trail segment persists.</summary>
        [field: SerializeField, Tooltip("Trail segment lifetime. Default 3.0s.")]
        public float TrailDuration { get; private set; } = 3f;

        /// <summary>Damage per second dealt to player standing on trail.</summary>
        [field: SerializeField, Tooltip("DPS to player on trail. Default 5.")]
        public int TrailDamagePerSecond { get; private set; } = 5;

        /// <summary>Radius of each trail segment.</summary>
        [field: SerializeField, Tooltip("Trail segment radius. Default 0.5.")]
        public float TrailRadius { get; private set; } = 0.5f;

        /// <summary>Prefab for the poison trail segment (pooled).</summary>
        [field: SerializeField, Tooltip("Poison trail segment prefab.")]
        public GameObject TrailPrefab { get; private set; }
    }
}
