using UnityEngine;
using Synthborn.Core.Persistence;

namespace Synthborn.Core.Items
{
    /// <summary>
    /// Manages crafting: combine materials into items.
    /// Materials: Scrap Metal (common), Dark Crystal (rare), Boss Essence (boss).
    /// </summary>
    public static class CraftingManager
    {
        /// <summary>Craft a random Common item from scrap metal.</summary>
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

        /// <summary>Craft a random Rare item from dark crystals + scrap.</summary>
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

        /// <summary>Craft a Legendary item from boss essence + dark crystals.</summary>
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

        /// <summary>Award materials from enemy kill based on tier.</summary>
        public static void AwardMaterials(int enemyTier)
        {
            var ch = SaveManager.Character;
            if (ch == null) return;

            switch (enemyTier)
            {
                case 0: // Normal — 20% chance for 1 scrap
                    if (Random.value < 0.20f) ch.scrapMetal++;
                    break;
                case 1: // Elite — guaranteed 1 scrap + 15% dark crystal
                    ch.scrapMetal++;
                    if (Random.value < 0.15f) ch.darkCrystals++;
                    break;
                case 2: // Boss — guaranteed 1 dark crystal + 1 boss essence
                    ch.darkCrystals++;
                    ch.bossEssences++;
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
