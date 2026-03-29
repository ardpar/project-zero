using UnityEngine;
using UnityEngine.Tilemaps;
using Synthborn.Core.Events;
using Synthborn.Enemies;

namespace Synthborn.Waves
{
    /// <summary>
    /// Manages level/stage progression within a run.
    /// When the boss is defeated, transitions to the next level:
    /// swaps biome, resets WaveSpawner with new enemy pool, increases difficulty.
    /// Levels 1-N are defined by LevelData assets; beyond that, infinite scaling.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [Header("Level Definitions (ordered)")]
        [SerializeField] private LevelData[] _levels;

        [Header("Scene References")]
        [SerializeField] private WaveSpawner _waveSpawner;
        [SerializeField] private Tilemap _floorTilemap;
        [SerializeField] private Tilemap _wallTilemap;

        [Header("Infinite Scaling (beyond defined levels)")]
        [SerializeField] private float _scalingPerLevel = 0.3f;

        /// <summary>Current level number (1-based).</summary>
        public int CurrentLevel { get; private set; } = 1;

        /// <summary>Current level's display name.</summary>
        public string CurrentLevelName { get; private set; } = "";

        private bool _waitingForContinue;

        private void OnEnable()
        {
            GameEvents.OnBossDefeated += OnBossDefeated;
        }

        private void OnDisable()
        {
            GameEvents.OnBossDefeated -= OnBossDefeated;
        }

        private void Start()
        {
            int selectedLevel = PlayerPrefs.GetInt("SelectedLevel", 1);
            StartLevel(selectedLevel);
        }

        private void OnBossDefeated()
        {
            // Mark level as completed in save data
            var ch = Synthborn.Core.Persistence.SaveManager.Character;
            if (ch != null)
            {
                ch.CompleteLevel(CurrentLevel);
                Synthborn.Core.Persistence.SaveManager.SaveSlot();
            }

            GameEvents.LevelCleared(CurrentLevel);
            _waitingForContinue = true;
        }

        /// <summary>Called by LevelTransitionScreen to return to world map.</summary>
        public void ReturnToWorldMap()
        {
            if (!_waitingForContinue) return;
            _waitingForContinue = false;
            Time.timeScale = 1f;
            GameEvents.Cleanup();
            UnityEngine.SceneManagement.SceneManager.LoadScene("WorldMap");
        }

        private void StartLevel(int levelNumber)
        {
            CurrentLevel = levelNumber;
            var levelData = GetLevelData(levelNumber);

            CurrentLevelName = levelData.levelName;

            // Swap biome visuals
            SwapBiome(levelData);

            // Configure WaveSpawner for this level
            ConfigureWaves(levelData, levelNumber);

            // Fire event
            GameEvents.LevelStarted(levelNumber, levelData.levelName);
        }

        private void SwapBiome(LevelData level)
        {
            if (level.floorTile != null && _floorTilemap != null)
                SwapTiles(_floorTilemap, level.floorTile);
            if (level.wallTile != null && _wallTilemap != null)
                SwapTiles(_wallTilemap, level.wallTile);

            var cam = Camera.main;
            if (cam != null)
                cam.backgroundColor = level.biomeTint;
        }

        private void ConfigureWaves(LevelData level, int levelNumber)
        {
            if (_waveSpawner == null) return;

            // Build dynamic WaveTableData for this level
            float difficulty = level.difficultyMultiplier;

            // For infinite levels beyond defined data, scale difficulty
            if (levelNumber > _levels.Length)
            {
                float extraScale = 1f + (levelNumber - _levels.Length) * _scalingPerLevel;
                difficulty *= extraScale;
            }

            // Create wave definitions for this level
            var waves = new WaveDefinition[level.waveCount];
            for (int i = 0; i < level.waveCount; i++)
            {
                float interval = Mathf.Max(
                    level.baseSpawnInterval - i * level.intervalDecreasePerWave,
                    0.4f);

                int elites = Mathf.Min(level.baseElites + i, level.maxElites);

                waves[i] = new WaveDefinition
                {
                    Duration = level.baseWaveDuration + i * 10f,
                    SpawnInterval = interval,
                    SpawnPool = level.spawnPool,
                    EliteCount = elites
                };
            }

            // Apply to WaveSpawner via runtime config
            _waveSpawner.StartNewLevel(waves, level.bossData, level.bossChaserData, difficulty);
        }

        private LevelData GetLevelData(int levelNumber)
        {
            if (_levels != null && _levels.Length > 0)
            {
                int idx = Mathf.Min(levelNumber - 1, _levels.Length - 1);
                var baseLevel = _levels[idx];

                // For levels beyond defined data, randomize biome from pool
                if (levelNumber > _levels.Length)
                {
                    // Use random level data as base but scale difficulty
                    int randomIdx = Random.Range(0, _levels.Length);
                    return _levels[randomIdx];
                }

                return baseLevel;
            }

            // Fallback: return first level
            return _levels[0];
        }

        private static void SwapTiles(Tilemap tilemap, TileBase newTile)
        {
            if (tilemap == null || newTile == null) return;
            var bounds = tilemap.cellBounds;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    var pos = new Vector3Int(x, y, 0);
                    if (tilemap.HasTile(pos))
                        tilemap.SetTile(pos, newTile);
                }
            }
            tilemap.RefreshAllTiles();
        }
    }
}
