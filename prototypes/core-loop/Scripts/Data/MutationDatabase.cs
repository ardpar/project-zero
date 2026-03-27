// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is the core loop fun?
// Date: 2026-03-27

using System.Collections.Generic;
using UnityEngine;

public static class MutationDatabase
{
    private static List<MutationData> _all = new()
    {
        // --- COMMON ---
        new() { Name = "Sert Yumruk", Description = "Hasar +15%", Rarity = MutationData.MutationRarity.Common, DamageModifier = 0.15f },
        new() { Name = "Hizli Bacak", Description = "Hiz +10%", Rarity = MutationData.MutationRarity.Common, SpeedModifier = 0.10f },
        new() { Name = "Kalin Deri", Description = "Max HP +15%", Rarity = MutationData.MutationRarity.Common, HPModifier = 0.15f },
        new() { Name = "Hizli Parmak", Description = "Saldiri hizi +10%", Rarity = MutationData.MutationRarity.Common, AttackSpeedModifier = 0.10f },
        new() { Name = "XP Manyetik", Description = "XP kazanimi +15%", Rarity = MutationData.MutationRarity.Common, XPModifier = 0.15f },

        // --- UNCOMMON ---
        new() { Name = "Bicak Kol", Description = "Hasar +25%, Hiz -5%", Rarity = MutationData.MutationRarity.Uncommon, DamageModifier = 0.25f, SpeedModifier = -0.05f },
        new() { Name = "Roket Bacak", Description = "Hiz +20%, Dash CD -15%", Rarity = MutationData.MutationRarity.Uncommon, SpeedModifier = 0.20f, DashCdModifier = 0.15f },
        new() { Name = "Zirh Kabuk", Description = "Max HP +30%, Hiz -10%", Rarity = MutationData.MutationRarity.Uncommon, HPModifier = 0.30f, SpeedModifier = -0.10f },
        new() { Name = "Tetik Parmak", Description = "Saldiri hizi +20%", Rarity = MutationData.MutationRarity.Uncommon, AttackSpeedModifier = 0.20f },

        // --- RARE ---
        new() { Name = "Krom Kollar", Description = "Hasar +40%, Saldiri hizi +15%", Rarity = MutationData.MutationRarity.Rare, DamageModifier = 0.40f, AttackSpeedModifier = 0.15f },
        new() { Name = "Isik Bacaklari", Description = "Hiz +35%, Dash CD -25%", Rarity = MutationData.MutationRarity.Rare, SpeedModifier = 0.35f, DashCdModifier = 0.25f },
        new() { Name = "Bio Kalkan", Description = "Max HP +50%", Rarity = MutationData.MutationRarity.Rare, HPModifier = 0.50f },

        // --- LEGENDARY ---
        new() { Name = "Tam Evrim", Description = "Tum statlar +20%", Rarity = MutationData.MutationRarity.Legendary, DamageModifier = 0.20f, AttackSpeedModifier = 0.20f, SpeedModifier = 0.20f, HPModifier = 0.20f },
        new() { Name = "Olum Makinesi", Description = "Hasar +60%, Saldiri +30%, HP -20%", Rarity = MutationData.MutationRarity.Legendary, DamageModifier = 0.60f, AttackSpeedModifier = 0.30f, HPModifier = -0.20f },
    };

    private static HashSet<int> _taken = new();

    public static void Reset() => _taken.Clear();

    public static MutationData[] GetThreeCards()
    {
        var available = new List<MutationData>();
        for (int i = 0; i < _all.Count; i++)
        {
            if (!_taken.Contains(i))
                available.Add(_all[i]);
        }

        // Fallback: generic cards
        while (available.Count < 3)
        {
            available.Add(new MutationData
            {
                Name = "Guc Artisi",
                Description = "Hasar +10%",
                Rarity = MutationData.MutationRarity.Common,
                DamageModifier = 0.10f
            });
        }

        // Weighted random pick 3
        var result = new MutationData[3];
        var picked = new HashSet<int>();
        for (int c = 0; c < 3; c++)
        {
            float totalWeight = 0;
            foreach (var m in available)
            {
                totalWeight += GetWeight(m.Rarity);
            }

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0;
            for (int i = 0; i < available.Count; i++)
            {
                if (picked.Contains(i)) continue;
                cumulative += GetWeight(available[i].Rarity);
                if (roll <= cumulative)
                {
                    result[c] = available[i];
                    picked.Add(i);
                    // Mark as taken in master list
                    int masterIdx = _all.IndexOf(available[i]);
                    if (masterIdx >= 0) _taken.Add(masterIdx);
                    break;
                }
            }
            result[c] ??= available[0]; // Safety fallback
        }

        return result;
    }

    private static float GetWeight(MutationData.MutationRarity rarity) => rarity switch
    {
        MutationData.MutationRarity.Common => 50f,
        MutationData.MutationRarity.Uncommon => 30f,
        MutationData.MutationRarity.Rare => 15f,
        MutationData.MutationRarity.Legendary => 5f,
        _ => 50f
    };
}
