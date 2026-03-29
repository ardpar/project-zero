using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
        [SerializeField] private Text _tooltipText;

        [Header("Merge")]
        [SerializeField] private Button _mergeButton;
        [SerializeField] private Text _mergeStatusText;

        private const int GridColumns = 5;
        private const float CellSize = 64f;
        private const float EquipSlotSize = 72f;

        private bool _mergeMode;
        private string _mergeFirstItemId;

        // Equipment slot positions (normalized within equipment container)
        // Layout like the reference image: head top-center, chest center,
        // arms left, hands right, legs bottom-center, weapon bottom-left, etc.
        private static readonly (ItemSlotType slot, Vector2 pos, string icon)[] EquipLayout = new[]
        {
            (ItemSlotType.Helmet,    new Vector2(0.50f, 0.82f), "\u26D1"),  // ⛑ Head — top center
            (ItemSlotType.Armor,     new Vector2(0.50f, 0.55f), "\u26E8"),  // ⛨ Chest — center
            (ItemSlotType.Weapon,    new Vector2(0.15f, 0.55f), "\u2694"),  // ⚔ Weapon — left
            (ItemSlotType.Gloves,    new Vector2(0.85f, 0.55f), "\u270B"),  // ✋ Hands — right
            (ItemSlotType.Boots,     new Vector2(0.50f, 0.22f), "\u2B25"),  // ⬥ Feet — bottom center
            (ItemSlotType.Accessory, new Vector2(0.85f, 0.22f), "\u2B50"),  // ⭐ Accessory — bottom right
        };

        public void Show()
        {
            if (_panel == null) return;
            _panel.SetActive(true);
            _mergeMode = false;
            _mergeFirstItemId = null;
            if (_mergeButton != null)
            {
                _mergeButton.onClick.RemoveAllListeners();
                _mergeButton.onClick.AddListener(ToggleMergeMode);
            }
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
                0 => "Dense Lattice", 1 => "Severed Thread", 2 => "Null Cascade", 3 => "Balanced Frame", _ => "?"
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
                $"<color=#FF6666>MASS</color>  {ch.statPoints[0]}  (+{totalDMG * 100:F0}% DMG)\n" +
                $"<color=#66FF66>RESILIENCE</color>  {ch.statPoints[1]}  (+{totalHP * 100:F0}% HP)\n" +
                $"<color=#6666FF>VELOCITY</color>  {ch.statPoints[2]}  (+{totalSPD * 100:F0}% SPD)\n" +
                $"<color=#FFFF66>VARIANCE</color>  {ch.statPoints[3]}  (+{totalCRIT * 100:F0}% CRIT)\n" +
                $"<color=#66FFFF>YIELD</color>  {ch.statPoints[4]}  (+{ch.statPoints[4] * 3:F0}% XP)\n\n" +
                $"Armor: {totalARM}\n" +
                $"Fragment: {ch.gold}\n" +
                $"XP: {ch.characterXP}/{ch.XPToNextLevel}\n" +
                $"Unspent Points: {ch.unspentStatPoints}";
        }

        // ─── Equipment Slots (Drop Targets) ───

        private void RefreshEquipment()
        {
            if (_equipmentContainer == null) return;
            foreach (Transform child in _equipmentContainer) Destroy(child.gameObject);

            foreach (var (slotType, pos, icon) in EquipLayout)
                CreateEquipSlot(slotType, InventoryManager.GetEquipped(slotType), pos, icon);
        }

        private void CreateEquipSlot(ItemSlotType slotType, ItemData equipped, Vector2 normalizedPos, string icon)
        {
            string slotName = ItemData.SlotName(slotType);

            // Square slot at fixed position
            var slotGO = new GameObject($"ESlot_{slotType}", typeof(RectTransform), typeof(Image));
            slotGO.transform.SetParent(_equipmentContainer, false);
            var rect = slotGO.GetComponent<RectTransform>();
            rect.anchorMin = normalizedPos;
            rect.anchorMax = normalizedPos;
            rect.sizeDelta = new Vector2(EquipSlotSize, EquipSlotSize);
            rect.pivot = new Vector2(0.5f, 0.5f);

            var bg = slotGO.GetComponent<Image>();
            bg.color = equipped != null
                ? new Color(0.18f, 0.16f, 0.25f)
                : new Color(0.08f, 0.08f, 0.12f);

            // Rarity border glow if equipped
            if (equipped != null)
            {
                var borderGO = new GameObject("Border", typeof(RectTransform), typeof(Image));
                borderGO.transform.SetParent(slotGO.transform, false);
                var bRect = borderGO.GetComponent<RectTransform>();
                bRect.anchorMin = Vector2.zero; bRect.anchorMax = Vector2.one;
                bRect.sizeDelta = Vector2.zero; bRect.offsetMin = Vector2.zero; bRect.offsetMax = Vector2.zero;
                var bImg = borderGO.GetComponent<Image>();
                bImg.color = new Color(equipped.RarityColor.r, equipped.RarityColor.g, equipped.RarityColor.b, 0.3f);
                bImg.raycastTarget = false;
                // Animated glow for Anomalous+ items
                if ((int)equipped.Rarity >= (int)ItemRarity.Anomalous)
                {
                    var glow = borderGO.AddComponent<RarityGlow>();
                    glow.Init(equipped.RarityColor, equipped.Rarity == ItemRarity.ArchitectGrade ? 3f : 2f);
                }
            }

            // Drop target
            var drop = slotGO.AddComponent<DropSlot>();
            drop.Mode = DropSlot.SlotMode.Equipment;
            drop.EquipSlotType = slotType;
            drop.OnItemDropped = OnItemDropped;

            if (equipped != null)
            {
                // Item name (center)
                MakeText(slotGO.transform, equipped.DisplayName, 9, equipped.RarityColor,
                    TextAnchor.MiddleCenter, new Vector2(0.05f, 0.25f), new Vector2(0.95f, 0.75f));

                // Draggable overlay
                CreateDraggableItem(slotGO.transform, equipped, true, (int)slotType,
                    new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f));
            }
            else
            {
                // Empty slot icon
                MakeText(slotGO.transform, icon, 22, new Color(0.25f, 0.25f, 0.3f),
                    TextAnchor.MiddleCenter, new Vector2(0f, 0.2f), new Vector2(1f, 0.85f));
            }

            // Slot label below
            MakeText(slotGO.transform, slotName, 8, new Color(0.5f, 0.5f, 0.55f),
                TextAnchor.LowerCenter, new Vector2(0f, -0.15f), new Vector2(1f, 0.15f));
        }

        // ─── Inventory Grid ───

        private void RefreshInventory()
        {
            if (_inventoryContainer == null) return;
            foreach (Transform child in _inventoryContainer) Destroy(child.gameObject);

            var items = InventoryManager.GetInventoryItemsSorted();
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
                new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.95f));

            // Sell button (small, bottom of cell)
            var sellGO = new GameObject("Sell", typeof(RectTransform), typeof(Image), typeof(Button));
            sellGO.transform.SetParent(cellGO.transform, false);
            var sRect = sellGO.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0.05f, 0f);
            sRect.anchorMax = new Vector2(0.95f, 0.15f);
            sRect.sizeDelta = Vector2.zero; sRect.offsetMin = Vector2.zero; sRect.offsetMax = Vector2.zero;
            sellGO.GetComponent<Image>().color = new Color(0.4f, 0.15f, 0.15f);
            var sellText = new GameObject("T", typeof(RectTransform), typeof(Text));
            sellText.transform.SetParent(sellGO.transform, false);
            var stRect = sellText.GetComponent<RectTransform>();
            stRect.anchorMin = Vector2.zero; stRect.anchorMax = Vector2.one; stRect.sizeDelta = Vector2.zero;
            var st = sellText.GetComponent<Text>();
            int price = InventoryManager.GetSellPrice(item.Rarity);
            st.text = $"{price}f"; st.fontSize = 7; st.color = new Color(1f, 0.85f, 0.3f);
            st.alignment = TextAnchor.MiddleCenter; st.font = _font; st.raycastTarget = false;
            var capturedId = item.Id;
            if (_mergeMode)
            {
                // In merge mode: clicking cell triggers merge selection
                var mergeBtn = cellGO.AddComponent<Button>();
                mergeBtn.onClick.AddListener(() => OnMergeItemClicked(capturedId));
                sellGO.SetActive(false);
            }
            else
            {
                sellGO.GetComponent<Button>().onClick.AddListener(() => {
                    InventoryManager.SellItem(capturedId);
                    Refresh();
                });
            }

            // Tooltip on hover
            var trigger = cellGO.AddComponent<EventTrigger>();
            var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            string tooltip = BuildItemTooltip(item);
            enterEntry.callback.AddListener((data) => ShowItemTooltip(tooltip));
            trigger.triggers.Add(enterEntry);
            var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener((data) => ShowItemTooltip(""));
            trigger.triggers.Add(exitEntry);
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

        // ─── Tooltip ───

        private void ShowItemTooltip(string text)
        {
            if (_tooltipText != null)
            {
                _tooltipText.text = text;
                _tooltipText.supportRichText = true;
            }
        }

        private string BuildItemTooltip(ItemData item)
        {
            string stats = BuildStatLine(item);
            int sellPrice = InventoryManager.GetSellPrice(item.Rarity);

            // Compare with currently equipped item in same slot
            string comparison = "";
            var equipped = InventoryManager.GetEquipped(item.SlotType);
            if (equipped != null && equipped.Id != item.Id)
            {
                comparison = "\n<color=#AAAAAA>vs Equipped:</color>";
                float dHP = item.HpModifier - equipped.HpModifier;
                float dDMG = item.DamageModifier - equipped.DamageModifier;
                float dSPD = item.SpeedModifier - equipped.SpeedModifier;
                float dCRT = item.CritChance - equipped.CritChance;
                int dARM = item.ArmorFlat - equipped.ArmorFlat;
                if (dHP != 0) comparison += $" HP{dHP:+0%;-0%}";
                if (dDMG != 0) comparison += $" DMG{dDMG:+0%;-0%}";
                if (dSPD != 0) comparison += $" SPD{dSPD:+0%;-0%}";
                if (dCRT != 0) comparison += $" CRT{dCRT:+0%;-0%}";
                if (dARM != 0) comparison += $" ARM{dARM:+0;-0}";
            }

            string rarityName = ItemData.RarityDisplayName(item.Rarity);
            return $"<b><color=#{ColorUtility.ToHtmlStringRGB(item.RarityColor)}>{item.DisplayName}</color></b>\n" +
                   $"[{rarityName}] {ItemData.SlotName(item.SlotType)}\n" +
                   $"{stats}\nSat: {sellPrice}f{comparison}";
        }

        // ─── Merge ───

        private void ToggleMergeMode()
        {
            _mergeMode = !_mergeMode;
            _mergeFirstItemId = null;
            UpdateMergeStatus(_mergeMode
                ? "Birle\u015ftirme modu: \u0130lk komponenti se\u00e7in"
                : "");
            Refresh();
        }

        private void OnMergeItemClicked(string itemId)
        {
            if (!_mergeMode) return;

            var itemDB = Resources.FindObjectsOfTypeAll<ItemDatabase>();
            if (itemDB.Length == 0) return;
            var db = itemDB[0];

            if (_mergeFirstItemId == null)
            {
                _mergeFirstItemId = itemId;
                var first = db.GetById(itemId);
                string rarityName = first != null ? ItemData.RarityDisplayName(first.Rarity) : "?";
                UpdateMergeStatus($"Se\u00e7ilen: {first?.DisplayName} [{rarityName}]\n\u0130kinci komponenti se\u00e7in (ayn\u0131 rarity)");
                return;
            }

            if (_mergeFirstItemId == itemId)
            {
                UpdateMergeStatus("<color=red>Ayn\u0131 komponenti se\u00e7emezsiniz!</color>");
                return;
            }

            var item1 = db.GetById(_mergeFirstItemId);
            var item2 = db.GetById(itemId);
            if (item1 == null || item2 == null) return;

            if (item1.Rarity != item2.Rarity)
            {
                UpdateMergeStatus("<color=red>Komponentler ayn\u0131 rarity olmal\u0131!</color>");
                _mergeFirstItemId = null;
                return;
            }

            if ((int)item1.Rarity >= (int)ItemRarity.ArchitectGrade)
            {
                UpdateMergeStatus("<color=red>Architect-Grade birle\u015ftirilemez!</color>");
                _mergeFirstItemId = null;
                return;
            }

            string resultId = CraftingManager.MergeComponents(db, _mergeFirstItemId, itemId);
            if (resultId != null)
            {
                var result = db.GetById(resultId);
                string name = result?.DisplayName ?? resultId;
                string rarityName = result != null ? ItemData.RarityDisplayName(result.Rarity) : "?";
                UpdateMergeStatus($"<color=green>Birle\u015ftirildi: {name} [{rarityName}]</color>");
            }
            else
            {
                UpdateMergeStatus("<color=red>Birle\u015ftirme ba\u015far\u0131s\u0131z!</color>");
            }

            _mergeFirstItemId = null;
            Refresh();
        }

        private void UpdateMergeStatus(string text)
        {
            if (_mergeStatusText != null)
                _mergeStatusText.text = text;
        }
    }
}
