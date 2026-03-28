using UnityEngine;
using UnityEngine.SceneManagement;
using Synthborn.Core.Events;

namespace Synthborn.Core
{
    /// <summary>
    /// Manages run lifecycle: playing, game over, victory.
    /// Shows end screen and handles restart.
    /// </summary>
    public class RunManager : MonoBehaviour
    {
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private GameObject _victoryPanel;

        private enum RunState { Playing, GameOver, Victory }
        private RunState _state = RunState.Playing;

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
            Time.timeScale = 0f;
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
        }

        private void OnBossDefeated()
        {
            if (_state != RunState.Playing) return;
            _state = RunState.Victory;
            Time.timeScale = 0f;
            if (_victoryPanel != null) _victoryPanel.SetActive(true);
        }

        /// <summary>Called by restart button.</summary>
        public void RestartRun()
        {
            Time.timeScale = 1f;
            GameEvents.Cleanup();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
