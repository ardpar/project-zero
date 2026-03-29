using UnityEngine;

namespace Synthborn.Core
{
    /// <summary>
    /// Defines a starter form with base stat modifiers applied at run start.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Core/StarterFormData")]
    public class StarterFormData : ScriptableObject
    {
        [field: SerializeField] public string FormName { get; private set; } = "Balanced";
        [field: SerializeField, TextArea] public string Description { get; private set; } = "Standard stats.";

        [Header("Stat Modifiers (applied at run start)")]
        [field: SerializeField] public float HpModifier { get; private set; }
        [field: SerializeField] public float SpeedModifier { get; private set; }
        [field: SerializeField] public float DamageModifier { get; private set; }
        [field: SerializeField] public float CritChance { get; private set; }
        [field: SerializeField] public int ArmorFlat { get; private set; }

        [Header("Unlock")]
        [field: SerializeField, Tooltip("Achievement ID required to unlock. Empty = always available.")]
        public string RequiredAchievement { get; private set; } = "";

        /// <summary>Apply this form's modifiers to a CombatStatBlock.</summary>
        public void ApplyToStats(Stats.CombatStatBlock stats)
        {
            stats.ApplyMutation(
                hpModifier: HpModifier,
                speedModifier: SpeedModifier,
                damageModifier: DamageModifier,
                critChance: CritChance,
                armorFlat: ArmorFlat
            );
        }

        /// <summary>Check if this form is unlocked.</summary>
        public bool IsUnlocked()
        {
            if (string.IsNullOrEmpty(RequiredAchievement)) return true;
            return Persistence.AchievementManager.IsUnlocked(RequiredAchievement);
        }
    }
}
