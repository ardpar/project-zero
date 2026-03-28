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

        // Settings
        public float masterVolume = 1f;
        public float sfxVolume = 1f;
        public bool screenShakeEnabled = true;
        public bool fullscreen = true;
    }
}
