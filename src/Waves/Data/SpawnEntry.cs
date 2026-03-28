using System;
using UnityEngine;
using Synthborn.Enemies;

namespace Synthborn.Waves
{
    /// <summary>
    /// One entry in a wave's weighted spawn pool.
    /// Weight is relative — a weight of 3 spawns 3× more often than weight 1.
    /// </summary>
    [Serializable]
    public struct SpawnEntry
    {
        /// <summary>Enemy type to spawn. Must not be null.</summary>
        [Tooltip("Enemy data asset for this spawn pool entry.")]
        public EnemyData EnemyData;

        /// <summary>
        /// Relative weight for weighted-random selection.
        /// Example: Chaser 3, Runner 1 → Chaser spawns 75% of the time.
        /// </summary>
        [Tooltip("Relative spawn weight for this enemy type."), Min(1)]
        public int Weight;
    }
}
