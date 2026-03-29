using UnityEngine;
using Synthborn.Core;
using Synthborn.Core.Pool;
using Synthborn.Core.Events;
using Synthborn.Enemies;

namespace Synthborn.Waves
{
    /// <summary>
    /// Drives wave progression: spawns enemies on timer, tracks alive count,
    /// manages wave breaks, and triggers boss phase.
    /// </summary>
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private WaveTableData _waveTable;
        [SerializeField] private EnemyScalingConfig _scalingConfig;
        [SerializeField] private Transform _playerTransform;

        private ObjectPool<EnemyBrain> _enemyPool;

        /// <summary>Inject pool at runtime (called by GameBootstrap).</summary>
        public void SetPool(ObjectPool<EnemyBrain> pool) => _enemyPool = pool;

        private enum SpawnerState { WaveActive, WaveBreak, BossPhase, Complete, Paused }

        private SpawnerState _state;
        private int _currentWave;
        private float _waveTimer;
        private float _waveDuration; // cached for elite timing
        private float _spawnTimer;
        private float _breakTimer;
        private int _aliveCount;
        private int _spawnSector;
        private float _arenaRadius = 14f;
        private bool _elitesSpawned;
        private bool _miniBossSpawned;

        // Dynamic level override
        private WaveDefinition[] _dynamicWaves;
        private EnemyData _dynamicBossData;
        private EnemyData _dynamicBossChaserData;
        private float _difficultyMultiplier = 1f;

        /// <summary>Current wave number (1-based, for display).</summary>
        public int CurrentWaveDisplay => _currentWave + 1;

        /// <summary>Time remaining in current wave.</summary>
        public float WaveTimeRemaining => _waveTimer;

        private void OnEnable()
        {
            GameEvents.OnEnemyDied += HandleEnemyDied;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemyDied -= HandleEnemyDied;
        }

        private void Start()
        {
            DailySeedManager.Initialize();
            // Initial start is now handled by LevelManager.
            // If no LevelManager exists, start wave 0 as fallback.
            if (FindFirstObjectByType<LevelManager>() == null)
                StartWave(0);
        }

        /// <summary>
        /// Called by LevelManager to start a new level with dynamic wave definitions.
        /// Resets wave state and begins wave 0.
        /// </summary>
        public void StartNewLevel(WaveDefinition[] waves, EnemyData bossData, EnemyData bossChaserData, float difficultyMultiplier)
        {
            _dynamicWaves = waves;
            _dynamicBossData = bossData;
            _dynamicBossChaserData = bossChaserData;
            _difficultyMultiplier = difficultyMultiplier;
            _state = SpawnerState.WaveActive;
            StartWave(0);
        }

        private void Update()
        {
            if (Time.timeScale == 0f) return;

            float dt = Time.deltaTime;

            switch (_state)
            {
                case SpawnerState.WaveActive:
                    UpdateWaveActive(dt);
                    break;
                case SpawnerState.WaveBreak:
                    _breakTimer -= dt;
                    if (_breakTimer <= 0f)
                        StartWave(_currentWave + 1);
                    break;
                case SpawnerState.BossPhase:
                    UpdateBossPhase(dt);
                    break;
            }
        }

        private int MaxAliveEnemies => _waveTable != null ? _waveTable.maxAliveEnemies : 200;

        private void UpdateWaveActive(float dt)
        {
            if (_enemyPool == null) return;
            if (ActiveWaves == null) return;

            _waveTimer -= dt;
            _spawnTimer -= dt;

            if (_spawnTimer <= 0f && _aliveCount < MaxAliveEnemies)
            {
                SpawnEnemy();
                var wave = GetCurrentWaveDefinition();
                _spawnTimer = wave.SpawnInterval;
            }

            // Mini-boss spawn at 50% of wave duration (wave 4+, random chance)
            if (!_miniBossSpawned && _waveDuration > 0f && _waveTable != null && _waveTable.miniBossData != null)
            {
                float elapsed = _waveDuration - _waveTimer;
                if (elapsed >= _waveDuration * 0.5f && CurrentWaveDisplay >= _waveTable.miniBossStartWave)
                {
                    _miniBossSpawned = true;
                    if (Random.value < _waveTable.miniBossChance)
                    {
                        SpawnSpecificEnemy(_waveTable.miniBossData);
                        GameEvents.WaveStarted(CurrentWaveDisplay); // re-trigger wave UI for alert
                    }
                }
            }

            // Elite spawn at 80% of wave duration
            if (!_elitesSpawned && _waveDuration > 0f)
            {
                float elapsed = _waveDuration - _waveTimer;
                if (elapsed >= _waveDuration * 0.8f)
                {
                    _elitesSpawned = true;
                    var wave = GetCurrentWaveDefinition();
                    for (int i = 0; i < wave.EliteCount; i++)
                        SpawnEnemy(); // elites use same pool but wave-scaled HP handles tier
                }
            }

            if (_waveTimer <= 0f)
            {
                EndWave();
            }
        }

        private void UpdateBossPhase(float dt)
        {
            _spawnTimer -= dt;
            var chaser = _dynamicBossChaserData ?? (_waveTable != null ? _waveTable.bossChaserData : null);
            if (_spawnTimer <= 0f && _aliveCount < MaxAliveEnemies && chaser != null)
            {
                SpawnSpecificEnemy(chaser);
                _spawnTimer = _waveTable != null ? _waveTable.bossChaserSpawnInterval : 3f;
            }
        }

        private WaveDefinition[] ActiveWaves => _dynamicWaves ?? (_waveTable != null ? _waveTable.waves : null);

        private void StartWave(int waveIndex)
        {
            var waves = ActiveWaves;
            if (waves == null || waveIndex >= waves.Length)
            {
                StartBossPhase();
                return;
            }

            _currentWave = waveIndex;
            var wave = waves[_currentWave];
            _waveTimer = wave.Duration;
            _waveDuration = wave.Duration;
            _spawnTimer = 0f;
            _elitesSpawned = false;
            _miniBossSpawned = false;
            _state = SpawnerState.WaveActive;

            GameEvents.WaveStarted(CurrentWaveDisplay);
        }

        private void EndWave()
        {
            GameEvents.WaveCleared();
            _breakTimer = _waveTable != null ? _waveTable.waveBreakDuration : 3f;
            _state = SpawnerState.WaveBreak;
        }

        private void StartBossPhase()
        {
            _state = SpawnerState.BossPhase;
            _spawnTimer = _waveTable != null ? _waveTable.bossChaserSpawnInterval : 3f;

            var boss = _dynamicBossData ?? (_waveTable != null ? _waveTable.bossData : null);
            if (boss != null)
                SpawnSpecificEnemy(boss);

            GameEvents.BossSpawned();
        }

        private void SpawnEnemy()
        {
            var wave = GetCurrentWaveDefinition();
            if (wave.SpawnPool == null || wave.SpawnPool.Length == 0) return;

            // Weighted random selection
            float totalWeight = 0;
            foreach (var entry in wave.SpawnPool)
                totalWeight += entry.Weight;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0;
            EnemyData selectedData = wave.SpawnPool[0].EnemyData;

            foreach (var entry in wave.SpawnPool)
            {
                cumulative += entry.Weight;
                if (roll <= cumulative)
                {
                    selectedData = entry.EnemyData;
                    break;
                }
            }

            SpawnSpecificEnemy(selectedData);
        }

        private void SpawnSpecificEnemy(EnemyData data)
        {
            if (_enemyPool == null || data == null) return;

            Vector2 pos = GetSpawnPosition();
            var brain = _enemyPool.Get();
            brain.transform.position = pos;

            // Apply difficulty multiplier as effective wave scaling
            // Higher difficulty = enemies behave as if in a later wave (more HP, faster)
            int effectiveWave = Mathf.RoundToInt((_currentWave + 1) * _difficultyMultiplier);
            brain.Initialize(_playerTransform, effectiveWave, _enemyPool, data);
            _aliveCount++;
        }

        private Vector2 GetSpawnPosition()
        {
            if (_playerTransform == null) return Vector2.zero;

            Vector2 playerPos = _playerTransform.position;
            float minDist = _waveTable != null ? _waveTable.minSpawnDistance : 12f;

            for (int attempt = 0; attempt < 5; attempt++)
            {
                // Round-robin sectors for spread
                float sectorAngle = (_spawnSector * 45f + Random.Range(0f, 45f)) * Mathf.Deg2Rad;
                _spawnSector = (_spawnSector + 1) % 8;

                float dist = Random.Range(minDist, minDist + 3f);
                Vector2 pos = playerPos + new Vector2(Mathf.Cos(sectorAngle), Mathf.Sin(sectorAngle)) * dist;

                if (Mathf.Abs(pos.x) < _arenaRadius && Mathf.Abs(pos.y) < _arenaRadius)
                    return pos;
            }

            // Fallback
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            return playerPos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * minDist;
        }

        private WaveDefinition GetCurrentWaveDefinition()
        {
            var waves = ActiveWaves;
            int idx = Mathf.Min(_currentWave, waves.Length - 1);
            return waves[idx];
        }

        private void HandleEnemyDied(Vector2 pos, GameObject enemy, int xp)
        {
            _aliveCount = Mathf.Max(0, _aliveCount - 1);

            // Check if boss died via EnemyBrain data
            var brain = enemy != null ? enemy.GetComponent<EnemyBrain>() : null;
            if (brain != null && brain.Data != null && brain.Data.Tier == EnemyTier.Boss && _state == SpawnerState.BossPhase)
            {
                _state = SpawnerState.Paused; // Wait for LevelManager to start next level
                GameEvents.RaiseBossDefeated();
            }
        }
    }
}
