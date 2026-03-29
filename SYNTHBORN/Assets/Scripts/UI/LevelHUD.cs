using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Events;

namespace Synthborn.UI
{
    /// <summary>
    /// Displays current level and wave on the HUD.
    /// </summary>
    public class LevelHUD : MonoBehaviour
    {
        [SerializeField] private Text _levelText;

        private int _currentLevel = 1;

        private void OnEnable()
        {
            GameEvents.OnLevelStarted += OnLevelStarted;
            GameEvents.OnWaveStarted += OnWaveStarted;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelStarted -= OnLevelStarted;
            GameEvents.OnWaveStarted -= OnWaveStarted;
        }

        private void OnLevelStarted(int level, string name)
        {
            _currentLevel = level;
            if (_levelText != null)
                _levelText.text = $"Level {level} - {name}";
        }

        private void OnWaveStarted(int wave)
        {
            if (_levelText != null)
                _levelText.text = $"Level {_currentLevel} - Wave {wave}";
        }
    }
}
