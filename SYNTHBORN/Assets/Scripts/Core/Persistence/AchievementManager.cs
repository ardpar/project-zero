using System;
using UnityEngine;
using Synthborn.Core.Events;

namespace Synthborn.Core.Persistence
{
    /// <summary>
    /// Tracks and unlocks achievements based on game events and run statistics.
    /// 10 achievements with varied unlock conditions.
    /// </summary>
    public static class AchievementManager
    {
        /// <summary>Fired when an achievement is newly unlocked.</summary>
        public static event Action<AchievementDef> OnAchievementUnlocked;

        /// <summary>All achievement definitions.</summary>
        public static readonly AchievementDef[] All = new[]
        {
            new AchievementDef("first_win",        "First Victory",        "Win your first run"),
            new AchievementDef("kill_100",          "Centurion",            "Kill 100 enemies in total"),
            new AchievementDef("kill_500",          "Slayer",               "Kill 500 enemies in total"),
            new AchievementDef("wave_6",            "Wave Master",          "Reach wave 6 in a single run"),
            new AchievementDef("runs_5",            "Persistent",           "Complete 5 runs"),
            new AchievementDef("mutations_10",      "Mutant",               "Collect 10 mutations in a single run"),
            new AchievementDef("boss_60s",          "Speed Kill",           "Defeat the boss in under 60 seconds"),
            new AchievementDef("discover_all",      "Collector",            "Discover all mutations"),
            new AchievementDef("synergies_3",       "Synergist",            "Activate 3 synergies in a single run"),
            new AchievementDef("xp_1000",           "XP Hoarder",           "Collect 1000 XP in a single run"),
        };

        /// <summary>Check if an achievement is unlocked.</summary>
        public static bool IsUnlocked(string id) =>
            SaveManager.Data.unlockedAchievements.Contains(id);

        /// <summary>Try to unlock an achievement. Returns true if newly unlocked.</summary>
        public static bool TryUnlock(string id)
        {
            if (IsUnlocked(id)) return false;
            SaveManager.Data.unlockedAchievements.Add(id);
            SaveManager.Save();

            var def = System.Array.Find(All, a => a.Id == id);
            if (def != null) OnAchievementUnlocked?.Invoke(def);
            return true;
        }

        /// <summary>
        /// Call at end of each run to check cumulative and per-run achievements.
        /// </summary>
        public static void CheckRunEnd(float survivalTime, int kills, int wavesCleared,
            int mutationsAcquired, int synergiesTriggered, int totalXP, bool victory)
        {
            var data = SaveManager.Data;

            if (victory) TryUnlock("first_win");
            if (data.totalKills >= 100) TryUnlock("kill_100");
            if (data.totalKills >= 500) TryUnlock("kill_500");
            if (wavesCleared >= 6) TryUnlock("wave_6");
            if (data.totalRuns >= 5) TryUnlock("runs_5");
            if (mutationsAcquired >= 10) TryUnlock("mutations_10");
            if (synergiesTriggered >= 3) TryUnlock("synergies_3");
            if (totalXP >= 1000) TryUnlock("xp_1000");

            // Boss speed kill: check if victory and time < 60s after last wave
            // Approximate: if total survival < wave_count * avg_duration + 60
            if (victory && survivalTime < 600f) TryUnlock("boss_60s");
        }

        /// <summary>Check discovery-based achievements (call after mutation discovery).</summary>
        public static void CheckDiscovery(int totalMutations)
        {
            if (SaveManager.Data.discoveredMutationIds.Count >= totalMutations)
                TryUnlock("discover_all");
        }

        /// <summary>Number of unlocked achievements.</summary>
        public static int UnlockedCount => SaveManager.Data.unlockedAchievements.Count;

        /// <summary>Total achievements.</summary>
        public static int TotalCount => All.Length;
    }

    /// <summary>Achievement definition (immutable).</summary>
    public class AchievementDef
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }

        public AchievementDef(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}
