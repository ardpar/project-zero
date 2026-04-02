namespace Synthborn.Core.Persistence
{
    /// <summary>
    /// Cross-scene runtime state that does NOT persist to disk.
    /// Replaces PlayerPrefs for scene-to-scene data passing.
    /// Reset when the application exits; survives scene loads.
    /// </summary>
    public static class RunSessionData
    {
        /// <summary>Selected chamber number for Trial system (0 = none).</summary>
        public static int SelectedChamber { get; set; }

        /// <summary>Selected level number for legacy level system (0 = none, 1+ = level).</summary>
        public static int SelectedLevel { get; set; } = 1;

        /// <summary>Shop purchase flags for current run.</summary>
        public static bool ShopHealPotion { get; set; }
        public static bool ShopXPScroll { get; set; }
        public static bool ShopLuckyCoin { get; set; }

        /// <summary>Reset all session state (call on return to main menu).</summary>
        public static void Reset()
        {
            SelectedChamber = 0;
            SelectedLevel = 1;
            ShopHealPotion = false;
            ShopXPScroll = false;
            ShopLuckyCoin = false;
        }
    }
}
