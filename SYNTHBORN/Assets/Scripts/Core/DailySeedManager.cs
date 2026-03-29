using System;
using UnityEngine;

namespace Synthborn.Core
{
    /// <summary>
    /// Generates a deterministic daily seed from the current date.
    /// Call Initialize() at run start to seed Unity's Random.
    /// Same day = same spawn patterns for all players.
    /// </summary>
    public static class DailySeedManager
    {
        /// <summary>Today's seed value.</summary>
        public static int TodaySeed { get; private set; }

        /// <summary>
        /// Initialize Random with today's date-based seed.
        /// Call once at the start of each run.
        /// </summary>
        public static void Initialize()
        {
            var today = DateTime.UtcNow.Date;
            TodaySeed = today.Year * 10000 + today.Month * 100 + today.Day;
            UnityEngine.Random.InitState(TodaySeed);
        }

        /// <summary>
        /// Get a display string for the current seed (e.g., "2026-03-29").
        /// </summary>
        public static string SeedDisplayString =>
            DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
    }
}
