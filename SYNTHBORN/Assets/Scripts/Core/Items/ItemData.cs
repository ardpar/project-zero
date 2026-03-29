using UnityEngine;

namespace Synthborn.Core.Items
{
    public enum ItemSlotType { Helmet, Armor, Weapon, Gloves, Boots, Accessory }
    public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }

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
            ItemRarity.Common => Color.white,
            ItemRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
            ItemRarity.Rare => new Color(0.3f, 0.5f, 1f),
            ItemRarity.Epic => new Color(0.7f, 0.3f, 0.9f),
            ItemRarity.Legendary => new Color(1f, 0.85f, 0.2f),
            _ => Color.white
        };

        /// <summary>Slot display name.</summary>
        public static string SlotName(ItemSlotType slot) => slot switch
        {
            ItemSlotType.Helmet => "Helmet",
            ItemSlotType.Armor => "Armor",
            ItemSlotType.Weapon => "Weapon",
            ItemSlotType.Gloves => "Gloves",
            ItemSlotType.Boots => "Boots",
            ItemSlotType.Accessory => "Accessory",
            _ => "?"
        };
    }
}
