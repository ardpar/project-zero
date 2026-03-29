using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Persistence;
using Synthborn.Core.Items;

namespace Synthborn.UI
{
    /// <summary>
    /// Character screen with grid-based drag & drop inventory.
    /// Left: Stats + Equipment slots (drop targets).
    /// Right: Inventory grid (draggable items).
    /// Drag item from bag → equipment slot to equip.
    /// Drag item from equipment → bag to unequip.
    /// </summary>
    public class CharacterScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Font _font;

        [Header("Stats")]
        [SerializeField] private Text _charNameText;
        [SerializeField] private Text _statsText;

        [Header("Equipment")]
        [SerializeField] private Transform _equipmentContainer;

        [Header("Inventory")]
        [SerializeField] private Transform _inventoryContainer;
        [SerializeField] private Text _inventoryTitle;

        private const int GridColumns = 5;
        private const float CellSize = 64f;

        public void Show()
        {
            if (_panel == null) return;
            _panel.SetActive(true);
            Refresh();
            PopupEscHandler.Register(_panel, Hide);
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            PopupEscHandler.Unregister();
        }

        private void Refresh()
        {
            RefreshStats();
            RefreshEquipment();
            RefreshInventory();
        }

        // ─── Stats ───

        private void RefreshStats()
        {
            var ch = SaveManager.Character;
            if (ch == null) return;

            string className = ch.classType switch
            {
                0 => "Warrior", 1 => "Rogue", 2 => "Mage", 3 => "Sentinel", _ => "?"
            };

            if (_charNameText != null)
                _charNameText.text = $"{ch.characterName}  —  {className}  Lv.{ch.characterLevel}";

            if (_statsText == null) return;

            float totalHP = ch.statPoints[1] * 0.03f;
            float totalDMG = ch.statPoints[0] * 0.02f;
            float totalSPD = ch.statPoints[2] * 0.02f;
            float totalCRIT = ch.statPoints[3] * 0.01f;
            int totalARM = 0;

            for (int i = 0; i < 6; i++)
            {
                var item = InventoryManager.GetEquipped((ItemSlotType)i);
                if (item == null) continue;
                totalHP += item.HpModifier;
                totalDMG += item.DamageModifier;
                totalSPD += item.SpeedModifier;
                totalCRIT += item.CritChance;
                totalARM += item.ArmorFlat;
            }

            _statsText.text =
                $"<color=#FF6666>STR</color>  {ch.statPoints[0]}  (+{totalDMG * 100:F0}% DMG)\n" +
                $"<color=#66FF66>VIT</color>  {ch.statPoints[1]}  (+{totalHP * 100:F0}% HP)\n" +
                $"<color=#6666FF>AGI</color>  {ch.statPoints[2]}  (+{totalSPD * 100:F0}% SPD)\n" +
                $"<color=#FFFF66>LCK</color>  {ch.statPoints[3]}  (+{totalCRIT * 100:F0}% CRIT)\n" +
                $"<color=#66FFFF>WIS</color>  {ch.statPoints[4]}\n\n" +
                $"Armor: {totalARM}\n" +
                $"Gold: {ch.gold}\n" +
                $"XP: {ch.characterXP}/{ch.XPToNextLevel}\n" +
                $"Unspent Points: {ch.unspentStatPoints}";
        }

        // ─── Equipment Slots (Drop Targets) ───

        private void RefreshEquipment()
        {
            if (_equipmentContainer == null) return;
            foreach (Transform child in _equipmentContainer) Destroy(child.gameObject);

            for (int i = 0; i < 6; i++)
                CreateEquipSlot((ItemSlotType)i, InventoryManager.GetEquipped((ItemSlotType)i));
        }

        private void CreateEquipSlot(ItemSlotType slotType, ItemData equipped)
        {
            string slotName = ItemData.SlotName(slotType);

            var slotGO = new GameObject($"ESlot_{slotType}", typeof(RectTransform), typeof(Image));
            slotGO.transform.SetParent(_equipmentContainer, false);
            slotGO.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 50);
            slotGO.GetComponent<Image>().color = equipped != null
                ? new Color(0.20f, 0.18f, 0.28f)
                : new Color(0.10f, 0.10f, 0.14f);

            // Drop target
            var drop = slotGO.AddComponent<DropSlot>();
            drop.Mode = DropSlot.SlotMode.Equipment;
            drop.EquipSlotType = slotType;
            drop.OnItemDropped = OnItemDropped;

            // Slot label
            MakeText(slotGO.transform, slotName, 10, new Color(0.6f, 0.6f, 0.7f), TextAnchor.UpperLeft,
                new Vector2(0.02f, 0.6f), new Vector2(0.4f, 1f));

            if (equipped != null)
            {
                // Draggable equipped item
                CreateDraggableItem(slotGO.transform, equipped, true, (int)slotType,
                    new Vector2(0.25f, 0.05f), new Vector2(0.98f, 0.95f));

                // Stat line
                MakeText(slotGO.transform, BuildStatLine(equipped), 9, new Color(0.6f, 0.6f, 0.6f),
                    TextAnchor.LowerLeft, new Vector2(0.02f, 0f), new Vector2(0.98f, 0.4f));
            }
            else
            {
                MakeText(slotGO.transform, "drag item here", 11, new Color(0.3f, 0.3f, 0.35f),
                    TextAnchor.MiddleCenter, new Vector2(0.25f, 0f), new Vector2(0.98f, 1f));
            }
        }

        // ─── Inventory Grid ───

        private void RefreshInventory()
        {
            if (_inventoryContainer == null) return;
            foreach (Transform child in _inventoryContainer) Destroy(child.gameObject);

            var items = InventoryManager.GetInventoryItems();
            if (_inventoryTitle != null)
                _inventoryTitle.text = $"BAG ({items.Count})";

            for (int i = 0; i < items.Count; i++)
                CreateGridCell(items[i], i);

            int totalCells = Mathf.Max(20, ((items.Count / GridColumns) + 2) * GridColumns);
            for (int i = items.Count; i < totalCells; i++)
                CreateEmptyGridCell(i);
        }

        private void CreateGridCell(ItemData item, int index)
        {
            var cellGO = new GameObject($"Cell_{index}", typeof(RectTransform), typeof(Image));
            cellGO.transform.SetParent(_inventoryContainer, false);
            cellGO.GetComponent<RectTransform>().sizeDelta = new Vector2(CellSize, CellSize);
            cellGO.GetComponent<Image>().color = new Color(0.14f, 0.13f, 0.18f);

            var drop = cellGO.AddComponent<DropSlot>();
            drop.Mode = DropSlot.SlotMode.Inventory;
            drop.GridIndex = index;
            drop.OnItemDropped = OnItemDropped;

            CreateDraggableItem(cellGO.transform, item, false, index,
                new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f));
        }

        private void CreateEmptyGridCell(int index)
        {
            var cellGO = new GameObject($"Cell_{index}", typeof(RectTransform), typeof(Image));
            cellGO.transform.SetParent(_inventoryContainer, false);
            cellGO.GetComponent<RectTransform>().sizeDelta = new Vector2(CellSize, CellSize);
            cellGO.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.10f);

            var drop = cellGO.AddComponent<DropSlot>();
            drop.Mode = DropSlot.SlotMode.Inventory;
            drop.GridIndex = index;
            drop.OnItemDropped = OnItemDropped;
        }

        private void CreateDraggableItem(Transform parent, ItemData item, bool isEquipped, int slotIndex,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject("Item", typeof(RectTransform), typeof(Image),
                typeof(CanvasGroup), typeof(DragDropItem));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            go.GetComponent<Image>().color = DarkenColor(item.RarityColor, 0.5f);

            var textGO = new GameObject("Name", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(go.transform, false);
            var tRect = textGO.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero; tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;
            tRect.offsetMin = new Vector2(2, 1); tRect.offsetMax = new Vector2(-2, -1);
            var text = textGO.GetComponent<Text>();
            text.text = isEquipped
                ? item.DisplayName
                : $"{item.DisplayName}\n<size=7>[{ItemData.SlotName(item.SlotType)}]</size>";
            text.fontSize = isEquipped ? 12 : 9;
            text.color = item.RarityColor;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = _font;
            text.supportRichText = true;
            text.raycastTarget = false;

            var drag = go.GetComponent<DragDropItem>();
            drag.ItemId = item.Id;
            drag.IsEquipped = isEquipped;
            drag.SourceSlotIndex = slotIndex;
        }

        // ─── Drop Handling ───

        private void OnItemDropped(DragDropItem dragItem, DropSlot targetSlot)
        {
            if (dragItem == null || targetSlot == null) return;

            var itemDB = Resources.FindObjectsOfTypeAll<ItemDatabase>();
            if (itemDB.Length > 0) InventoryManager.SetDatabase(itemDB[0]);

            var item = itemDB.Length > 0 ? itemDB[0].GetById(dragItem.ItemId) : null;
            if (item == null) return;

            if (dragItem.IsEquipped && targetSlot.Mode == DropSlot.SlotMode.Inventory)
            {
                InventoryManager.Unequip((ItemSlotType)dragItem.SourceSlotIndex);
            }
            else if (!dragItem.IsEquipped && targetSlot.Mode == DropSlot.SlotMode.Equipment)
            {
                if (item.SlotType == targetSlot.EquipSlotType)
                    InventoryManager.Equip(item);
            }

            Refresh();
        }

        // ─── Helpers ───

        private GameObject MakeText(Transform parent, string text, int size, Color color,
            TextAnchor align, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject("Txt", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax;
            rect.sizeDelta = Vector2.zero; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            var t = go.GetComponent<Text>();
            t.text = text; t.fontSize = size; t.color = color;
            t.alignment = align; t.font = _font; t.raycastTarget = false;
            return go;
        }

        private static string BuildStatLine(ItemData item)
        {
            var parts = new List<string>();
            if (item.HpModifier != 0) parts.Add($"HP{item.HpModifier:+0%;-0%}");
            if (item.DamageModifier != 0) parts.Add($"DMG{item.DamageModifier:+0%;-0%}");
            if (item.SpeedModifier != 0) parts.Add($"SPD{item.SpeedModifier:+0%;-0%}");
            if (item.CritChance != 0) parts.Add($"CRT{item.CritChance:+0%;-0%}");
            if (item.ArmorFlat != 0) parts.Add($"ARM+{item.ArmorFlat}");
            if (item.AttackSpeedModifier != 0) parts.Add($"ASPD{item.AttackSpeedModifier:+0%;-0%}");
            return string.Join(" ", parts);
        }

        private static Color DarkenColor(Color c, float factor) =>
            new Color(c.r * factor, c.g * factor, c.b * factor, 0.9f);
    }
}
