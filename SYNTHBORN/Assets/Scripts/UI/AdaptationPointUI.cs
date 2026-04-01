using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Synthborn.Core.Events;
using Synthborn.Progression;

namespace Synthborn.UI
{
    /// <summary>
    /// UI panel for allocating adaptation points during level-up.
    /// Shows 5 parameter buttons (MASS, RESILIENCE, VELOCITY, VARIANCE, YIELD)
    /// with current allocation and stat preview.
    ///
    /// Panel visibility: shown when unspent points > 0, hidden when all allocated.
    /// Integrates with the mutation selection flow — appears alongside it.
    /// </summary>
    public class AdaptationPointUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AdaptationPointManager _manager;
        [SerializeField] private GameObject _panel;

        [Header("Parameter Rows")]
        [SerializeField] private Button[] _paramButtons = new Button[5];
        [SerializeField] private TMP_Text[] _paramLabels = new TMP_Text[5];
        [SerializeField] private TMP_Text[] _paramValues = new TMP_Text[5];

        [Header("Unspent Display")]
        [SerializeField] private TMP_Text _unspentText;

        private void OnEnable()
        {
            GameEvents.OnAdaptationPointsAwarded += OnPointsAwarded;
            GameEvents.OnAdaptationPointAllocated += OnPointAllocated;

            // Wire up buttons
            for (int i = 0; i < 5; i++)
            {
                int index = i; // capture for closure
                if (_paramButtons[i] != null)
                    _paramButtons[i].onClick.AddListener(() => OnParamClicked(index));
            }
        }

        private void OnDisable()
        {
            GameEvents.OnAdaptationPointsAwarded -= OnPointsAwarded;
            GameEvents.OnAdaptationPointAllocated -= OnPointAllocated;

            for (int i = 0; i < 5; i++)
                if (_paramButtons[i] != null)
                    _paramButtons[i].onClick.RemoveAllListeners();
        }

        private void Start()
        {
            // Set parameter names
            for (int i = 0; i < 5; i++)
                if (_paramLabels[i] != null)
                    _paramLabels[i].text = AdaptationPointManager.ParameterNames[i];

            if (_panel != null) _panel.SetActive(false);
            RefreshDisplay();
        }

        private void OnPointsAwarded(int unspent)
        {
            if (_panel != null) _panel.SetActive(unspent > 0);
            RefreshDisplay();
        }

        private void OnPointAllocated(int param, int newTotal, int unspent)
        {
            RefreshDisplay();
            if (unspent <= 0 && _panel != null)
                _panel.SetActive(false);
        }

        private void OnParamClicked(int paramIndex)
        {
            if (_manager == null) return;
            _manager.AllocatePoint(paramIndex);
        }

        private void RefreshDisplay()
        {
            if (_manager == null) return;

            if (_unspentText != null)
                _unspentText.text = $"Adaptasyon Noktası: {_manager.UnspentPoints}";

            for (int i = 0; i < 5; i++)
            {
                if (_paramValues[i] != null)
                {
                    int allocated = _manager.GetAllocated(i);
                    float statVal = _manager.GetStatValue(i);
                    _paramValues[i].text = $"{allocated} (+{statVal:P0})";
                }

                if (_paramButtons[i] != null)
                    _paramButtons[i].interactable = _manager.UnspentPoints > 0;
            }
        }
    }
}
