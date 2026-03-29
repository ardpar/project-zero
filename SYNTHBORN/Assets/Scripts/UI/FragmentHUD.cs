using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Events;
using Synthborn.Core.Persistence;

namespace Synthborn.UI
{
    /// <summary>
    /// Displays current Substrate Fragment count on the HUD.
    /// </summary>
    public class FragmentHUD : MonoBehaviour
    {
        [SerializeField] private Text _fragmentText;

        private void OnEnable()
        {
            GameEvents.OnFragmentChanged += OnFragmentChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnFragmentChanged -= OnFragmentChanged;
        }

        private void Start()
        {
            UpdateDisplay(FragmentManager.RunFragments);
        }

        private void OnFragmentChanged(int total)
        {
            UpdateDisplay(total);
        }

        private void UpdateDisplay(int fragments)
        {
            if (_fragmentText != null)
                _fragmentText.text = $"Fragment: {fragments}";
        }
    }
}
