using System.Collections.Generic;
using UnityEngine;
using Synthborn.Core.Persistence;

namespace Synthborn.Core.Items
{
    /// <summary>
    /// Manages character inventory and equipment.
    /// Reads/writes to CharacterSaveData.
    /// </summary>
    public static class InventoryManager
    {
        private static ItemDatabase _database;

        /// <summary>Set the item database reference (call once on game start).</summary>
        public static void SetDatabase(ItemDatabase db) => _database = db;

        /// <summary>Get equipped item in a slot. Null if empty.</summary>
        public static ItemData GetEquipped(ItemSlotType slot)
        {
            var ch = SaveManager.Character;
            if (ch == null || _database == null) return null;

            int idx = (int)slot;
            if (idx >= ch.equippedItemIds.Length) return null;

            string itemId = ch.equippedItemIds[idx];
            if (string.IsNullOrEmpty(itemId)) return null;

            return _database.GetById(itemId);
        }

        /// <summary>Equip an item. Moves previous item to inventory.</summary>
        public static void Equip(ItemData item)
        {
            var ch = SaveManager.Character;
            if (ch == null || item == null) return;

            int slotIdx = (int)item.SlotType;

            // Unequip current item in that slot (put back in inventory)
            string currentId = ch.equippedItemIds[slotIdx];
            if (!string.IsNullOrEmpty(currentId))
            {
                ch.inventoryItems.Add(new ItemSaveEntry { itemId = currentId });
            }

            // Equip new item
            ch.equippedItemIds[slotIdx] = item.Id;

            // Remove from inventory
            ch.inventoryItems.RemoveAll(e => e.itemId == item.Id);

            SaveManager.SaveSlot();
        }

        /// <summary>Unequip an item from a slot. Moves to inventory.</summary>
        public static void Unequip(ItemSlotType slot)
        {
            var ch = SaveManager.Character;
            if (ch == null) return;

            int slotIdx = (int)slot;
            string currentId = ch.equippedItemIds[slotIdx];
            if (string.IsNullOrEmpty(currentId)) return;

            ch.inventoryItems.Add(new ItemSaveEntry { itemId = currentId });
            ch.equippedItemIds[slotIdx] = "";

            SaveManager.SaveSlot();
        }

        /// <summary>Add an item to inventory (from loot drop).</summary>
        public static void AddToInventory(string itemId)
        {
            var ch = SaveManager.Character;
            if (ch == null) return;

            ch.inventoryItems.Add(new ItemSaveEntry { itemId = itemId });
            SaveManager.SaveSlot();
        }

        /// <summary>Get all inventory items as ItemData list.</summary>
        public static List<ItemData> GetInventoryItems()
        {
            var result = new List<ItemData>();
            var ch = SaveManager.Character;
            if (ch == null || _database == null) return result;

            foreach (var entry in ch.inventoryItems)
            {
                var item = _database.GetById(entry.itemId);
                if (item != null) result.Add(item);
            }
            return result;
        }

        /// <summary>Get total stat summary from all equipped items.</summary>
        public static string GetEquipmentStatSummary()
        {
            string summary = "";
            for (int i = 0; i < 6; i++)
            {
                var item = GetEquipped((ItemSlotType)i);
                if (item != null)
                    summary += $"{ItemData.SlotName((ItemSlotType)i)}: {item.DisplayName}\n";
                else
                    summary += $"{ItemData.SlotName((ItemSlotType)i)}: Empty\n";
            }
            return summary;
        }

        /// <summary>Apply all equipped item stats to a CombatStatBlock.</summary>
        public static void ApplyEquipmentToStats(Stats.CombatStatBlock stats)
        {
            for (int i = 0; i < 6; i++)
            {
                var item = GetEquipped((ItemSlotType)i);
                item?.ApplyToStats(stats);
            }
        }
    }
}
