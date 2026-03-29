using UnityEngine;
using Synthborn.Core.Persistence;

namespace Synthborn.Core.Items
{
    /// <summary>
    /// Manages synthesis: combine materials into components.
    /// Materials (Arena terminology):
    ///   Residual Compound (scrapMetal) — common synthesis base
    ///   Mutation Residue (darkCrystals) — rare catalytic material
    ///   Stabilized Core (bossEssences) — boss-tier foundation
    /// </summary>
    public static class CraftingManager
    {
        /// <summary>Synthesize a random Common component from Residual Compound.</summary>
        public static bool CraftCommon(ItemDatabase db)
        {
            var ch = SaveManager.Character;
            if (ch == null || db == null) return false;
            if (ch.scrapMetal < 3) return false;

            ch.scrapMetal -= 3;
            var item = GetRandomItemOfRarity(db, ItemRarity.Common);
            if (item != null) InventoryManager.AddToInventory(item.Id);
            SaveManager.SaveSlot();
            return true;
        }

        /// <summary>Synthesize a random Rare component from Mutation Residue + Residual Compound.</summary>
        public static bool CraftRare(ItemDatabase db)
        {
            var ch = SaveManager.Character;
            if (ch == null || db == null) return false;
            if (ch.darkCrystals < 2 || ch.scrapMetal < 1) return false;

            ch.darkCrystals -= 2;
            ch.scrapMetal -= 1;
            var item = GetRandomItemOfRarity(db, ItemRarity.Rare);
            if (item != null) InventoryManager.AddToInventory(item.Id);
            SaveManager.SaveSlot();
            return true;
        }

        /// <summary>Synthesize a Legendary component from Stabilized Core + Mutation Residue.</summary>
        public static bool CraftLegendary(ItemDatabase db)
        {
            var ch = SaveManager.Character;
            if (ch == null || db == null) return false;
            if (ch.bossEssences < 1 || ch.darkCrystals < 3) return false;

            ch.bossEssences -= 1;
            ch.darkCrystals -= 3;
            var item = GetRandomItemOfRarity(db, ItemRarity.Legendary);
            if (item == null) item = GetRandomItemOfRarity(db, ItemRarity.Epic);
            if (item != null) InventoryManager.AddToInventory(item.Id);
            SaveManager.SaveSlot();
            return true;
        }

        /// <summary>Award materials from enemy kill based on tier. Pressure scales drop rates.</summary>
        public static void AwardMaterials(int enemyTier, float pressureMultiplier = 1f)
        {
            var ch = SaveManager.Character;
            if (ch == null) return;

            // Base rates, boosted by pressure
            float compoundChance = 0.20f * pressureMultiplier;
            float residueChance = 0.15f * pressureMultiplier;

            switch (enemyTier)
            {
                case 0: // Normal — chance for Residual Compound
                    if (Random.value < compoundChance) ch.scrapMetal++;
                    break;
                case 1: // Elite — guaranteed Compound + chance for Residue
                    ch.scrapMetal++;
                    if (pressureMultiplier >= 1.3f) ch.scrapMetal++; // Bonus at high pressure
                    if (Random.value < residueChance) ch.darkCrystals++;
                    break;
                case 2: // Stabilized (Boss) — guaranteed Residue + Core
                    ch.darkCrystals++;
                    ch.bossEssences++;
                    if (pressureMultiplier >= 1.5f) ch.darkCrystals++; // Bonus at high pressure
                    break;
            }
        }

        private static ItemData GetRandomItemOfRarity(ItemDatabase db, ItemRarity rarity)
        {
            var candidates = new System.Collections.Generic.List<ItemData>();
            foreach (var item in db.allItems)
                if (item != null && item.Rarity == rarity)
                    candidates.Add(item);
            if (candidates.Count == 0)
                foreach (var item in db.allItems)
                    if (item != null) candidates.Add(item);
            return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : null;
        }
    }
}
