using UnityEngine;

namespace Synthborn.Progression
{
    /// <summary>Configuration for XP gem behavior.</summary>
    [CreateAssetMenu(menuName = "Synthborn/Progression/XPConfig")]
    public class XPConfig : ScriptableObject
    {
        [Tooltip("Radius at which gems start being magnetized to player.")]
        public float pickupRadius = 1.0f;

        [Tooltip("Speed of magnet pull (units/sec).")]
        public float magnetSpeed = 15f;

        [Tooltip("Seconds before uncollected gem disappears.")]
        public float gemLifetime = 30f;

        [Tooltip("Maximum active gems on screen.")]
        public int maxActiveGems = 500;

        [Tooltip("Distance at which gem is considered collected.")]
        public float collectDistance = 0.3f;
    }
}
