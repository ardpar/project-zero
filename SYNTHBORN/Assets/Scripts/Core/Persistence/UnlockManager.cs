using System.Collections.Generic;
using UnityEngine;

namespace Synthborn.Core.Persistence
{
    /// <summary>
    /// Manages mutation unlock state. Reads from SaveData, filters MutationPool.
    /// </summary>
    public static class UnlockManager
    {
        /// <summary>Check if a mutation is unlocked.</summary>
        public static bool IsUnlocked(string mutationId) =>
            SaveManager.Data.unlockedMutationIds.Contains(mutationId);

        /// <summary>Try to unlock a mutation. Returns false if not enough cells or already unlocked.</summary>
        public static bool TryUnlock(string mutationId, int cost)
        {
            var data = SaveManager.Data;
            if (data.unlockedMutationIds.Contains(mutationId)) return false;
            if (data.totalCells < cost) return false;

            data.totalCells -= cost;
            data.unlockedMutationIds.Add(mutationId);
            SaveManager.Save();
            return true;
        }

        /// <summary>Add cells earned from a run.</summary>
        public static void AddCells(int amount)
        {
            SaveManager.Data.totalCells += amount;
            SaveManager.Save();
        }

        /// <summary>Calculate cells earned from a run.</summary>
        public static int CalculateRunReward(int kills, int wavesCleared, bool victory)
        {
            return Mathf.RoundToInt(kills * 0.1f) + wavesCleared * 5 + (victory ? 50 : 0);
        }

        /// <summary>Record run completion stats.</summary>
        public static void RecordRun(float survivalTime, int wavesCleared, int kills, int level, int mutations, int cells, bool victory)
        {
            var data = SaveManager.Data;
            data.totalRuns++;
            data.totalKills += kills;
            if (survivalTime > data.bestSurvivalTime) data.bestSurvivalTime = survivalTime;
            if (wavesCleared > data.bestWavesCleared) data.bestWavesCleared = wavesCleared;

            // Add to history (keep last 10)
            data.runHistory.Add(new RunHistoryEntry
            {
                date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                survivalTime = survivalTime,
                enemiesKilled = kills,
                finalLevel = level,
                wavesCleared = wavesCleared,
                mutationsAcquired = mutations,
                cellsEarned = cells,
                victory = victory
            });
            if (data.runHistory.Count > 10)
                data.runHistory.RemoveAt(0);

            SaveManager.Save();
        }

        /// <summary>Mark a mutation as discovered (for collection screen).</summary>
        public static void DiscoverMutation(string mutationId)
        {
            var data = SaveManager.Data;
            if (!data.discoveredMutationIds.Contains(mutationId))
            {
                data.discoveredMutationIds.Add(mutationId);
                SaveManager.Save();
            }
        }

        public static bool IsDiscovered(string mutationId) =>
            SaveManager.Data.discoveredMutationIds.Contains(mutationId);

        /// <summary>Get unlock cost based on rarity index (0=Common, 1=Uncommon, 2=Rare, 3=Legendary).</summary>
        public static int GetUnlockCost(int rarityIndex)
        {
            return rarityIndex switch
            {
                0 => 10,   // Common
                1 => 25,   // Uncommon
                2 => 50,   // Rare
                3 => 100,  // Legendary
                _ => 10
            };
        }

        /// <summary>Get list of unlocked mutation IDs.</summary>
        public static IReadOnlyList<string> UnlockedIds => SaveManager.Data.unlockedMutationIds;

        /// <summary>Current cell balance.</summary>
        public static int Cells => SaveManager.Data.totalCells;
    }
}
