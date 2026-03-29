using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Events;
using Synthborn.Waves;

namespace Synthborn.UI
{
    /// <summary>
    /// Displayed between Trial Chambers during a multi-room run.
    /// Shows loot collected, XP/gold summary, and options to continue or return.
    /// </summary>
    public class CalibrationIntervalScreen : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TrialManager _trialManager;
        [SerializeField] private GameObject _panel;

        [Header("UI Elements")]
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _summaryText;
        [SerializeField] private Text _lootListText;
        [SerializeField] private Transform _nextChamberContainer;
        [SerializeField] private Button _returnButton;

        [SerializeField] private Font _font;

        private void OnEnable()
        {
            GameEvents.OnCalibrationIntervalStarted += Show;
        }

        private void OnDisable()
        {
            GameEvents.OnCalibrationIntervalStarted -= Show;
        }

        private void Start()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void Show()
        {
            if (_panel != null) _panel.SetActive(true);

            var chamber = _trialManager?.CurrentChamber;
            if (chamber != null && _titleText != null)
                _titleText.text = $"KALIBRASYON ARALI\u011eI\n<size=14>Deneme Odas\u0131 {chamber.chamberNumber} Tamamland\u0131</size>";

            // Summary
            if (_summaryText != null)
            {
                var ch = Synthborn.Core.Persistence.SaveManager.Character;
                string gold = ch != null ? ch.gold.ToString() : "0";
                int roomsThisRun = _trialManager?.RunCompletedChambers?.Count ?? 0;
                _summaryText.text = $"Toplanan Substrate Fragment: {gold}\nBu denemede tamamlanan oda: {roomsThisRun}";
            }

            // Loot list
            if (_lootListText != null)
            {
                var loot = _trialManager?.RunLoot;
                if (loot != null && loot.Count > 0)
                    _lootListText.text = $"Kazan\u0131lan Komponent: {loot.Count} adet";
                else
                    _lootListText.text = "Komponent bulunamad\u0131.";
            }

            // Next chamber buttons
            RefreshNextChamberButtons();

            // Return button
            if (_returnButton != null)
            {
                _returnButton.onClick.RemoveAllListeners();
                _returnButton.onClick.AddListener(OnReturnClicked);
            }
        }

        private void RefreshNextChamberButtons()
        {
            if (_nextChamberContainer == null || _trialManager == null) return;

            foreach (Transform child in _nextChamberContainer) Destroy(child.gameObject);

            var available = _trialManager.GetAvailableNextChambers();
            foreach (int chamberNum in available)
            {
                CreateNextChamberButton(chamberNum);
            }
        }

        private void CreateNextChamberButton(int chamberNumber)
        {
            var btnGO = new GameObject($"NextChamber_{chamberNumber}", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(_nextChamberContainer, false);

            var rect = btnGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 50);

            var img = btnGO.GetComponent<Image>();
            img.color = new Color(0.2f, 0.3f, 0.4f);

            // Button text
            var textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(btnGO.transform, false);
            var tRect = textGO.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;

            var text = textGO.GetComponent<Text>();
            text.text = $"Sonraki Oda: {chamberNumber}";
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = _font;
            text.raycastTarget = false;

            var btn = btnGO.GetComponent<Button>();
            int captured = chamberNumber;
            btn.onClick.AddListener(() => OnNextChamberClicked(captured));
        }

        private void OnNextChamberClicked(int chamberNumber)
        {
            if (_panel != null) _panel.SetActive(false);
            _trialManager?.ContinueToNextChamber(chamberNumber);
        }

        private void OnReturnClicked()
        {
            if (_panel != null) _panel.SetActive(false);
            _trialManager?.ReturnToArenaMap();
        }
    }
}
