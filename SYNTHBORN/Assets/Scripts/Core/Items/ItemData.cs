using UnityEngine;

namespace Synthborn.Core.Items
{
    public enum ItemSlotType { Helmet, Armor, Weapon, Gloves, Boots, Accessory }

    /// <summary>
    /// Component integrity classification (Arena terminology).
    /// Int values preserved for save compatibility: 0=Baseline, 1=Calibrated, 2=Reinforced, 3=Anomalous, 4=Architect-Grade.
    /// </summary>
    public enum ItemRarity
    {
        Baseline = 0,       // was Common
        Calibrated = 1,     // was Uncommon
        Reinforced = 2,     // was Rare
        Anomalous = 3,      // was Epic
        ArchitectGrade = 4  // was Legendary
    }

    /// <summary>
    /// Defines an equippable item with stat modifiers and rarity.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Items/ItemData")]
    public class ItemData : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; } = "item_id";
        [field: SerializeField] public string DisplayName { get; private set; } = "Item";
        [field: SerializeField, TextArea] public string Description { get; private set; } = "";
        [field: SerializeField] public ItemSlotType SlotType { get; private set; }
        [field: SerializeField] public ItemRarity Rarity { get; private set; }

        [Header("Stat Modifiers")]
        [field: SerializeField] public float HpModifier { get; private set; }
        [field: SerializeField] public float DamageModifier { get; private set; }
        [field: SerializeField] public float SpeedModifier { get; private set; }
        [field: SerializeField] public float CritChance { get; private set; }
        [field: SerializeField] public float AttackSpeedModifier { get; private set; }
        [field: SerializeField] public int ArmorFlat { get; private set; }
        [field: SerializeField] public float DashCooldownModifier { get; private set; }

        /// <summary>Apply this item's stats to a CombatStatBlock.</summary>
        public void ApplyToStats(Stats.CombatStatBlock stats)
        {
            stats.ApplyMutation(
                hpModifier: HpModifier,
                damageModifier: DamageModifier,
                speedModifier: SpeedModifier,
                critChance: CritChance,
                attackSpeedModifier: AttackSpeedModifier,
                armorFlat: ArmorFlat,
                dashCdModifier: DashCooldownModifier
            );
        }

        /// <summary>Remove this item's stats from a CombatStatBlock.</summary>
        public void RemoveFromStats(Stats.CombatStatBlock stats)
        {
            stats.RemoveMutation(
                hpModifier: HpModifier,
                damageModifier: DamageModifier,
                speedModifier: SpeedModifier,
                critChance: CritChance,
                attackSpeedModifier: AttackSpeedModifier,
                armorFlat: ArmorFlat,
                dashCdModifier: DashCooldownModifier
            );
        }

        /// <summary>Rarity color for UI display.</summary>
        public Color RarityColor => Rarity switch
        {
            ItemRarity.Baseline => new Color(0.7f, 0.7f, 0.7f),
            ItemRarity.Calibrated => new Color(0.2f, 0.8f, 0.2f),
            ItemRarity.Reinforced => new Color(0.3f, 0.5f, 1f),
            ItemRarity.Anomalous => new Color(0.7f, 0.3f, 0.9f),
            ItemRarity.ArchitectGrade => new Color(1f, 0.85f, 0.2f),
            _ => Color.white
        };

        /// <summary>Display name for rarity (Arena terminology).</summary>
        public static string RarityDisplayName(ItemRarity r) => r switch
        {
            ItemRarity.Baseline => "Baseline",
            ItemRarity.Calibrated => "Calibrated",
            ItemRarity.Reinforced => "Reinforced",
            ItemRarity.Anomalous => "Anomalous",
            ItemRarity.ArchitectGrade => "Architect-Grade",
            _ => "?"
        };

        /// <summary>Slot display name.</summary>
        public static string SlotName(ItemSlotType slot) => slot switch
        {
            ItemSlotType.Helmet => "Cranial Module",
            ItemSlotType.Armor => "Carapace Plate",
            ItemSlotType.Weapon => "Appendage Core",
            ItemSlotType.Gloves => "Sensory Array",
            ItemSlotType.Boots => "Locomotion Frame",
            ItemSlotType.Accessory => "Auxiliary Port",
            _ => "?"
        };
    }
}
