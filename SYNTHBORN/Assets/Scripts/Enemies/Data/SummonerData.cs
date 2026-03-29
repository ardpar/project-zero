using UnityEngine;

namespace Synthborn.Enemies
{
    /// <summary>
    /// EnemyData extension for the Summoner enemy type.
    /// Adds summon range, interval, max minions, and minion data reference.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSummonerData", menuName = "Synthborn/Enemies/Summoner Data")]
    public class SummonerData : EnemyData
    {
        /// <summary>Distance to player that triggers summoning state.</summary>
        [field: SerializeField, Tooltip("Distance to start summoning. Default 7.")]
        public float SummonRange { get; private set; } = 7f;

        /// <summary>Seconds between minion spawns.</summary>
        [field: SerializeField, Tooltip("Interval between summons. Default 3.0s.")]
        public float SummonInterval { get; private set; } = 3f;

        /// <summary>Maximum active minions at once.</summary>
        [field: SerializeField, Tooltip("Max concurrent minions. Default 4.")]
        public int MaxMinions { get; private set; } = 4;

        /// <summary>EnemyData for the spawned minions (typically a weak Chaser).</summary>
        [field: SerializeField, Tooltip("EnemyData asset for summoned minions.")]
        public EnemyData MinionData { get; private set; }
    }
}
