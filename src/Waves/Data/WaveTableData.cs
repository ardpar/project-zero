using UnityEngine;
using Synthborn.Enemies;

namespace Synthborn.Waves
{
    /// <summary>ScriptableObject containing the full wave table for a biome.</summary>
    [CreateAssetMenu(menuName = "Synthborn/Waves/WaveTableData")]
    public class WaveTableData : ScriptableObject
    {
        [Tooltip("Ordered wave definitions. Index = wave number (0-based).")]
        public WaveDefinition[] waves;

        [Tooltip("Seconds of rest between waves.")]
        public float waveBreakDuration = 3f;

        [Tooltip("Maximum enemies alive simultaneously.")]
        public int maxAliveEnemies = 200;

        [Tooltip("Minimum spawn distance from player (units).")]
        public float minSpawnDistance = 12f;

        [Header("Boss Phase")]
        [Tooltip("Boss enemy data.")]
        public EnemyData bossData;

        [Tooltip("Chaser data spawned during boss fight.")]
        public EnemyData bossChaserData;

        [Tooltip("Chaser spawn interval during boss fight (seconds).")]
        public float bossChaserSpawnInterval = 3f;

        [Tooltip("Delay before boss activates after cinematic entry (seconds).")]
        public float bossIntroDelay = 2f;
    }
}
