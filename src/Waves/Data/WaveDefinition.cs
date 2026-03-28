using System;
using UnityEngine;

namespace Synthborn.Waves
{
    /// <summary>
    /// Defines a single wave's composition, timing, and elite budget.
    /// Stored as an element of <see cref="WaveTableData.Waves"/>.
    /// All values match the GDD wave table defaults.
    /// </summary>
    [Serializable]
    public struct WaveDefinition
    {
        /// <summary>
        /// Total duration of this wave in seconds before the break phase.
        /// GDD defaults: 90 / 90 / 120 / 120 / 120 / 150 s.
        /// </summary>
        [Tooltip("Wave active duration in seconds."), Min(10f)]
        public float Duration;

        /// <summary>
        /// Seconds between individual enemy spawns during this wave.
        /// GDD defaults: 2.0 / 1.5 / 1.2 / 1.0 / 0.8 / 0.6 s.
        /// </summary>
        [Tooltip("Seconds between each spawn. GDD: 2.0 → 0.6 across waves."), Min(0.1f)]
        public float SpawnInterval;

        /// <summary>
        /// Weighted spawn pool for this wave.
        /// Entries are selected via weighted random each spawn event.
        /// </summary>
        [Tooltip("Weighted pool of enemy types available this wave.")]
        public SpawnEntry[] SpawnPool;

        /// <summary>
        /// Number of Elite-tier enemies to spawn toward the end of this wave
        /// (at EliteSpawnTimeRatio of wave duration — default 80%).
        /// GDD defaults: 0 / 0 / 1 / 2 / 3 / 4.
        /// </summary>
        [Tooltip("Number of elite enemies spawned at 80% of wave duration. GDD: 0,0,1,2,3,4."), Min(0)]
        public int EliteCount;
    }
}
