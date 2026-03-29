using System;
using UnityEngine;

namespace Synthborn.Waves
{
    public enum ModifierType
    {
        None,
        Enraged,      // Enemies +50% speed, -25% HP
        ArmoredTide,  // Enemies +50% HP, -25% speed
        EliteSurge,   // Double elite count
        Bloodrush,    // Each kill has 10% chance to spawn extra enemy
        GoldFever,    // 3x gold drops, enemies +30% damage
        Blessed,      // Player +20% HP regen between waves
        FogOfWar,     // Reduced arena radius (enemies spawn closer)
        Swarm,        // 2x spawn rate, enemies -40% HP
    }

    /// <summary>
    /// Modifier applied to a level to change gameplay rules.
    /// </summary>
    [Serializable]
    public class LevelModifier
    {
        public ModifierType type = ModifierType.None;
        [TextArea] public string description = "";

        /// <summary>Display name for UI.</summary>
        public string DisplayName => type switch
        {
            ModifierType.Enraged => "ENRAGED",
            ModifierType.ArmoredTide => "ARMORED TIDE",
            ModifierType.EliteSurge => "ELITE SURGE",
            ModifierType.Bloodrush => "BLOODRUSH",
            ModifierType.GoldFever => "GOLD FEVER",
            ModifierType.Blessed => "BLESSED",
            ModifierType.FogOfWar => "FOG OF WAR",
            ModifierType.Swarm => "SWARM",
            _ => ""
        };

        /// <summary>Color for UI display.</summary>
        public Color DisplayColor => type switch
        {
            ModifierType.Enraged => new Color(1f, 0.3f, 0.2f),
            ModifierType.ArmoredTide => new Color(0.5f, 0.5f, 0.7f),
            ModifierType.EliteSurge => new Color(0.9f, 0.6f, 0.1f),
            ModifierType.Bloodrush => new Color(0.8f, 0.1f, 0.1f),
            ModifierType.GoldFever => new Color(1f, 0.85f, 0.2f),
            ModifierType.Blessed => new Color(0.3f, 0.9f, 0.4f),
            ModifierType.FogOfWar => new Color(0.4f, 0.4f, 0.6f),
            ModifierType.Swarm => new Color(0.7f, 0.3f, 0.7f),
            _ => Color.white
        };

        /// <summary>Get a random modifier for infinite levels (level 6+).</summary>
        public static LevelModifier RandomForLevel(int level)
        {
            // No modifier for early levels
            if (level <= 5) return new LevelModifier { type = ModifierType.None };

            // Increasing chance of modifier with level
            float chance = Mathf.Clamp01((level - 5) * 0.05f); // 5% per level above 5, max 100%
            if (UnityEngine.Random.value > chance)
                return new LevelModifier { type = ModifierType.None };

            var types = (ModifierType[])Enum.GetValues(typeof(ModifierType));
            // Skip None (index 0)
            int idx = UnityEngine.Random.Range(1, types.Length);
            return new LevelModifier { type = types[idx] };
        }
    }
}
