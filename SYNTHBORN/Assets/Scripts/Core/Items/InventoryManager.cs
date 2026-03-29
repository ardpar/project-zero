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

            // Unequip current item — preserve original ItemSaveEntry data
            string currentId = ch.equippedItemIds[slotIdx];
            if (!string.IsNullOrEmpty(currentId))
            {
                // Find the item's original save entry to preserve rarity/stats
                var originalEntry = ch.inventoryItems.Find(e => e.itemId == currentId);
                if (originalEntry != null)
                    ch.inventoryItems.Add(originalEntry);
                else
                    ch.inventoryItems.Add(new ItemSaveEntry { itemId = currentId });
            }

            // Remove the item being equipped from inventory
            int removeIdx = ch.inventoryItems.FindIndex(e => e.itemId == item.Id);
            if (removeIdx >= 0) ch.inventoryItems.RemoveAt(removeIdx);

            // Equip new item
            ch.equippedItemIds[slotIdx] = item.Id;

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

        /// <summary>Sell an item from inventory for gold.</summary>
        /// <summary>Remove an item from inventory without selling (used by merge).</summary>
        public static bool RemoveFromInventory(string itemId)
        {
            var ch = SaveManager.Character;
            if (ch == null) return false;
            int idx = ch.inventoryItems.FindIndex(e => e.itemId == itemId);
            if (idx < 0) return false;
            ch.inventoryItems.RemoveAt(idx);
            return true;
        }

        public static bool SellItem(string itemId)
        {
            var ch = SaveManager.Character;
            if (ch == null || _database == null) return false;

            int idx = ch.inventoryItems.FindIndex(e => e.itemId == itemId);
            if (idx < 0) return false;

            var item = _database.GetById(itemId);
            int sellPrice = item != null ? GetSellPrice(item.Rarity) : 5;

            ch.inventoryItems.RemoveAt(idx);
            ch.gold += sellPrice;
            SaveManager.SaveSlot();
            return true;
        }

        /// <summary>Gold value for selling an item by rarity.</summary>
        public static int GetSellPrice(ItemRarity rarity) => rarity switch
        {
            ItemRarity.Baseline => 10,
            ItemRarity.Calibrated => 25,
            ItemRarity.Reinforced => 75,
            ItemRarity.Anomalous => 200,
            ItemRarity.ArchitectGrade => 500,
            _ => 5
        };

        /// <summary>Get inventory items sorted by slot type then rarity (descending).</summary>
        public static List<ItemData> GetInventoryItemsSorted()
        {
            var items = GetInventoryItems();
            items.Sort((a, b) =>
            {
                int slotCmp = ((int)a.SlotType).CompareTo((int)b.SlotType);
                if (slotCmp != 0) return slotCmp;
                return ((int)b.Rarity).CompareTo((int)a.Rarity); // higher rarity first
            });
            return items;
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
