using UnityEngine;

namespace Synthborn.Core.Persistence
{
    /// <summary>
    /// Manages persistent stat upgrades purchased with cells.
    /// 5 upgrade types, each with 5 tiers of increasing cost and effect.
    /// </summary>
    public static class UpgradeManager
    {
        public const int UpgradeCount = 5;
        public const int MaxLevel = 5;

        /// <summary>Upgrade type indices matching SaveData.upgradeLevels array.</summary>
        public enum UpgradeType { MaxHP = 0, MoveSpeed = 1, XPGain = 2, CritChance = 3, StartArmor = 4 }

        /// <summary>Display names for each upgrade type.</summary>
        public static readonly string[] Names = { "Max HP", "Move Speed", "XP Gain", "Crit Chance", "Starting Armor" };

        /// <summary>Per-tier cell costs: [5, 15, 35, 70, 120].</summary>
        public static readonly int[] TierCosts = { 5, 15, 35, 70, 120 };

        /// <summary>Per-tier stat bonuses per upgrade type.</summary>
        private static readonly float[][] TierValues =
        {
            new[] { 0.05f, 0.10f, 0.15f, 0.20f, 0.30f }, // MaxHP: +5% to +30%
            new[] { 0.03f, 0.06f, 0.09f, 0.12f, 0.15f }, // MoveSpeed: +3% to +15%
            new[] { 0.05f, 0.10f, 0.15f, 0.20f, 0.30f }, // XPGain: +5% to +30%
            new[] { 0.02f, 0.04f, 0.06f, 0.08f, 0.10f }, // CritChance: +2% to +10%
            new[] { 2f,    4f,    6f,    8f,    12f    }, // StartArmor: +2 to +12
        };

        /// <summary>Current level of an upgrade (0 = not purchased).</summary>
        public static int GetLevel(UpgradeType type) => SaveManager.Data.upgradeLevels[(int)type];

        /// <summary>Cost to purchase the next tier. -1 if maxed.</summary>
        public static int GetNextCost(UpgradeType type)
        {
            int level = GetLevel(type);
            return level >= MaxLevel ? -1 : TierCosts[level];
        }

        /// <summary>Current stat bonus for an upgrade type at its current level.</summary>
        public static float GetCurrentBonus(UpgradeType type)
        {
            int level = GetLevel(type);
            return level <= 0 ? 0f : TierValues[(int)type][level - 1];
        }

        /// <summary>Try to purchase the next tier. Returns false if maxed or insufficient cells.</summary>
        public static bool TryUpgrade(UpgradeType type)
        {
            var data = SaveManager.Data;
            int level = data.upgradeLevels[(int)type];
            if (level >= MaxLevel) return false;

            int cost = TierCosts[level];
            if (data.totalCells < cost) return false;

            data.totalCells -= cost;
            data.upgradeLevels[(int)type] = level + 1;
            SaveManager.Save();
            return true;
        }

        /// <summary>
        /// Apply all upgrade bonuses to a CombatStatBlock at run start.
        /// Call after Reset() and before mutations.
        /// </summary>
        public static void ApplyToStats(Stats.CombatStatBlock stats)
        {
            stats.ApplyMutation(
                hpModifier: GetCurrentBonus(UpgradeType.MaxHP),
                speedModifier: GetCurrentBonus(UpgradeType.MoveSpeed),
                critChance: GetCurrentBonus(UpgradeType.CritChance),
                armorFlat: Mathf.RoundToInt(GetCurrentBonus(UpgradeType.StartArmor))
            );
        }

        /// <summary>Current XP gain multiplier from upgrades (1.0 = no bonus).</summary>
        public static float XPGainMultiplier => 1f + GetCurrentBonus(UpgradeType.XPGain);
    }
}
