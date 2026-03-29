using System;

namespace Synthborn.Core.Persistence
{
    [Serializable]
    public class RunHistoryEntry
    {
        public string date;
        public float survivalTime;
        public int enemiesKilled;
        public int finalLevel;
        public int wavesCleared;
        public int mutationsAcquired;
        public int cellsEarned;
        public int levelReached;
        public bool victory;
    }
}
