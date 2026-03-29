using UnityEngine;

namespace Synthborn.Core
{
    /// <summary>
    /// Defines a character class with starting stat modifiers.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Core/ClassData")]
    public class ClassData : ScriptableObject
    {
        [field: SerializeField] public string ClassName { get; private set; } = "Warrior";
        [field: SerializeField, TextArea] public string Description { get; private set; } = "";

        [Header("Stat Modifiers")]
        [field: SerializeField] public float HpModifier { get; private set; }
        [field: SerializeField] public float DamageModifier { get; private set; }
        [field: SerializeField] public float SpeedModifier { get; private set; }
        [field: SerializeField] public float CritChance { get; private set; }
        [field: SerializeField] public int ArmorFlat { get; private set; }

        [Header("Visual")]
        [field: SerializeField] public Color ClassColor { get; private set; } = Color.white;

        /// <summary>Apply class modifiers to a CombatStatBlock.</summary>
        public void ApplyToStats(Stats.CombatStatBlock stats)
        {
            stats.ApplyMutation(
                hpModifier: HpModifier,
                damageModifier: DamageModifier,
                speedModifier: SpeedModifier,
                critChance: CritChance,
                armorFlat: ArmorFlat
            );
        }
    }
}
