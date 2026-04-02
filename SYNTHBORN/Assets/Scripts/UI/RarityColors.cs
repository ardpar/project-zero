using UnityEngine;
using Synthborn.Core.Items;

namespace Synthborn.UI
{
    /// <summary>
    /// Centralized rarity color definitions. All UI that displays rarity should
    /// use these colors for visual consistency across screens.
    /// </summary>
    public static class RarityColors
    {
        /// <summary>Baseline — Grey</summary>
        public static readonly Color Baseline = new(0.6f, 0.6f, 0.6f);

        /// <summary>Calibrated — Green</summary>
        public static readonly Color Calibrated = new(0.3f, 0.85f, 0.3f);

        /// <summary>Reinforced — Blue</summary>
        public static readonly Color Reinforced = new(0.3f, 0.5f, 1f);

        /// <summary>Anomalous — Purple</summary>
        public static readonly Color Anomalous = new(0.7f, 0.3f, 1f);

        /// <summary>Architect-Grade — Gold</summary>
        public static readonly Color ArchitectGrade = new(1f, 0.82f, 0.2f);

        /// <summary>Get color by rarity enum value.</summary>
        public static Color GetColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Baseline => Baseline,
                ItemRarity.Calibrated => Calibrated,
                ItemRarity.Reinforced => Reinforced,
                ItemRarity.Anomalous => Anomalous,
                ItemRarity.ArchitectGrade => ArchitectGrade,
                _ => Color.white
            };
        }

        /// <summary>Get color by rarity int (0-4).</summary>
        public static Color GetColor(int rarityIndex)
        {
            return rarityIndex switch
            {
                0 => Baseline,
                1 => Calibrated,
                2 => Reinforced,
                3 => Anomalous,
                4 => ArchitectGrade,
                _ => Color.white
            };
        }

        /// <summary>Get hex string for rich text usage.</summary>
        public static string GetHex(ItemRarity rarity)
        {
            return ColorUtility.ToHtmlStringRGB(GetColor(rarity));
        }
    }
}
