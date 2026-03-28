using UnityEngine;

namespace Synthborn.Mutations
{
    /// <summary>
    /// ScriptableObject defining a single mutation.
    /// Slot mutations occupy a body slot and change the sprite.
    /// Passive mutations stack without limit (no duplicates).
    /// Create via Assets > Create > Synthborn/Mutations/Mutation Data.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Mutations/Mutation Data", fileName = "NewMutation")]
    public class MutationData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for code lookups.")]
        public string id;

        [Tooltip("Display name shown on mutation card.")]
        public string displayName;

        [Tooltip("Short description of the mutation's effect.")]
        [TextArea(2, 4)]
        public string description;

        [Header("Classification")]
        public MutationCategory category;
        [Tooltip("Body slot (only for Slot mutations). Set to None for passives.")]
        public MutationSlot slot;
        public MutationRarity rarity;

        [Header("Stat Modifiers (Additive — fed to CombatStatBlock)")]
        [Tooltip("Added to speed_modifier. GDD clamp: -0.5 to 2.0.")]
        public float speedModifier;
        [Tooltip("Added to dash_cd_modifier. GDD clamp: 0 to 0.6.")]
        public float dashCooldownModifier;
        [Tooltip("Added to hp_modifier. GDD clamp: -0.3 to 2.0.")]
        public float hpModifier;
        [Tooltip("Flat armor bonus. GDD clamp: 0 to 50.")]
        public int armorFlat;
        [Tooltip("Added to damage_modifier. GDD clamp: 0 to 3.0.")]
        public float damageModifier;
        [Tooltip("Added to crit_chance. GDD clamp: 0 to 0.5.")]
        public float critChance;
        [Tooltip("Added to crit_multiplier_bonus. Base is 2.0.")]
        public float critMultiplierBonus;
        [Tooltip("Added to attack_speed_modifier. GDD clamp: 0 to 0.9.")]
        public float attackSpeedModifier;

        [Header("Synergy")]
        [Tooltip("Tags used by Synergy Matrix to detect combos.")]
        public string[] synergyTags;

        [Header("Visual (Slot mutations)")]
        [Tooltip("Sprite overlay for the body slot. Null for passives.")]
        public Sprite slotSprite;

        [Header("UI")]
        [Tooltip("Card icon displayed in mutation selection.")]
        public Sprite icon;
    }
}
