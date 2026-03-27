// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is the core loop fun?
// Date: 2026-03-27

using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action<int> OnWaveStarted;
    public static event Action OnWaveCleared;
    public static event Action<Vector2, int> OnEnemyDied; // position, xpValue
    public static event Action<int> OnPlayerDamaged; // damage
    public static event Action OnPlayerDied;
    public static event Action OnLevelUp;
    public static event Action<MutationData> OnMutationSelected;
    public static event Action OnGamePaused;
    public static event Action OnGameResumed;

    public static void EnemyDied(Vector2 pos, int xp) => OnEnemyDied?.Invoke(pos, xp);
    public static void PlayerDamaged(int dmg) => OnPlayerDamaged?.Invoke(dmg);
    public static void PlayerDied() => OnPlayerDied?.Invoke();
    public static void LevelUp() => OnLevelUp?.Invoke();
    public static void MutationSelected(MutationData data) => OnMutationSelected?.Invoke(data);
    public static void WaveStarted(int wave) => OnWaveStarted?.Invoke(wave);
    public static void WaveCleared() => OnWaveCleared?.Invoke();
    public static void GamePaused() { OnGamePaused?.Invoke(); Time.timeScale = 0f; }
    public static void GameResumed() { OnGameResumed?.Invoke(); Time.timeScale = 1f; }

    public static void Cleanup()
    {
        OnWaveStarted = null;
        OnWaveCleared = null;
        OnEnemyDied = null;
        OnPlayerDamaged = null;
        OnPlayerDied = null;
        OnLevelUp = null;
        OnMutationSelected = null;
        OnGamePaused = null;
        OnGameResumed = null;
    }
}
