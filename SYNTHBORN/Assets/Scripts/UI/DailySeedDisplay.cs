using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core;

namespace Synthborn.UI
{
    /// <summary>
    /// Displays the daily seed string on the HUD.
    /// Updates once on Start after DailySeedManager initializes.
    /// </summary>
    public class DailySeedDisplay : MonoBehaviour
    {
        [SerializeField] private Text _seedText;

        private void Start()
        {
            UpdateDisplay();
        }

        /// <summary>Refresh seed text from DailySeedManager.</summary>
        public void UpdateDisplay()
        {
            if (_seedText == null) return;
            _seedText.text = $"Seed: {DailySeedManager.SeedDisplayString}";
        }
    }
}
