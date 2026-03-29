using UnityEngine;
using Synthborn.Core.Persistence;

namespace Synthborn.Core.Items
{
    /// <summary>
    /// Manages synthesis: combine materials into components.
    /// Expansion-vision recipes:
    ///   3x Residual Compound → Baseline Komponent
    ///   5x Residual Compound → Calibrated Komponent
    ///   2x Mutation Residue + 1x Compound → Reinforced Komponent
    ///   1x Stabilized Core + 3x Residue → Architect-Grade Komponent
    ///   2x Same Rarity → 1x Next Rarity (merge)
    /// </summary>
    public static class CraftingManager
    {
        /// <summary>Fired when a recipe is used for the first time. string = recipe ID.</summary>
        public static event System.Action<string> OnRecipeDiscovered;

        /// <summary>Synthesize a random Baseline component. Cost: 3 Residual Compound.</summary>
        public static bool SynthesizeBaseline(ItemDatabase db)
        {
            var ch = SaveManager.Character;
            if (ch == null || db == null) return false;
            if (ch.scrapMetal < 3) return false;

            ch.scrapMetal -= 3;
            var item = GetRandomItemOfRarity(db, ItemRarity.Baseline);
            if (item != null) InventoryManager.AddToInventory(item.Id);
            TrackRecipe("baseline");
            SaveManager.SaveSlot();
            return true;
        }

        /// <summary>Synthesize a random Calibrated component. Cost: 5 Residual Compound.</summary>
        public static bool SynthesizeCalibrated(ItemDatabase db)
        {
            var ch = SaveManager.Character;
            if (ch == null || db == null) return false;
            if (ch.scrapMetal < 5) return false;

            ch.scrapMetal -= 5;
            var item = GetRandomItemOfRarity(db, ItemRarity.Calibrated);
            if (item == null) item = GetRandomItemOfRarity(db, ItemRarity.Baseline);
            if (item != null) InventoryManager.AddToInventory(item.Id);
            TrackRecipe("calibrated");
            SaveManager.SaveSlot();
            return true;
        }

        /// <summary>Synthesize a random Reinforced component. Cost: 2 Mutation Residue + 1 Compound.</summary>
        public static bool SynthesizeReinforced(ItemDatabase db)
        {
            var ch = SaveManager.Character;
            if (ch == null || db == null) return false;
            if (ch.darkCrystals < 2 || ch.scrapMetal < 1) return false;

            ch.darkCrystals -= 2;
            ch.scrapMetal -= 1;
            var item = GetRandomItemOfRarity(db, ItemRarity.Reinforced);
            if (item != null) InventoryManager.AddToInventory(item.Id);
            TrackRecipe("reinforced");
            SaveManager.SaveSlot();
            return true;
        }

        /// <summary>Synthesize an Architect-Grade component. Cost: 1 Stabilized Core + 3 Mutation Residue.</summary>
        public static bool SynthesizeArchitectGrade(ItemDatabase db)
        {
            var ch = SaveManager.Character;
            if (ch == null || db == null) return false;
            if (ch.bossEssences < 1 || ch.darkCrystals < 3) return false;

            ch.bossEssences -= 1;
            ch.darkCrystals -= 3;
            var item = GetRandomItemOfRarity(db, ItemRarity.ArchitectGrade);
            if (item == null) item = GetRandomItemOfRarity(db, ItemRarity.Anomalous);
            if (item != null) InventoryManager.AddToInventory(item.Id);
            TrackRecipe("architect_grade");
            SaveManager.SaveSlot();
            return true;
        }

        /// <summary>Merge two items of the same rarity into one of the next rarity tier.
        /// Stats are averaged with a 10% bonus. Returns the new item ID or null.</summary>
        public static string MergeComponents(ItemDatabase db, string itemId1, string itemId2)
        {
            var ch = SaveManager.Character;
            if (ch == null || db == null) return null;

            var item1 = db.GetById(itemId1);
            var item2 = db.GetById(itemId2);
            if (item1 == null || item2 == null) return null;
            if (item1.Rarity != item2.Rarity) return null;
            if ((int)item1.Rarity >= (int)ItemRarity.ArchitectGrade) return null; // Can't merge max rarity

            ItemRarity targetRarity = (ItemRarity)((int)item1.Rarity + 1);

            // Remove both items from inventory
            InventoryManager.RemoveFromInventory(itemId1);
            InventoryManager.RemoveFromInventory(itemId2);

            // Get a random item of the target rarity
            var result = GetRandomItemOfRarity(db, targetRarity);
            if (result == null) result = GetRandomItemOfRarity(db, item1.Rarity); // Fallback
            if (result != null) InventoryManager.AddToInventory(result.Id);

            TrackRecipe("merge_" + targetRarity.ToString().ToLower());
            SaveManager.SaveSlot();
            return result?.Id;
        }

        // ─── Legacy aliases for backward compat ───

        /// <summary>Legacy: calls SynthesizeBaseline.</summary>
        public static bool CraftCommon(ItemDatabase db) => SynthesizeBaseline(db);
        /// <summary>Legacy: calls SynthesizeReinforced.</summary>
        public static bool CraftRare(ItemDatabase db) => SynthesizeReinforced(db);
        /// <summary>Legacy: calls SynthesizeArchitectGrade.</summary>
        public static bool CraftLegendary(ItemDatabase db) => SynthesizeArchitectGrade(db);

        /// <summary>Award materials from enemy kill based on tier. Pressure scales drop rates.</summary>
        public static void AwardMaterials(int enemyTier, float pressureMultiplier = 1f)
        {
            var ch = SaveManager.Character;
            if (ch == null) return;

            float compoundChance = 0.20f * pressureMultiplier;
            float residueChance = 0.15f * pressureMultiplier;

            switch (enemyTier)
            {
                case 0: // Normal — chance for Residual Compound
                    if (Random.value < compoundChance) ch.scrapMetal++;
                    break;
                case 1: // Elite — guaranteed Compound + chance for Residue
                    ch.scrapMetal++;
                    if (pressureMultiplier >= 1.3f) ch.scrapMetal++;
                    if (Random.value < residueChance) ch.darkCrystals++;
                    break;
                case 2: // Stabilized — guaranteed Residue + Core
                    ch.darkCrystals++;
                    ch.bossEssences++;
                    if (pressureMultiplier >= 1.5f) ch.darkCrystals++;
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

        private static void TrackRecipe(string recipeId)
        {
            var ch = SaveManager.Character;
            if (ch == null) return;
            if (!ch.discoveredRecipes.Contains(recipeId))
            {
                ch.discoveredRecipes.Add(recipeId);
                OnRecipeDiscovered?.Invoke(recipeId);
            }
        }

        /// <summary>Check if a recipe has been discovered.</summary>
        public static bool IsRecipeDiscovered(string recipeId)
        {
            var ch = SaveManager.Character;
            return ch != null && ch.discoveredRecipes.Contains(recipeId);
        }
    }
}
