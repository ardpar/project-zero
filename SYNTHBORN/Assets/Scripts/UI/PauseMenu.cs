using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Synthborn.Core.Events;

namespace Synthborn.UI
{
    /// <summary>
    /// Pause menu toggled by ESC or gamepad Start button.
    /// Resume, Main Menu, Quit buttons.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private SettingsScreen _settingsScreen;

        /// <summary>Open settings from pause menu.</summary>
        public void OpenSettings()
        {
            _settingsScreen?.Show();
        }

        private bool _isPaused;

        private void Start()
        {
            if (_pausePanel != null)
                _pausePanel.SetActive(false);
        }

        private void Update()
        {
            bool escPressed = Keyboard.current != null &&
                              Keyboard.current.escapeKey.wasPressedThisFrame;
            bool startPressed = Gamepad.current != null &&
                                Gamepad.current.startButton.wasPressedThisFrame;

            if (escPressed || startPressed)
            {
                // Don't toggle pause if a popup is being closed by the same ESC press
                if (PopupEscHandler.IsActive) return;

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

            // Select resume button for gamepad navigation
            if (_resumeButton != null)
                EventSystem.current?.SetSelectedGameObject(_resumeButton.gameObject);
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
            SceneFader.LoadScene("MainMenu");
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
