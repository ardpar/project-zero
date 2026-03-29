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

        [Header("Wave / Chamber")]
        [SerializeField] private Text _waveText;
        [SerializeField] private Text _chamberText;

        [Header("Dash Cooldown")]
        [SerializeField] private Image _dashCDFill;

        [Header("Mutation Icons")]
        [SerializeField] private Transform _mutationIconContainer;
        [SerializeField] private MutationDatabase _mutationDatabase;

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
            var tm = FindAnyObjectByType<Synthborn.Waves.TrialManager>();
            if (tm?.CurrentChamber != null && tm.CurrentBiomeConfig != null)
                _chamberText.text = $"Deneme Odas\u0131 {chamberNumber} \u2014 {tm.CurrentBiomeConfig.displayName}";
            else if (tm?.CurrentChamber != null)
                _chamberText.text = $"Deneme Odas\u0131 {chamberNumber}";
        }

        private void UpdateChamberFromTrialManager()
        {
            var tm = FindAnyObjectByType<Synthborn.Waves.TrialManager>();
            if (tm != null && tm.IsTrialActive && tm.CurrentChamber != null)
                UpdateChamber(tm.CurrentChamber.chamberNumber);
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
