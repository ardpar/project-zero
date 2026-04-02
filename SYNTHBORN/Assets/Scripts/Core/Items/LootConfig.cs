using UnityEngine;

namespace Synthborn.Core.Items
{
    /// <summary>
    /// Configuration for loot drop rates and tier thresholds.
    /// Replaces hardcoded values in LootDropper.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Items/Loot Config", fileName = "LootConfig")]
    public class LootConfig : ScriptableObject
    {
        [Header("Normal Enemy Drop")]
        [Tooltip("Base drop chance for normal enemies. Default 0.05 = 5%.")]
        [Range(0.01f, 0.20f)]
        public float normalDropChance = 0.05f;

        [Header("Elite Enemy Drop")]
        [Tooltip("Base drop chance for elite enemies. Default 0.30 = 30%.")]
        [Range(0.10f, 0.60f)]
        public float eliteDropChance = 0.30f;

        [Header("Boss (Stabilized) Drop")]
        [Tooltip("Legendary threshold roll. Below this = Architect-Grade attempt. Default 0.10.")]
        [Range(0.01f, 0.30f)]
        public float legendaryBaseThreshold = 0.10f;

        [Tooltip("Epic threshold roll. Below this = Anomalous. Default 0.30.")]
        [Range(0.10f, 0.60f)]
        public float epicBaseThreshold = 0.30f;

        [Tooltip("Pressure boost multiplier for legendary chance. Default 0.5.")]
        [Range(0.1f, 1f)]
        public float legendaryPressureScale = 0.5f;

        [Tooltip("Pressure boost multiplier for epic chance. Default 0.3.")]
        [Range(0.1f, 1f)]
        public float epicPressureScale = 0.3f;

        [Header("Level Thresholds")]
        [Tooltip("Minimum level for Anomalous drops from bosses. Default 4.")]
        [Range(1, 20)]
        public int epicMinLevel = 4;

        [Tooltip("Minimum level for Architect-Grade drops from bosses. Default 8.")]
        [Range(1, 20)]
        public int legendaryMinLevel = 8;

        [Header("Pressure Scaling")]
        [Tooltip("Pressure rating threshold for elite rarity upgrade. Default 0.3.")]
        [Range(0.1f, 0.5f)]
        public float eliteRarityUpgradeThreshold = 0.3f;
    }
}
