using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Synthborn.Core.Persistence;

namespace Synthborn.UI
{
    /// <summary>
    /// Settings panel: master/music/sfx volume sliders, shake toggle,
    /// fullscreen toggle, tutorial reset button.
    /// Reads/writes to SaveManager.Data.
    /// </summary>
    public class SettingsScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private Toggle _shakeToggle;
        [SerializeField] private Toggle _fullscreenToggle;
        [SerializeField] private Button _tutorialResetButton;
        [SerializeField] private Text _tutorialResetFeedback;

        private void Awake()
        {
            if (_tutorialResetButton != null)
                _tutorialResetButton.onClick.AddListener(ResetTutorial);
        }

        private void OnDestroy()
        {
            if (_tutorialResetButton != null)
                _tutorialResetButton.onClick.RemoveListener(ResetTutorial);
        }

        public void Show()
        {
            if (_panel == null) return;
            _panel.SetActive(true);
            LoadSettings();
            PopupEscHandler.Register(_panel, Hide);

            if (_masterVolumeSlider != null)
                EventSystem.current?.SetSelectedGameObject(_masterVolumeSlider.gameObject);
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            PopupEscHandler.Unregister();
            ApplyAndSave();
        }

        private void LoadSettings()
        {
            var data = SaveManager.Data;
            if (_masterVolumeSlider != null) _masterVolumeSlider.value = data.masterVolume;
            if (_musicVolumeSlider != null) _musicVolumeSlider.value = data.musicVolume;
            if (_sfxVolumeSlider != null) _sfxVolumeSlider.value = data.sfxVolume;
            if (_shakeToggle != null) _shakeToggle.isOn = data.screenShakeEnabled;
            if (_fullscreenToggle != null) _fullscreenToggle.isOn = data.fullscreen;
        }

        private void ApplyAndSave()
        {
            var data = SaveManager.Data;
            if (_masterVolumeSlider != null) data.masterVolume = _masterVolumeSlider.value;
            if (_musicVolumeSlider != null) data.musicVolume = _musicVolumeSlider.value;
            if (_sfxVolumeSlider != null) data.sfxVolume = _sfxVolumeSlider.value;
            if (_shakeToggle != null) data.screenShakeEnabled = _shakeToggle.isOn;
            if (_fullscreenToggle != null) data.fullscreen = _fullscreenToggle.isOn;

            AudioListener.volume = data.masterVolume;
            Screen.fullScreen = data.fullscreen;
            SaveManager.Save();
        }

        private void ResetTutorial()
        {
            SaveManager.Data.tutorialCompleted = false;
            SaveManager.Save();
            if (_tutorialResetFeedback != null)
                _tutorialResetFeedback.text = "Tutorial will show on next run";
        }

        /// <summary>Apply saved settings at game start.</summary>
        public static void ApplyOnStartup()
        {
            var data = SaveManager.Data;
            AudioListener.volume = data.masterVolume;
            Screen.fullScreen = data.fullscreen;
        }

        /// <summary>Current music volume from save data (0-1).</summary>
        public static float MusicVolume => SaveManager.Data.musicVolume;

        /// <summary>Current SFX volume from save data (0-1).</summary>
        public static float SfxVolume => SaveManager.Data.sfxVolume;
    }
}
