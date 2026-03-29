using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synthborn.Core.Persistence
{
    /// <summary>
    /// Persistent character data stored per save slot.
    /// Includes identity, progression, equipment, inventory, and skill tree.
    /// </summary>
    [Serializable]
    public class CharacterSaveData
    {
        // Identity
        public string characterName = "";
        public int classType; // 0=Warrior, 1=Rogue, 2=Mage, 3=Sentinel

        // Progression
        public int characterLevel = 1;
        public int characterXP;
        public int[] statPoints = new int[5]; // STR, VIT, AGI, LCK, WIS
        public int unspentStatPoints;
        public int unspentSkillPoints;

        // Equipment (item IDs per slot: 0=Helmet, 1=Armor, 2=Weapon, 3=Gloves, 4=Boots, 5=Accessory)
        public string[] equippedItemIds = new string[6];

        // Inventory
        public List<ItemSaveEntry> inventoryItems = new();

        // Skill Tree
        public List<string> unlockedSkillNodes = new();

        // World Map (legacy level-based)
        public List<int> completedLevels = new();
        public int highestLevelUnlocked = 1;

        // Arena Map (Trial Chambers)
        public List<int> completedChambers = new();
        public List<int> unlockedChambers = new() { 1 }; // Chamber 1 always unlocked

        // Economy
        public int gold;

        // Crafting Materials
        public int scrapMetal;
        public int darkCrystals;
        public int bossEssences;

        // Discovered Synthesis Recipes (recipe IDs)
        public List<string> discoveredRecipes = new();

        // Star Ratings (best star count per level, index = level-1)
        public List<int> levelStars = new();

        // Meta
        public string lastPlayedDate = "";
        public float totalPlayTime;

        /// <summary>Check if a level is unlocked.</summary>
        public bool IsLevelUnlocked(int levelNumber) => levelNumber <= highestLevelUnlocked;

        /// <summary>Check if a level is completed.</summary>
        public bool IsLevelCompleted(int levelNumber) => completedLevels.Contains(levelNumber);

        /// <summary>Mark a level as completed and unlock the next.</summary>
        public void CompleteLevel(int levelNumber)
        {
            if (!completedLevels.Contains(levelNumber))
                completedLevels.Add(levelNumber);
            if (levelNumber >= highestLevelUnlocked)
                highestLevelUnlocked = levelNumber + 1;
        }

        // ─── Trial Chamber helpers ───

        /// <summary>Check if a chamber is unlocked.</summary>
        public bool IsChamberUnlocked(int chamberNumber) => unlockedChambers.Contains(chamberNumber);

        /// <summary>Check if a chamber is completed.</summary>
        public bool IsChamberCompleted(int chamberNumber) => completedChambers.Contains(chamberNumber);

        /// <summary>Mark a chamber as completed and unlock adjacent chambers.</summary>
        public void CompleteChamber(int chamberNumber, int[] adjacentChambers)
        {
            if (!completedChambers.Contains(chamberNumber))
                completedChambers.Add(chamberNumber);

            if (adjacentChambers != null)
            {
                foreach (int adj in adjacentChambers)
                {
                    if (!unlockedChambers.Contains(adj))
                        unlockedChambers.Add(adj);
                }
            }
        }

        /// <summary>XP needed for next level. Exponential curve with soft cap.</summary>
        public int XPToNextLevel => (int)(100 * Mathf.Pow(characterLevel, 1.4f));

        /// <summary>Add XP and handle level-ups. Returns number of level-ups.</summary>
        public int AddXP(int amount)
        {
            characterXP += amount;
            int levelUps = 0;
            while (characterXP >= XPToNextLevel && characterLevel < 99)
            {
                characterXP -= XPToNextLevel;
                characterLevel++;
                unspentStatPoints++;
                unspentSkillPoints++;
                levelUps++;
            }
            return levelUps;
        }
    }

    /// <summary>Serializable item entry for inventory.</summary>
    [Serializable]
    public class ItemSaveEntry
    {
        public string itemId;
        public int rarity; // 0-4
        public float[] statBonuses = new float[8]; // matches CombatStatBlock order
    }
}
