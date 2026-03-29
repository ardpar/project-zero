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

        /// <summary>Active modifier for current level.</summary>
        public LevelModifier CurrentModifier { get; private set; }

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
            // If TrialManager is handling this run, defer to it
            int selectedChamber = PlayerPrefs.GetInt("SelectedChamber", 0);
            if (selectedChamber > 0) return;

            int selectedLevel = PlayerPrefs.GetInt("SelectedLevel", 1);
            StartLevel(selectedLevel);
        }

        private void OnBossDefeated()
        {
            // If TrialManager is handling this run, skip legacy flow
            int selectedChamber = PlayerPrefs.GetInt("SelectedChamber", 0);
            if (selectedChamber > 0) return;

            var ch = Synthborn.Core.Persistence.SaveManager.Character;
            if (ch != null)
            {
                ch.CompleteLevel(CurrentLevel);

                // Award XP based on level difficulty
                int xpReward = 50 + CurrentLevel * 10;
                int levelUps = ch.AddXP(xpReward);

                // Award gold bonus (with milestone multiplier)
                int goldBonus = 20 + CurrentLevel * 5;
                if (IsMilestoneLevel(CurrentLevel))
                    goldBonus *= 3; // 3x gold on milestone levels
                Synthborn.Core.Persistence.GoldManager.AddGold(goldBonus);
                ch.gold = Synthborn.Core.Persistence.GoldManager.RunGold;

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
            CurrentModifier = LevelModifier.RandomForLevel(levelNumber);

            CurrentLevelName = levelData.levelName;

            // Swap biome visuals
            SwapBiome(levelData);

            // Configure WaveSpawner for this level (with modifier)
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

            // S-curve difficulty: gentle early, steep mid, asymptotic late
            if (levelNumber > _levels.Length)
            {
                float n = levelNumber - _levels.Length;
                // Learning (1-15): +15%/level, Mastery (16-55): +25%/level, Endgame (56+): logarithmic
                float extraScale;
                if (n <= 15)
                    extraScale = 1f + n * 0.15f;
                else if (n <= 55)
                    extraScale = 1f + 15 * 0.15f + (n - 15) * 0.25f;
                else
                    extraScale = 1f + 15 * 0.15f + 40 * 0.25f + Mathf.Log(n - 54) * 2f;
                difficulty *= extraScale;
            }

            // Apply modifier effects
            float spawnRateMultiplier = 1f;
            float eliteMultiplier = 1f;
            if (CurrentModifier != null)
            {
                switch (CurrentModifier.type)
                {
                    case ModifierType.Enraged: difficulty *= 0.75f; spawnRateMultiplier = 0.8f; break; // less HP but faster
                    case ModifierType.ArmoredTide: difficulty *= 1.5f; spawnRateMultiplier = 1.3f; break; // tankier, slower spawns
                    case ModifierType.EliteSurge: eliteMultiplier = 2f; break;
                    case ModifierType.Swarm: spawnRateMultiplier = 0.5f; difficulty *= 0.6f; break; // many weak enemies
                    case ModifierType.GoldFever: difficulty *= 1.3f; break;
                    case ModifierType.FogOfWar: spawnRateMultiplier = 0.9f; break;
                }
            }

            // Create wave definitions for this level
            var waves = new WaveDefinition[level.waveCount];
            for (int i = 0; i < level.waveCount; i++)
            {
                float interval = Mathf.Max(
                    (level.baseSpawnInterval - i * level.intervalDecreasePerWave) * spawnRateMultiplier,
                    0.3f);

                int elites = Mathf.Min(
                    Mathf.RoundToInt((level.baseElites + i) * eliteMultiplier),
                    level.maxElites * 2);

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

            // Fallback: should not reach here, but guard against crash
            Debug.LogError("[LevelManager] No levels configured!");
            return ScriptableObject.CreateInstance<LevelData>();
        }

        /// <summary>Check if a level is a milestone (bonus rewards).</summary>
        public static bool IsMilestoneLevel(int level) =>
            level == 10 || level == 25 || level == 50 || level == 75 || level == 100
            || (level > 0 && level % 20 == 0);

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
