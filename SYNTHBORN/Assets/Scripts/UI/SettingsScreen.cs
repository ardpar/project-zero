using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Persistence;

namespace Synthborn.UI
{
    /// <summary>
    /// Settings panel: volume sliders, shake toggle, fullscreen toggle.
    /// Reads/writes to SaveManager.Data.
    /// </summary>
    public class SettingsScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private Toggle _shakeToggle;
        [SerializeField] private Toggle _fullscreenToggle;

        public void Show()
        {
            if (_panel == null) return;
            _panel.SetActive(true);
            LoadSettings();
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            ApplyAndSave();
        }

        private void LoadSettings()
        {
            var data = SaveManager.Data;
            if (_masterVolumeSlider != null) _masterVolumeSlider.value = data.masterVolume;
            if (_sfxVolumeSlider != null) _sfxVolumeSlider.value = data.sfxVolume;
            if (_shakeToggle != null) _shakeToggle.isOn = data.screenShakeEnabled;
            if (_fullscreenToggle != null) _fullscreenToggle.isOn = data.fullscreen;
        }

        private void ApplyAndSave()
        {
            var data = SaveManager.Data;
            if (_masterVolumeSlider != null) data.masterVolume = _masterVolumeSlider.value;
            if (_sfxVolumeSlider != null) data.sfxVolume = _sfxVolumeSlider.value;
            if (_shakeToggle != null) data.screenShakeEnabled = _shakeToggle.isOn;
            if (_fullscreenToggle != null) data.fullscreen = _fullscreenToggle.isOn;

            AudioListener.volume = data.masterVolume;
            Screen.fullScreen = data.fullscreen;
            SaveManager.Save();
        }

        /// <summary>Apply saved settings at game start.</summary>
        public static void ApplyOnStartup()
        {
            var data = SaveManager.Data;
            AudioListener.volume = data.masterVolume;
            Screen.fullScreen = data.fullscreen;
        }
    }
}
