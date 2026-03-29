using UnityEngine;

namespace Synthborn.Core.Persistence
{
    /// <summary>
    /// Tracks Substrate Fragments earned during a run. Fragments are the Arena's
    /// primary currency — spent at Calibration Intervals and the Synthesis Lab.
    /// Drop amounts scale with chamber pressure.
    /// </summary>
    public static class FragmentManager
    {
        /// <summary>Fragments earned in current run.</summary>
        public static int RunFragments { get; private set; }

        /// <summary>Fragment drop amounts by enemy tier.</summary>
        private static readonly int[] TierFragMin = { 1, 5, 50 };  // Normal, Elite, Stabilized
        private static readonly int[] TierFragMax = { 3, 10, 100 };

        /// <summary>Reset fragments for a new run. Loads from character save if available.</summary>
        public static void ResetRun()
        {
            var ch = SaveManager.Character;
            RunFragments = ch != null ? ch.gold : 0;
        }

        /// <summary>Add fragments from a kill or reward.</summary>
        public static void AddFragments(int amount)
        {
            RunFragments += amount;
            Events.GameEvents.FragmentChanged(RunFragments);
        }

        /// <summary>Spend fragments (shop, synthesis). Returns false if insufficient.</summary>
        public static bool TrySpend(int amount)
        {
            if (RunFragments < amount) return false;
            RunFragments -= amount;
            Events.GameEvents.FragmentChanged(RunFragments);
            return true;
        }

        /// <summary>Calculate fragment drop for an enemy tier, scaled by pressure.</summary>
        public static int RollFragmentDrop(int tierIndex, float pressureMultiplier = 1f)
        {
            int idx = Mathf.Clamp(tierIndex, 0, TierFragMin.Length - 1);
            int baseAmount = Random.Range(TierFragMin[idx], TierFragMax[idx] + 1);
            return Mathf.RoundToInt(baseAmount * pressureMultiplier);
        }

        /// <summary>Convert remaining run fragments to meta-fragments (20%) on run end.</summary>
        public static int ConvertToMeta()
        {
            int metaFragments = Mathf.RoundToInt(RunFragments * 0.2f);
            return metaFragments;
        }

        // ─── Backward compatibility ───

        /// <summary>Legacy alias for AddFragments.</summary>
        public static void AddGold(int amount) => AddFragments(amount);

        /// <summary>Legacy alias for RunFragments.</summary>
        public static int RunGold => RunFragments;
    }
}
