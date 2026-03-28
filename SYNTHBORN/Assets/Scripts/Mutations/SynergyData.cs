using UnityEngine;

namespace Synthborn.Mutations
{
    /// <summary>
    /// Defines a single synergy: required tags and stat bonuses when activated.
    /// Activated automatically when all required tags are present in active mutations.
    /// Create via Assets > Create > Synthborn/Mutations/Synergy Data.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Mutations/Synergy Data", fileName = "NewSynergy")]
    public class SynergyData : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        [TextArea(2, 3)]
        public string description;

        [Header("Requirements")]
        [Tooltip("All of these tags must be present in active mutations to trigger this synergy.")]
        public string[] requiredTags;

        [Header("Bonus Stats (Additive — stacks with mutation stats)")]
        public float speedModifier;
        public float dashCooldownModifier;
        public float hpModifier;
        public int armorFlat;
        public float damageModifier;
        public float critChance;
        public float critMultiplierBonus;
        public float attackSpeedModifier;
    }
}
