using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Events;
using Synthborn.Mutations;

namespace Synthborn.UI
{
    /// <summary>
    /// Prototype HUD: HP bar, XP bar, level text, wave text, dash CD, mutation icons.
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

        [Header("Dash Cooldown")]
        [SerializeField] private Image _dashCDFill;

        [Header("Mutation Icons")]
        [SerializeField] private Transform _mutationIconContainer;

        private float _xpTargetFill;
        private float _xpCurrentFill;
        private Synthborn.Player.PlayerController _playerController;

        private void Start()
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                _playerController = player.GetComponent<Synthborn.Player.PlayerController>();
        }

        private void Update()
        {
            // Smooth XP bar fill
            if (_xpFill != null && Mathf.Abs(_xpCurrentFill - _xpTargetFill) > 0.001f)
            {
                _xpCurrentFill = Mathf.MoveTowards(_xpCurrentFill, _xpTargetFill, Time.unscaledDeltaTime * 3f);
                _xpFill.fillAmount = _xpCurrentFill;
            }

            // Dash CD
            if (_dashCDFill != null && _playerController != null)
                _dashCDFill.fillAmount = _playerController.DashCooldownProgress;
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerHPChanged += UpdateHP;
            GameEvents.OnXPChanged += UpdateXP;
            GameEvents.OnLevelUp += UpdateLevel;
            GameEvents.OnWaveStarted += UpdateWave;
            GameEvents.OnWaveCleared += ShowWaveCleared;
            GameEvents.OnMutationApplied += OnMutationApplied;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerHPChanged -= UpdateHP;
            GameEvents.OnXPChanged -= UpdateXP;
            GameEvents.OnLevelUp -= UpdateLevel;
            GameEvents.OnWaveStarted -= UpdateWave;
            GameEvents.OnWaveCleared -= ShowWaveCleared;
            GameEvents.OnMutationApplied -= OnMutationApplied;
        }

        private void UpdateHP(int current, int max)
        {
            float ratio = max > 0 ? (float)current / max : 0f;
            if (_hpFill != null)
            {
                _hpFill.fillAmount = ratio;
                // Green→Yellow→Red gradient
                _hpFill.color = Color.Lerp(Color.red, Color.green, ratio);
            }
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

        private void OnMutationApplied(string mutationId, bool isSlot)
        {
            if (_mutationIconContainer == null) return;

            // Find the mutation data for its icon
            var db = Object.FindFirstObjectByType<MutationManager>();
            // Create a small icon in the container
            var iconGO = new GameObject("MutIcon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(_mutationIconContainer, false);
            var rt = iconGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(24, 24);
            var img = iconGO.GetComponent<Image>();
            img.color = isSlot ? new Color(1f, 0.8f, 0.3f) : Color.white;

            // Try to find the icon sprite from MutationDatabase
            var databases = Resources.FindObjectsOfTypeAll<MutationDatabase>();
            foreach (var mdb in databases)
            {
                var m = mdb.GetById(mutationId);
                if (m != null && m.icon != null)
                {
                    img.sprite = m.icon;
                    break;
                }
            }
        }
    }
}
