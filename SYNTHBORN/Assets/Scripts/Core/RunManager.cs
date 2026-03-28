using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Synthborn.Core.Events;

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
            UpdateStatsDisplay();
        }

        private void UpdateStatsDisplay()
        {
            if (_statsText == null || _stats == null) return;

            int minutes = Mathf.FloorToInt(_stats.SurvivalTime / 60f);
            int seconds = Mathf.FloorToInt(_stats.SurvivalTime % 60f);

            _statsText.text =
                $"Time: {minutes:00}:{seconds:00}\n" +
                $"Kills: {_stats.EnemiesKilled}\n" +
                $"XP: {_stats.TotalXPCollected}\n" +
                $"Level: {_stats.FinalLevel}\n" +
                $"Mutations: {_stats.MutationsAcquired}\n" +
                $"Synergies: {_stats.SynergiesTriggered}\n" +
                $"Waves: {_stats.WavesCleared}";
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
            SceneManager.LoadScene("MainMenu");
        }
    }
}
