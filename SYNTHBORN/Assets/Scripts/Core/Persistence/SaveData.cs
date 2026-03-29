using System;
using System.Collections.Generic;

namespace Synthborn.Core.Persistence
{
    /// <summary>
    /// Serializable save data structure. Stored as JSON.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int totalCells;
        public List<string> unlockedMutationIds = new();
        public int totalRuns;
        public int totalKills;
        public float bestSurvivalTime;
        public int bestWavesCleared;

        // Run History (last 10)
        public List<RunHistoryEntry> runHistory = new();

        // Discovered mutations (for collection screen)
        public List<string> discoveredMutationIds = new();

        // Stat Upgrades (index: 0=HP, 1=Speed, 2=XPGain, 3=Crit, 4=Armor)
        public int[] upgradeLevels = new int[5];

        // Achievements (unlocked achievement IDs)
        public List<string> unlockedAchievements = new();

        // Starter form (0=Balanced, 1=Berserker, 2=Speedster)
        public int selectedStarterForm;

        // Tutorial
        public bool tutorialCompleted;

        // Settings
        public float masterVolume = 1f;
        public float musicVolume = 0.4f;
        public float sfxVolume = 1f;
        public bool screenShakeEnabled = true;
        public bool fullscreen = true;
    }
}
