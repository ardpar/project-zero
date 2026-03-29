using UnityEngine;
using UnityEngine.Tilemaps;
using Synthborn.Enemies;

namespace Synthborn.Waves
{
    /// <summary>
    /// Defines a single level/stage: biome visuals, enemy composition,
    /// boss type, and difficulty scaling. Each run progresses through
    /// multiple LevelData instances.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Waves/LevelData")]
    public class LevelData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Display name shown on HUD and transition screen.")]
        public string levelName = "Caverns";

        [Header("Biome Visuals")]
        [Tooltip("Floor tile for this level's biome.")]
        public TileBase floorTile;

        [Tooltip("Wall tile for this level's biome.")]
        public TileBase wallTile;

        [Tooltip("Camera background tint for atmosphere.")]
        public Color biomeTint = new Color(0.08f, 0.07f, 0.12f);

        [Header("Enemies")]
        [Tooltip("Weighted spawn pool for waves in this level.")]
        public SpawnEntry[] spawnPool;

        [Tooltip("Number of waves before the boss (default 5).")]
        public int waveCount = 5;

        [Tooltip("Base spawn interval for wave 1 of this level.")]
        public float baseSpawnInterval = 1.5f;

        [Tooltip("Spawn interval decrease per wave within this level.")]
        public float intervalDecreasePerWave = 0.15f;

        [Tooltip("Base wave duration in seconds.")]
        public float baseWaveDuration = 100f;

        [Header("Boss")]
        [Tooltip("Boss enemy data for this level's final fight.")]
        public EnemyData bossData;

        [Tooltip("Chaser enemy spawned during boss fight.")]
        public EnemyData bossChaserData;

        [Header("Difficulty")]
        [Tooltip("Multiplier applied to enemy HP/damage for this level (1.0 = base).")]
        public float difficultyMultiplier = 1.0f;

        [Tooltip("Elite count for wave N = baseElites + N (capped at maxElites).")]
        public int baseElites = 0;

        [Tooltip("Maximum elites per wave.")]
        public int maxElites = 5;
    }
}
