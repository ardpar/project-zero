using UnityEngine;

namespace Synthborn.Core.Persistence
{
    /// <summary>
    /// Tracks gold earned during a run. Gold is spent at level transition shops
    /// and partially converted to meta-gold on run end.
    /// </summary>
    public static class GoldManager
    {
        /// <summary>Gold earned in current run.</summary>
        public static int RunGold { get; private set; }

        /// <summary>Gold drop amounts by enemy tier.</summary>
        private static readonly int[] TierGoldMin = { 1, 5, 50 };  // Normal, Elite, Boss
        private static readonly int[] TierGoldMax = { 3, 10, 100 };

        /// <summary>Reset gold for a new run.</summary>
        public static void ResetRun() => RunGold = 0;

        /// <summary>Add gold from a kill.</summary>
        public static void AddGold(int amount)
        {
            RunGold += amount;
            Events.GameEvents.GoldChanged(RunGold);
        }

        /// <summary>Spend gold (shop, craft). Returns false if insufficient.</summary>
        public static bool TrySpend(int amount)
        {
            if (RunGold < amount) return false;
            RunGold -= amount;
            Events.GameEvents.GoldChanged(RunGold);
            return true;
        }

        /// <summary>Calculate gold drop for an enemy tier (0=Normal, 1=Elite, 2=Boss).</summary>
        public static int RollGoldDrop(int tierIndex)
        {
            int idx = Mathf.Clamp(tierIndex, 0, TierGoldMin.Length - 1);
            return Random.Range(TierGoldMin[idx], TierGoldMax[idx] + 1);
        }

        /// <summary>Convert remaining run gold to meta-gold (20%) on run end.</summary>
        public static int ConvertToMeta()
        {
            int metaGold = Mathf.RoundToInt(RunGold * 0.2f);
            // Could store in SaveData for meta purchases later
            return metaGold;
        }
    }
}
