using UnityEngine;
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

        private void UpdateWaveActive(float dt)
        {
            if (_waveTable == null || _enemyPool == null) return;

            _waveTimer -= dt;
            _spawnTimer -= dt;

            if (_spawnTimer <= 0f && _aliveCount < _waveTable.maxAliveEnemies)
            {
                SpawnEnemy();
                var wave = GetCurrentWaveDefinition();
                _spawnTimer = wave.SpawnInterval;
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
            if (_spawnTimer <= 0f && _aliveCount < _waveTable.maxAliveEnemies && _waveTable.bossChaserData != null)
            {
                SpawnSpecificEnemy(_waveTable.bossChaserData);
                _spawnTimer = _waveTable.bossChaserSpawnInterval;
            }
        }

        private void StartWave(int waveIndex)
        {
            if (waveIndex >= _waveTable.waves.Length)
            {
                StartBossPhase();
                return;
            }

            _currentWave = waveIndex;
            var wave = _waveTable.waves[_currentWave];
            _waveTimer = wave.Duration;
            _waveDuration = wave.Duration;
            _spawnTimer = 0f;
            _elitesSpawned = false;
            _state = SpawnerState.WaveActive;

            GameEvents.WaveStarted(CurrentWaveDisplay);
        }

        private void EndWave()
        {
            GameEvents.WaveCleared();
            _breakTimer = _waveTable.waveBreakDuration;
            _state = SpawnerState.WaveBreak;
        }

        private void StartBossPhase()
        {
            _state = SpawnerState.BossPhase;
            _spawnTimer = _waveTable.bossChaserSpawnInterval;

            if (_waveTable.bossData != null)
            {
                SpawnSpecificEnemy(_waveTable.bossData);
            }

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

            brain.Initialize(_playerTransform, _currentWave + 1, _enemyPool, data);
            _aliveCount++;
        }

        private Vector2 GetSpawnPosition()
        {
            if (_playerTransform == null) return Vector2.zero;

            Vector2 playerPos = _playerTransform.position;
            float minDist = _waveTable.minSpawnDistance;

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
            int idx = Mathf.Min(_currentWave, _waveTable.waves.Length - 1);
            return _waveTable.waves[idx];
        }

        private void HandleEnemyDied(Vector2 pos, GameObject enemy, int xp)
        {
            _aliveCount = Mathf.Max(0, _aliveCount - 1);

            // Check if boss died via EnemyBrain data
            var brain = enemy != null ? enemy.GetComponent<EnemyBrain>() : null;
            if (brain != null && brain.Data != null && brain.Data.Tier == EnemyTier.Boss && _state == SpawnerState.BossPhase)
            {
                _state = SpawnerState.Complete;
                GameEvents.RaiseBossDefeated();
            }
        }
    }
}
