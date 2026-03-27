// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is the core loop fun?
// Date: 2026-03-27

using UnityEngine;
using UnityEngine.UI;

public class PrototypeHUD : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private Image _hpBarFill;    // Image with Fill type
    [SerializeField] private Text _hpText;

    [Header("XP")]
    [SerializeField] private Image _xpBarFill;
    [SerializeField] private Text _levelText;

    [Header("Wave")]
    [SerializeField] private Text _waveText;
    [SerializeField] private Text _timerText;

    [Header("Dash")]
    [SerializeField] private Image _dashCooldownFill;

    [Header("References")]
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private PlayerController _playerController;

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        // HP Bar
        if (_playerHealth != null && _hpBarFill != null)
        {
            _hpBarFill.fillAmount = _playerHealth.HPRatio;
            _hpBarFill.color = Color.Lerp(Color.red, Color.green, _playerHealth.HPRatio);
            if (_hpText != null)
                _hpText.text = $"{_playerHealth.CurrentHP}/{_playerHealth.MaxHP}";
        }

        // XP Bar
        if (XPSystem.Instance != null && _xpBarFill != null)
        {
            _xpBarFill.fillAmount = XPSystem.Instance.XPRatio;
            if (_levelText != null)
                _levelText.text = $"Lv {XPSystem.Instance.Level}";
        }

        // Wave & Timer
        if (GameManager.Instance != null)
        {
            if (_waveText != null)
                _waveText.text = $"Wave {GameManager.Instance.CurrentWave}";
            if (_timerText != null)
                _timerText.text = $"{GameManager.Instance.RunTime:F0}s";
        }

        // Dash Cooldown
        if (_playerController != null && _dashCooldownFill != null)
        {
            _dashCooldownFill.fillAmount = 1f - _playerController.DashCooldownRatio;
        }
    }
}
