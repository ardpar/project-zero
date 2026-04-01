using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
        [SerializeField] private TMP_Text _hpText;

        [Header("XP Bar")]
        [SerializeField] private Image _xpFill;
        [SerializeField] private TMP_Text _levelText;

        [Header("Wave / Chamber")]
        [SerializeField] private TMP_Text _waveText;
        [SerializeField] private TMP_Text _chamberText;

        [Header("Dash Cooldown")]
        [SerializeField] private Image _dashCDFill;

        [Header("Mutation Icons")]
        [SerializeField] private Transform _mutationIconContainer;
        [SerializeField] private MutationDatabase _mutationDatabase;

        private float _xpTargetFill;
        private float _xpCurrentFill;
        private Synthborn.Player.PlayerController _playerController;
        private Synthborn.Waves.TrialManager _trialManager;

        private void Start()
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                _playerController = player.GetComponent<Synthborn.Player.PlayerController>();
            _trialManager = FindAnyObjectByType<Synthborn.Waves.TrialManager>();
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
            GameEvents.OnChamberStarted += UpdateChamber;
            UpdateChamberFromTrialManager();
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerHPChanged -= UpdateHP;
            GameEvents.OnXPChanged -= UpdateXP;
            GameEvents.OnLevelUp -= UpdateLevel;
            GameEvents.OnWaveStarted -= UpdateWave;
            GameEvents.OnWaveCleared -= ShowWaveCleared;
            GameEvents.OnMutationApplied -= OnMutationApplied;
            GameEvents.OnChamberStarted -= UpdateChamber;
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
                _waveText.text = $"Dalga {waveNumber}";
        }

        private void ShowWaveCleared()
        {
            if (_waveText != null)
                _waveText.text = "Dalga Temizlendi!";
        }

        private void UpdateChamber(int chamberNumber)
        {
            if (_chamberText == null) return;
            if (_trialManager == null) _trialManager = FindAnyObjectByType<Synthborn.Waves.TrialManager>();
            if (_trialManager?.CurrentChamber != null && _trialManager.CurrentBiomeConfig != null)
                _chamberText.text = $"Deneme Odas\u0131 {chamberNumber} \u2014 {_trialManager.CurrentBiomeConfig.displayName}";
            else if (_trialManager?.CurrentChamber != null)
                _chamberText.text = $"Deneme Odas\u0131 {chamberNumber}";
        }

        private void UpdateChamberFromTrialManager()
        {
            if (_trialManager == null) _trialManager = FindAnyObjectByType<Synthborn.Waves.TrialManager>();
            if (_trialManager != null && _trialManager.IsTrialActive && _trialManager.CurrentChamber != null)
                UpdateChamber(_trialManager.CurrentChamber.chamberNumber);
        }

        private void OnMutationApplied(string mutationId, bool isSlot)
        {
            if (_mutationIconContainer == null) return;

            var iconGO = new GameObject("MutIcon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(_mutationIconContainer, false);
            var rt = iconGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(24, 24);
            var img = iconGO.GetComponent<Image>();
            img.color = isSlot ? new Color(1f, 0.8f, 0.3f) : Color.white;

            if (_mutationDatabase != null)
            {
                var m = _mutationDatabase.GetById(mutationId);
                if (m != null && m.icon != null)
                    img.sprite = m.icon;
            }
        }
    }
}
