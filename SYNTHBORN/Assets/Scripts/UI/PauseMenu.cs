using UnityEngine;
using UnityEngine.SceneManagement;
using Synthborn.Core.Events;

namespace Synthborn.UI
{
    /// <summary>
    /// Pause menu toggled by ESC. Resume, Main Menu, Quit buttons.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _pausePanel;

        private bool _isPaused;

        private void Start()
        {
            if (_pausePanel != null)
                _pausePanel.SetActive(false);
        }

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (_isPaused) Resume();
                else Pause();
            }
        }

        public void Pause()
        {
            _isPaused = true;
            Time.timeScale = 0f;
            GameEvents.RaiseGamePaused();
            if (_pausePanel != null) _pausePanel.SetActive(true);
        }

        public void Resume()
        {
            _isPaused = false;
            Time.timeScale = 1f;
            GameEvents.RaiseGameResumed();
            if (_pausePanel != null) _pausePanel.SetActive(false);
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            GameEvents.Cleanup();
            SceneManager.LoadScene("MainMenu");
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
