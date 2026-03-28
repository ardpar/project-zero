using UnityEngine;

namespace Synthborn.Progression
{
    /// <summary>Stepped XP curve data. Per-level requirements.</summary>
    [CreateAssetMenu(menuName = "Synthborn/Progression/XPLevelTable")]
    public class XPLevelTable : ScriptableObject
    {
        [Tooltip("XP required per level (index 0 = level 1→2).")]
        public int[] xpTable = { 20, 25, 30, 35, 40, 50, 60, 70, 80, 100, 120, 150, 180, 220, 270, 330, 400, 500, 600, 750 };
        [Tooltip("Growth rate for levels beyond the table.")]
        public float growthRate = 1.25f;
        [Tooltip("Maximum XP gain modifier (clamp).")]
        public float xpGainModifierMax = 1.0f;
        /// <summary>Get XP required for a specific level (1-based).</summary>
        public int GetXPForLevel(int level)
        {
            int idx = level - 1;
            if (idx >= 0 && idx < xpTable.Length)
                return xpTable[idx];
            // Fallback: exponential growth beyond table
            int lastValue = xpTable.Length > 0 ? xpTable[^1] : 1000;
            int beyondCount = idx - xpTable.Length + 1;
            return Mathf.RoundToInt(lastValue * Mathf.Pow(growthRate, beyondCount));
        }
    }
}
