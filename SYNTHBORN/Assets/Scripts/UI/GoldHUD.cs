using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Synthborn.Core.Events;
using Synthborn.Core.Persistence;

namespace Synthborn.UI
{
    /// <summary>
    /// Displays current run gold on the HUD.
    /// </summary>
    public class GoldHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text _goldText;

        private void OnEnable()
        {
            GameEvents.OnFragmentChanged += OnGoldChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnFragmentChanged -= OnGoldChanged;
        }

        private void Start()
        {
            UpdateDisplay(FragmentManager.RunFragments);
        }

        private void OnGoldChanged(int total)
        {
            UpdateDisplay(total);
        }

        private void UpdateDisplay(int gold)
        {
            if (_goldText != null)
                _goldText.text = $"Gold: {gold}";
        }
    }
}
