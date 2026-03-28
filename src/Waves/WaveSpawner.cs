using UnityEngine;
using Synthborn.Core;
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
        [SerializeField] private ObjectPool<EnemyBrain> _enemyPool;
        [SerializeField] private EnemyScalingConfig _scalingConfig;
        [SerializeField] private Transform _playerTransform;

        private enum SpawnerState { WaveActive, WaveBreak, BossPhase, Complete, Paused }

        private SpawnerState _state;
        private int _currentWave;
        private float _waveTimer;
        private float _spawnTimer;
        private float _breakTimer;
        private int _aliveCount;
        private int _spawnSector;
        private float _arenaRadius = 14f;

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
            _waveTimer -= dt;
            _spawnTimer -= dt;

            if (_spawnTimer <= 0f && _aliveCount < _waveTable.maxAliveEnemies)
            {
                SpawnEnemy();
                var wave = GetCurrentWaveDefinition();
                _spawnTimer = wave.spawnInterval;
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
            _waveTimer = wave.duration;
            _spawnTimer = 0f;
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
            if (wave.spawnPool == null || wave.spawnPool.Length == 0) return;

            // Weighted random selection
            float totalWeight = 0;
            foreach (var entry in wave.spawnPool)
                totalWeight += entry.weight;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0;
            EnemyData selectedData = wave.spawnPool[0].enemyData;

            foreach (var entry in wave.spawnPool)
            {
                cumulative += entry.weight;
                if (roll <= cumulative)
                {
                    selectedData = entry.enemyData;
                    break;
                }
            }

            SpawnSpecificEnemy(selectedData);
        }

        private void SpawnSpecificEnemy(EnemyData data)
        {
            Vector2 pos = GetSpawnPosition();
            var brain = _enemyPool.Get(pos);

            // Calculate scaled HP
            float hpScale = 1f + _currentWave * _scalingConfig.waveHPScale;
            float tierMult = data.tier switch
            {
                EnemyTier.Elite => _scalingConfig.eliteTierMultiplier,
                EnemyTier.Boss => _scalingConfig.bossTierMultiplier,
                _ => _scalingConfig.normalTierMultiplier
            };
            int scaledHP = Mathf.RoundToInt(data.baseHP * tierMult * hpScale);
            float speedMult = 1f + _currentWave * data.speedScalePerWave;

            brain.Initialize(data, scaledHP, speedMult, _playerTransform);
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

        private void HandleEnemyDied(Vector2 pos, EnemyData data, int xp)
        {
            _aliveCount = Mathf.Max(0, _aliveCount - 1);

            // Check if boss died
            if (data.tier == EnemyTier.Boss && _state == SpawnerState.BossPhase)
            {
                _state = SpawnerState.Complete;
                GameEvents.BossDefeated();
            }
        }
    }
}
