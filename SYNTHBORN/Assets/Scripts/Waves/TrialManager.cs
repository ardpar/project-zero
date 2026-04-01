using UnityEngine;
using UnityEngine.SceneManagement;
using Synthborn.Core.Events;
using Synthborn.Core.Persistence;
using Synthborn.Enemies;

namespace Synthborn.Waves
{
    /// <summary>
    /// Orchestrates multi-room Trial Chamber runs.
    /// Replaces LevelManager's boss-defeat-ends-run flow with:
    /// Boss defeated -> Calibration Interval -> Next chamber or return to map.
    /// </summary>
    public class TrialManager : MonoBehaviour
    {
        [Header("Chamber Definitions")]
        [SerializeField] private TrialChamberData[] _chambers;

        [Header("Biome Configs")]
        [SerializeField] private BiomeConfig[] _biomeConfigs;

        [Header("Scene References")]
        [SerializeField] private WaveSpawner _waveSpawner;
        [SerializeField] private UnityEngine.Tilemaps.Tilemap _floorTilemap;
        [SerializeField] private UnityEngine.Tilemaps.Tilemap _wallTilemap;

        /// <summary>Current chamber being played.</summary>
        public TrialChamberData CurrentChamber { get; private set; }

        /// <summary>Current biome config for visuals.</summary>
        public BiomeConfig CurrentBiomeConfig { get; private set; }

        /// <summary>Run loot collected across all chambers this run.</summary>
        public System.Collections.Generic.List<string> RunLoot { get; } = new();

        /// <summary>Chambers completed this run (for multi-room continuation).</summary>
        public System.Collections.Generic.List<int> RunCompletedChambers { get; } = new();

        private bool _waitingForContinue;

        private void OnEnable()
        {
            GameEvents.OnBossDefeated += OnBossDefeated;
            GameEvents.OnLootDropped += OnLootDropped;
        }

        private void OnDisable()
        {
            GameEvents.OnBossDefeated -= OnBossDefeated;
            GameEvents.OnLootDropped -= OnLootDropped;
        }

        private void Start()
        {
            int selectedChamber = PlayerPrefs.GetInt("SelectedChamber", 0);

            // If SelectedChamber is set, use trial system; otherwise fall through to LevelManager
            if (selectedChamber > 0)
            {
                StartChamber(selectedChamber);
            }
        }

        /// <summary>Is the trial system active (vs legacy LevelManager)?</summary>
        public bool IsTrialActive => CurrentChamber != null;

        private void OnBossDefeated()
        {
            if (!IsTrialActive) return;

            var ch = SaveManager.Character;
            if (ch != null)
            {
                ch.CompleteChamber(CurrentChamber.chamberNumber, CurrentChamber.adjacentChambers);

                // Award XP based on pressure
                int xpReward = 50 + CurrentChamber.pressureRating * 20;
                ch.AddXP(xpReward);

                // Award gold/fragments based on pressure
                int goldBonus = 20 + CurrentChamber.pressureRating * 15;
                FragmentManager.AddFragments(goldBonus);
                ch.gold = FragmentManager.RunFragments;

                // Save adaptation points alongside character data
                var apm = FindAnyObjectByType<Synthborn.Progression.AdaptationPointManager>();
                if (apm != null) apm.SaveToCharacter();

                SaveManager.SaveSlot();
            }

            RunCompletedChambers.Add(CurrentChamber.chamberNumber);
            GameEvents.ChamberCleared(CurrentChamber.chamberNumber);

            _waitingForContinue = true;
            Time.timeScale = 0f;
            GameEvents.CalibrationIntervalStarted();
        }

        private void OnLootDropped(string itemId, string displayName, int rarity)
        {
            if (IsTrialActive)
                RunLoot.Add(itemId);
        }

        /// <summary>Called by CalibrationIntervalScreen to continue to next chamber.</summary>
        public void ContinueToNextChamber(int nextChamberNumber)
        {
            if (!_waitingForContinue) return;
            _waitingForContinue = false;
            Time.timeScale = 1f;

            // Fade transition if changing biomes
            var nextChamber = FindChamber(nextChamberNumber);
            if (nextChamber != null && CurrentChamber != null
                && nextChamber.biomeLayer != CurrentChamber.biomeLayer)
            {
                StartCoroutine(BiomeTransition(nextChamberNumber));
                return;
            }

            StartChamber(nextChamberNumber);
        }

        private System.Collections.IEnumerator BiomeTransition(int nextChamberNumber)
        {
            // Simple fade using CanvasGroup on a black overlay
            var fadeObj = new GameObject("BiomeFade", typeof(RectTransform),
                typeof(Canvas), typeof(UnityEngine.UI.Image));
            var canvas = fadeObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            var img = fadeObj.GetComponent<UnityEngine.UI.Image>();
            img.color = Color.clear;
            var rect = fadeObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;

            // Fade to black
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.unscaledDeltaTime;
                img.color = new Color(0, 0, 0, Mathf.Clamp01(t / 0.3f));
                yield return null;
            }

            StartChamber(nextChamberNumber);
            yield return null; // wait one frame

            // Fade from black
            t = 0f;
            while (t < 0.3f)
            {
                t += Time.unscaledDeltaTime;
                img.color = new Color(0, 0, 0, 1f - Mathf.Clamp01(t / 0.3f));
                yield return null;
            }

            Destroy(fadeObj);
        }

        /// <summary>Called by CalibrationIntervalScreen to return to Arena Map.</summary>
        public void ReturnToArenaMap()
        {
            if (!_waitingForContinue) return;
            _waitingForContinue = false;
            Time.timeScale = 1f;
            GameEvents.Cleanup();
            SceneManager.LoadScene("WorldMap");
        }

        private void StartChamber(int chamberNumber)
        {
            CurrentChamber = FindChamber(chamberNumber);
            if (CurrentChamber == null)
            {
                Debug.LogError($"[TrialManager] Chamber {chamberNumber} not found!");
                return;
            }

            CurrentBiomeConfig = FindBiomeConfig(CurrentChamber.biomeLayer);
            SwapBiome(CurrentChamber);
            ConfigureWaves(CurrentChamber);

            // Set biome music stems on AdaptiveMusicManager
            var musicManager = FindAnyObjectByType<Synthborn.Core.Audio.AdaptiveMusicManager>();
            if (musicManager != null && CurrentBiomeConfig != null)
            {
                musicManager.SetBiomeStems(
                    CurrentBiomeConfig.biomeCalmStem,
                    CurrentBiomeConfig.biomeCombatStem,
                    CurrentBiomeConfig.biomeBossStem);
            }

            GameEvents.ChamberStarted(chamberNumber);
        }

        private void SwapBiome(TrialChamberData chamber)
        {
            if (chamber.floorTile != null && _floorTilemap != null)
                SwapTiles(_floorTilemap, chamber.floorTile);
            if (chamber.wallTile != null && _wallTilemap != null)
                SwapTiles(_wallTilemap, chamber.wallTile);

            var cam = Camera.main;
            if (cam != null)
            {
                Color tint = CurrentBiomeConfig != null
                    ? CurrentBiomeConfig.backgroundTint
                    : chamber.biomeTint;
                cam.backgroundColor = tint;
            }
        }

        private void ConfigureWaves(TrialChamberData chamber)
        {
            if (_waveSpawner == null) return;

            float difficulty = chamber.difficultyMultiplier * chamber.EffectivePressureMultiplier;

            var waves = new WaveDefinition[chamber.waveCount];
            for (int i = 0; i < chamber.waveCount; i++)
            {
                float interval = Mathf.Max(
                    chamber.baseSpawnInterval - i * chamber.intervalDecreasePerWave,
                    0.3f);

                int elites = Mathf.Min(chamber.baseElites + i, chamber.maxElites);

                waves[i] = new WaveDefinition
                {
                    Duration = chamber.baseWaveDuration + i * 10f,
                    SpawnInterval = interval,
                    SpawnPool = chamber.spawnPool,
                    EliteCount = elites
                };
            }

            _waveSpawner.StartNewLevel(waves, chamber.bossData, chamber.bossChaserData, difficulty);
        }

        private TrialChamberData FindChamber(int chamberNumber)
        {
            if (_chambers == null) return null;
            foreach (var c in _chambers)
            {
                if (c != null && c.chamberNumber == chamberNumber)
                    return c;
            }
            return null;
        }

        private BiomeConfig FindBiomeConfig(BiomeLayer layer)
        {
            if (_biomeConfigs == null) return null;
            foreach (var bc in _biomeConfigs)
            {
                if (bc != null && bc.biomeLayer == layer)
                    return bc;
            }
            return null;
        }

        /// <summary>Get adjacent unlocked chambers that haven't been completed this run.</summary>
        public System.Collections.Generic.List<int> GetAvailableNextChambers()
        {
            var available = new System.Collections.Generic.List<int>();
            if (CurrentChamber?.adjacentChambers == null) return available;

            var ch = SaveManager.Character;
            foreach (int adj in CurrentChamber.adjacentChambers)
            {
                if (ch != null && ch.IsChamberUnlocked(adj) && !RunCompletedChambers.Contains(adj))
                    available.Add(adj);
            }
            return available;
        }

        private static void SwapTiles(UnityEngine.Tilemaps.Tilemap tilemap, UnityEngine.Tilemaps.TileBase newTile)
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
