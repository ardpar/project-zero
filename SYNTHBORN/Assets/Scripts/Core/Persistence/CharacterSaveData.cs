using System;
using System.Collections.Generic;

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

        // Equipment (item IDs per slot: 0=Helmet, 1=Armor, 2=Weapon, 3=Gloves, 4=Boots, 5=Accessory)
        public string[] equippedItemIds = new string[6];

        // Inventory
        public List<ItemSaveEntry> inventoryItems = new();

        // Skill Tree
        public List<string> unlockedSkillNodes = new();

        // World Map
        public List<int> completedLevels = new();
        public int highestLevelUnlocked = 1;

        // Economy
        public int gold;

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

        /// <summary>XP needed for next level.</summary>
        public int XPToNextLevel => (int)(100 * (1f + characterLevel * 0.15f));

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
