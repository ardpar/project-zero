using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Synthborn.Core.Events;
using Synthborn.Core.Persistence;

namespace Synthborn.Core
{
    /// <summary>
    /// Manages run lifecycle: playing, game over, victory.
    /// Shows end screen with stats and handles restart/menu navigation.
    /// </summary>
    public class RunManager : MonoBehaviour
    {
        [Header("End Panels")]
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private GameObject _victoryPanel;

        [Header("Stats Display (shared between panels)")]
        [SerializeField] private Text _statsText;

        private RunStats _stats;

        private enum RunState { Playing, GameOver, Victory }
        private RunState _state = RunState.Playing;

        private void Awake()
        {
            _stats = GetComponent<RunStats>();
            if (_stats == null)
                _stats = gameObject.AddComponent<RunStats>();
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerDied += OnPlayerDied;
            GameEvents.OnBossDefeated += OnBossDefeated;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDied -= OnPlayerDied;
            GameEvents.OnBossDefeated -= OnBossDefeated;
        }

        private void Start()
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_victoryPanel != null) _victoryPanel.SetActive(false);
        }

        private void OnPlayerDied()
        {
            if (_state != RunState.Playing) return;
            _state = RunState.GameOver;
            ShowEndScreen(_gameOverPanel);
        }

        private void OnBossDefeated()
        {
            if (_state != RunState.Playing) return;
            _state = RunState.Victory;
            ShowEndScreen(_victoryPanel);
        }

        private void ShowEndScreen(GameObject panel)
        {
            Time.timeScale = 0f;
            if (panel != null) panel.SetActive(true);

            bool victory = _state == RunState.Victory;
            int cellsEarned = UnlockManager.CalculateRunReward(
                _stats.EnemiesKilled, _stats.WavesCleared, victory);
            UnlockManager.AddCells(cellsEarned);
            UnlockManager.RecordRun(_stats.SurvivalTime, _stats.WavesCleared,
                _stats.EnemiesKilled, _stats.FinalLevel, _stats.MutationsAcquired,
                cellsEarned, victory);

            int achBefore = AchievementManager.UnlockedCount;
            AchievementManager.CheckRunEnd(
                _stats.SurvivalTime, _stats.EnemiesKilled, _stats.WavesCleared,
                _stats.MutationsAcquired, _stats.SynergiesTriggered,
                _stats.TotalXPCollected, victory);
            int newAchievements = AchievementManager.UnlockedCount - achBefore;

            UpdateStatsDisplay(cellsEarned, newAchievements);
        }

        private void UpdateStatsDisplay(int cellsEarned, int newAchievements = 0)
        {
            if (_statsText == null || _stats == null) return;

            int minutes = Mathf.FloorToInt(_stats.SurvivalTime / 60f);
            int seconds = Mathf.FloorToInt(_stats.SurvivalTime % 60f);

            string achText = newAchievements > 0
                ? $"\n<color=#FFD700>+{newAchievements} Achievement{(newAchievements > 1 ? "s" : "")}!</color>"
                : "";

            // Get current map level from PlayerPrefs (set by WorldMapScreen)
            int mapLevel = PlayerPrefs.GetInt("SelectedLevel", 0);
            string levelInfo = mapLevel > 0 ? $"Map Level: {mapLevel}\n" : "";

            _statsText.text =
                $"{levelInfo}" +
                $"Time: {minutes:00}:{seconds:00}\n" +
                $"Kills: {_stats.EnemiesKilled}\n" +
                $"Player Lv: {_stats.FinalLevel}\n" +
                $"Mutations: {_stats.MutationsAcquired}\n" +
                $"Synergies: {_stats.SynergiesTriggered}\n" +
                $"Waves: {_stats.WavesCleared}\n\n" +
                $"<color=yellow>+{cellsEarned} Cells</color>" +
                achText;
        }

        public void RestartRun()
        {
            Time.timeScale = 1f;
            GameEvents.Cleanup();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            GameEvents.Cleanup();
            // Return to WorldMap if character is loaded, otherwise MainMenu
            if (Persistence.SaveManager.Character != null)
                SceneManager.LoadScene("WorldMap");
            else
                SceneManager.LoadScene("MainMenu");
        }
    }
}
