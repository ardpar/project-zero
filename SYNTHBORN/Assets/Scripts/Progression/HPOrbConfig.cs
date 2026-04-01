using UnityEngine;

namespace Synthborn.Progression
{
    /// <summary>
    /// ScriptableObject for HP Orb tuning parameters.
    /// Centralizes all tuning for health pickup behaviour.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Progression/HP Orb Config", fileName = "HPOrbConfig")]
    public class HPOrbConfig : ScriptableObject
    {
        [Tooltip("Radius at which the orb starts magnetizing to the player. Default 1.5.")]
        [Range(0.5f, 5f)]
        public float pickupRadius = 1.5f;

        [Tooltip("Speed at which the orb moves toward the player when magnetized. Default 12.")]
        [Range(5f, 25f)]
        public float magnetSpeed = 12f;

        [Tooltip("Distance at which the orb is collected. Default 0.3.")]
        [Range(0.1f, 1f)]
        public float collectDistance = 0.3f;

        [Tooltip("Seconds before the orb despawns. Default 15.")]
        [Range(5f, 60f)]
        public float lifetime = 15f;

        [Tooltip("HP healed on collection. Default 10.")]
        [Range(1, 100)]
        public int healAmount = 10;
    }
}
