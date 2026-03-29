using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Events;
using Synthborn.Core.Persistence;
using Synthborn.Waves;

namespace Synthborn.UI
{
    /// <summary>
    /// Shows level complete screen between levels.
    /// Displays gold earned, level reached, and continue button.
    /// Heals player 30% HP on display.
    /// </summary>
    public class LevelTransitionScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _infoText;
        [SerializeField] private Button _continueButton;
        [SerializeField] private LevelManager _levelManager;

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);
            if (_continueButton != null)
                _continueButton.onClick.AddListener(OnContinue);
        }

        private void OnDestroy()
        {
            if (_continueButton != null)
                _continueButton.onClick.RemoveListener(OnContinue);
        }

        private void OnEnable()
        {
            GameEvents.OnLevelCleared += OnLevelCleared;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelCleared -= OnLevelCleared;
        }

        private void OnLevelCleared(int level)
        {
            Time.timeScale = 0f;
            if (_panel != null) _panel.SetActive(true);

            if (_titleText != null)
                _titleText.text = $"LEVEL {level} COMPLETE";

            var ch = SaveManager.Character;
            string charInfo = ch != null
                ? $"Character Lv.{ch.characterLevel}  |  XP: {ch.characterXP}/{ch.XPToNextLevel}"
                : "";

            if (_infoText != null)
                _infoText.text = $"Gold: {GoldManager.RunGold}\n{charInfo}\n\nHP restored +30%";

            GameEvents.RaisePlayerHealRequested(0.3f);
        }

        private void OnContinue()
        {
            if (_panel != null) _panel.SetActive(false);

            if (_levelManager != null)
                _levelManager.ReturnToWorldMap();
        }
    }
}
