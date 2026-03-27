// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is the core loop fun?
// Date: 2026-03-27

using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private float _arenaRadius = 14f;
    [SerializeField] private float _minSpawnDistance = 10f;
    [SerializeField] private int _maxAliveEnemies = 150;

    // Wave table (hardcoded for prototype)
    private readonly float[] _waveDurations = { 90, 90, 120, 120, 120, 150 };
    private readonly float[] _spawnIntervals = { 2f, 1.5f, 1.2f, 1f, 0.8f, 0.6f };
    private readonly int[] _baseEnemyHP = { 10, 12, 15, 18, 22, 28 };
    private readonly int[] _baseEnemyXP = { 3, 4, 5, 6, 7, 8 };

    private int _currentWave;
    private float _waveTimer;
    private float _spawnTimer;
    private int _aliveEnemies;
    private Transform _player;

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _player = player.transform;
        StartWave(0);
    }

    private void OnEnable()
    {
        GameEvents.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyDied -= HandleEnemyDied;
    }

    private void Update()
    {
        if (Time.timeScale == 0f || _player == null) return;

        _waveTimer -= Time.deltaTime;
        _spawnTimer -= Time.deltaTime;

        if (_spawnTimer <= 0f && _aliveEnemies < _maxAliveEnemies)
        {
            SpawnEnemy();
            _spawnTimer = GetCurrentSpawnInterval();
        }

        if (_waveTimer <= 0f)
        {
            NextWave();
        }
    }

    private void StartWave(int wave)
    {
        _currentWave = wave;
        _waveTimer = GetWaveDuration();
        _spawnTimer = 0f;
        GameManager.Instance.SetWave(wave + 1);
        GameEvents.WaveStarted(wave + 1);
        Debug.Log($"Wave {wave + 1} started — {_waveTimer}s, spawn interval: {GetCurrentSpawnInterval()}s");
    }

    private void NextWave()
    {
        if (_currentWave + 1 < _waveDurations.Length)
        {
            GameEvents.WaveCleared();
            StartWave(_currentWave + 1);
        }
        else
        {
            // Loop last wave for infinite survival
            _waveTimer = _waveDurations[^1];
            Debug.Log("Looping final wave...");
        }
    }

    private void SpawnEnemy()
    {
        Vector2 spawnPos = GetSpawnPosition();
        var go = ObjectPool.Instance.GetEnemy(spawnPos);

        int waveIdx = Mathf.Min(_currentWave, _baseEnemyHP.Length - 1);
        float hpScale = 1f + _currentWave * 0.15f;
        int hp = Mathf.RoundToInt(_baseEnemyHP[waveIdx] * hpScale);
        int xp = _baseEnemyXP[waveIdx];
        float speedMult = 1f + _currentWave * 0.03f;

        go.GetComponent<EnemyHealth>().Init(hp, xp);
        go.GetComponent<EnemyAI>().Init(speedMult);
        _aliveEnemies++;
    }

    private Vector2 GetSpawnPosition()
    {
        if (_player == null) return Vector2.zero;

        for (int i = 0; i < 10; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(_minSpawnDistance, _minSpawnDistance + 3f);
            Vector2 pos = (Vector2)_player.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

            if (Mathf.Abs(pos.x) < _arenaRadius && Mathf.Abs(pos.y) < _arenaRadius)
                return pos;
        }

        // Fallback: just spawn at edge
        float a = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        return (Vector2)_player.position + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * _minSpawnDistance;
    }

    private float GetWaveDuration() => _waveDurations[Mathf.Min(_currentWave, _waveDurations.Length - 1)];
    private float GetCurrentSpawnInterval() => _spawnIntervals[Mathf.Min(_currentWave, _spawnIntervals.Length - 1)];

    private void HandleEnemyDied(Vector2 pos, int xp)
    {
        _aliveEnemies = Mathf.Max(0, _aliveEnemies - 1);
    }
}
