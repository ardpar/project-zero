using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Events;

namespace Synthborn.UI
{
    /// <summary>
    /// Prototype HUD: HP bar, XP bar, level text, wave text.
    /// Subscribe to GameEvents and update UGUI elements.
    /// </summary>
    public class PrototypeHUD : MonoBehaviour
    {
        [Header("HP Bar")]
        [SerializeField] private Image _hpFill;
        [SerializeField] private Text _hpText;

        [Header("XP Bar")]
        [SerializeField] private Image _xpFill;
        [SerializeField] private Text _levelText;

        [Header("Wave")]
        [SerializeField] private Text _waveText;

        private float _xpTargetFill;
        private float _xpCurrentFill;

        private void Update()
        {
            // Smooth XP bar fill
            if (_xpFill != null && Mathf.Abs(_xpCurrentFill - _xpTargetFill) > 0.001f)
            {
                _xpCurrentFill = Mathf.MoveTowards(_xpCurrentFill, _xpTargetFill, Time.unscaledDeltaTime * 3f);
                _xpFill.fillAmount = _xpCurrentFill;
            }
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerHPChanged += UpdateHP;
            GameEvents.OnXPChanged += UpdateXP;
            GameEvents.OnLevelUp += UpdateLevel;
            GameEvents.OnWaveStarted += UpdateWave;
            GameEvents.OnWaveCleared += ShowWaveCleared;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerHPChanged -= UpdateHP;
            GameEvents.OnXPChanged -= UpdateXP;
            GameEvents.OnLevelUp -= UpdateLevel;
            GameEvents.OnWaveStarted -= UpdateWave;
            GameEvents.OnWaveCleared -= ShowWaveCleared;
        }

        private void UpdateHP(int current, int max)
        {
            if (_hpFill != null)
                _hpFill.fillAmount = max > 0 ? (float)current / max : 0f;
            if (_hpText != null)
                _hpText.text = $"{current}/{max}";
        }

        private void UpdateXP(int current, int xpToNext)
        {
            _xpTargetFill = xpToNext > 0 ? (float)current / xpToNext : 0f;
        }

        private void UpdateLevel(int level)
        {
            if (_levelText != null)
                _levelText.text = $"Lv.{level}";
        }

        private void UpdateWave(int waveNumber)
        {
            if (_waveText != null)
                _waveText.text = $"Wave {waveNumber}";
        }

        private void ShowWaveCleared()
        {
            if (_waveText != null)
                _waveText.text = "Wave Clear!";
        }
    }
}
