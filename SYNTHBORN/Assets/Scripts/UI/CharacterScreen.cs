using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Persistence;
using Synthborn.Core.Items;

namespace Synthborn.UI
{
    /// <summary>
    /// Character screen: stats, equipment slots, inventory bag.
    /// Accessible from WorldMap.
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

            // Calculate total stats from base + equipment
            float totalHP = ch.statPoints[1] * 0.03f;   // VIT
            float totalDMG = ch.statPoints[0] * 0.02f;  // STR
            float totalSPD = ch.statPoints[2] * 0.02f;  // AGI
            float totalCRIT = ch.statPoints[3] * 0.01f;  // LCK
            int totalARM = 0;

            // Add equipment stats
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

        private void RefreshEquipment()
        {
            if (_equipmentContainer == null) return;
            foreach (Transform child in _equipmentContainer) Destroy(child.gameObject);

            for (int i = 0; i < 6; i++)
            {
                var slotType = (ItemSlotType)i;
                var equipped = InventoryManager.GetEquipped(slotType);
                CreateEquipSlot(slotType, equipped);
            }
        }

        private void CreateEquipSlot(ItemSlotType slotType, ItemData equipped)
        {
            var slotGO = new GameObject($"Slot_{slotType}", typeof(RectTransform), typeof(Image));
            slotGO.transform.SetParent(_equipmentContainer, false);
            var rect = slotGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280, 40);
            var img = slotGO.GetComponent<Image>();
            img.color = equipped != null
                ? new Color(0.2f, 0.18f, 0.28f)
                : new Color(0.12f, 0.12f, 0.15f);
            img.raycastTarget = false;

            // Slot label
            string slotName = ItemData.SlotName(slotType);
            string itemName = equipped != null ? equipped.DisplayName : "— Empty —";
            Color textColor = equipped != null ? equipped.RarityColor : Color.gray;

            var textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(slotGO.transform, false);
            var tRect = textGO.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero; tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;
            tRect.offsetMin = new Vector2(8, 0); tRect.offsetMax = new Vector2(-8, 0);
            var text = textGO.GetComponent<Text>();
            text.text = $"<color=white>{slotName}:</color> {itemName}";
            text.fontSize = 13;
            text.color = textColor;
            text.alignment = TextAnchor.MiddleLeft;
            text.font = _font;
            text.supportRichText = true;
            text.raycastTarget = false;

            // Unequip button (if equipped)
            if (equipped != null)
            {
                var unequipGO = new GameObject("Unequip", typeof(RectTransform), typeof(Image), typeof(Button));
                unequipGO.transform.SetParent(slotGO.transform, false);
                var uRect = unequipGO.GetComponent<RectTransform>();
                uRect.anchorMin = new Vector2(1, 0); uRect.anchorMax = new Vector2(1, 1);
                uRect.pivot = new Vector2(1, 0.5f);
                uRect.sizeDelta = new Vector2(30, 0);
                uRect.anchoredPosition = Vector2.zero;
                unequipGO.GetComponent<Image>().color = new Color(0.5f, 0.2f, 0.2f);
                var uText = new GameObject("X", typeof(RectTransform), typeof(Text));
                uText.transform.SetParent(unequipGO.transform, false);
                var uxRect = uText.GetComponent<RectTransform>();
                uxRect.anchorMin = Vector2.zero; uxRect.anchorMax = Vector2.one; uxRect.sizeDelta = Vector2.zero;
                var ux = uText.GetComponent<Text>();
                ux.text = "X"; ux.fontSize = 14; ux.color = Color.white;
                ux.alignment = TextAnchor.MiddleCenter; ux.font = _font; ux.raycastTarget = false;

                var capturedSlot = slotType;
                unequipGO.GetComponent<Button>().onClick.AddListener(() => {
                    InventoryManager.Unequip(capturedSlot);
                    Refresh();
                });
            }
        }

        private void RefreshInventory()
        {
            if (_inventoryContainer == null) return;
            foreach (Transform child in _inventoryContainer) Destroy(child.gameObject);

            var items = InventoryManager.GetInventoryItems();

            if (_inventoryTitle != null)
                _inventoryTitle.text = $"INVENTORY ({items.Count})";

            if (items.Count == 0)
            {
                CreateText(_inventoryContainer, "No items yet. Complete levels to earn loot!", Color.gray);
                return;
            }

            foreach (var item in items)
                CreateInventoryItem(item);
        }

        private void CreateInventoryItem(ItemData item)
        {
            var go = new GameObject(item.Id, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_inventoryContainer, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280, 50);
            go.GetComponent<Image>().color = new Color(0.15f, 0.14f, 0.2f);

            // Item info
            string slotLabel = ItemData.SlotName(item.SlotType);
            string stats = "";
            if (item.HpModifier != 0) stats += $"HP{item.HpModifier:+0%;-0%} ";
            if (item.DamageModifier != 0) stats += $"DMG{item.DamageModifier:+0%;-0%} ";
            if (item.SpeedModifier != 0) stats += $"SPD{item.SpeedModifier:+0%;-0%} ";
            if (item.CritChance != 0) stats += $"CRT{item.CritChance:+0%;-0%} ";
            if (item.ArmorFlat != 0) stats += $"ARM+{item.ArmorFlat} ";
            if (item.AttackSpeedModifier != 0) stats += $"ASPD{item.AttackSpeedModifier:+0%;-0%} ";

            var textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(go.transform, false);
            var tRect = textGO.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero; tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;
            tRect.offsetMin = new Vector2(8, 2); tRect.offsetMax = new Vector2(-60, -2);
            var text = textGO.GetComponent<Text>();
            text.text = $"{item.DisplayName} <size=10>[{slotLabel}]</size>\n<size=10>{stats}</size>";
            text.fontSize = 13;
            text.color = item.RarityColor;
            text.alignment = TextAnchor.MiddleLeft;
            text.font = _font;
            text.supportRichText = true;
            text.raycastTarget = false;

            // Equip button
            var equipGO = new GameObject("Equip", typeof(RectTransform), typeof(Image), typeof(Button));
            equipGO.transform.SetParent(go.transform, false);
            var eRect = equipGO.GetComponent<RectTransform>();
            eRect.anchorMin = new Vector2(1, 0); eRect.anchorMax = new Vector2(1, 1);
            eRect.pivot = new Vector2(1, 0.5f);
            eRect.sizeDelta = new Vector2(55, 0);
            eRect.anchoredPosition = Vector2.zero;
            equipGO.GetComponent<Image>().color = new Color(0.2f, 0.35f, 0.2f);
            var eTextGO = new GameObject("T", typeof(RectTransform), typeof(Text));
            eTextGO.transform.SetParent(equipGO.transform, false);
            var etRect = eTextGO.GetComponent<RectTransform>();
            etRect.anchorMin = Vector2.zero; etRect.anchorMax = Vector2.one; etRect.sizeDelta = Vector2.zero;
            var et = eTextGO.GetComponent<Text>();
            et.text = "EQUIP"; et.fontSize = 11; et.color = Color.white;
            et.alignment = TextAnchor.MiddleCenter; et.font = _font; et.raycastTarget = false;

            var capturedItem = item;
            equipGO.GetComponent<Button>().onClick.AddListener(() => {
                InventoryManager.Equip(capturedItem);
                Refresh();
            });
        }

        private void CreateText(Transform parent, string text, Color color)
        {
            var go = new GameObject("Info", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280, 30);
            var t = go.GetComponent<Text>();
            t.text = text; t.fontSize = 12; t.color = color;
            t.alignment = TextAnchor.MiddleCenter; t.font = _font; t.raycastTarget = false;
        }
    }
}
