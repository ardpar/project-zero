using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Persistence;
using Synthborn.Core.Items;

namespace Synthborn.UI
{
    /// <summary>
    /// Synthesis Lab: buy consumables, reroll loot, buy materials.
    /// Accessible from Arena Map.
    /// </summary>
    public class ShopScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _goldText;
        [SerializeField] private Transform _shopContainer;
        [SerializeField] private Text _resultText;
        [SerializeField] private Font _font;

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
            var ch = SaveManager.Character;
            if (ch == null) return;

            if (_goldText != null)
                _goldText.text = $"Fragment: {ch.gold}";
            if (_resultText != null)
                _resultText.text = "";

            if (_shopContainer == null) return;
            foreach (Transform child in _shopContainer) Destroy(child.gameObject);

            // Shop items — Arena terminology
            CreateShopItem("Residual Compound x5", "Temel sentez malzemesi", 50, () => {
                ch.scrapMetal += 5;
            });

            CreateShopItem("Mutation Residue x2", "Nadir sentez malzemesi", 150, () => {
                ch.darkCrystals += 2;
            });

            CreateShopItem("Rejenerasyon Ampul\u00fc", "Deneme ba\u015flang\u0131c\u0131nda %50 HP", 100, () => {
                PlayerPrefs.SetInt("ShopHealPotion", 1);
                PlayerPrefs.Save();
            });

            CreateShopItem("XP Kataliz\u00f6r", "Sonraki deneme i\u00e7in +%50 XP", 200, () => {
                PlayerPrefs.SetInt("ShopXPScroll", 1);
                PlayerPrefs.Save();
            });

            CreateShopItem("Olas\u0131l\u0131k Mod\u00fcl\u00fc", "Sonraki deneme i\u00e7in +%25 drop", 250, () => {
                PlayerPrefs.SetInt("ShopLuckyCoin", 1);
                PlayerPrefs.Save();
            });
        }

        private void CreateShopItem(string name, string desc, int cost, System.Action onBuy)
        {
            var ch = SaveManager.Character;
            bool canBuy = ch != null && ch.gold >= cost;

            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_shopContainer, false);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 55);
            go.GetComponent<Image>().color = canBuy
                ? new Color(0.18f, 0.16f, 0.25f)
                : new Color(0.10f, 0.10f, 0.12f);

            var textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(go.transform, false);
            var tRect = textGO.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero; tRect.anchorMax = new Vector2(0.65f, 1f);
            tRect.sizeDelta = Vector2.zero; tRect.offsetMin = new Vector2(10, 2); tRect.offsetMax = new Vector2(-5, -2);
            var text = textGO.GetComponent<Text>();
            text.text = $"<b>{name}</b>  <color=yellow>{cost}f</color>\n<size=10>{desc}</size>";
            text.fontSize = 13; text.color = canBuy ? Color.white : Color.gray;
            text.alignment = TextAnchor.MiddleLeft; text.font = _font;
            text.supportRichText = true; text.raycastTarget = false;

            var btnGO = new GameObject("BuyBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(go.transform, false);
            var bRect = btnGO.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.68f, 0.15f); bRect.anchorMax = new Vector2(0.98f, 0.85f);
            bRect.sizeDelta = Vector2.zero; bRect.offsetMin = Vector2.zero; bRect.offsetMax = Vector2.zero;
            btnGO.GetComponent<Image>().color = canBuy
                ? new Color(0.2f, 0.35f, 0.2f) : new Color(0.12f, 0.12f, 0.12f);
            btnGO.GetComponent<Button>().interactable = canBuy;

            var btGO = new GameObject("T", typeof(RectTransform), typeof(Text));
            btGO.transform.SetParent(btnGO.transform, false);
            var btRect = btGO.GetComponent<RectTransform>();
            btRect.anchorMin = Vector2.zero; btRect.anchorMax = Vector2.one; btRect.sizeDelta = Vector2.zero;
            var bt = btGO.GetComponent<Text>();
            bt.text = "SENTEZLE"; bt.fontSize = 14; bt.color = canBuy ? Color.white : Color.gray;
            bt.alignment = TextAnchor.MiddleCenter; bt.font = _font; bt.raycastTarget = false;

            if (canBuy)
            {
                int capturedCost = cost;
                btnGO.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (ch.gold >= capturedCost)
                    {
                        ch.gold -= capturedCost;
                        onBuy();
                        SaveManager.SaveSlot();
                        if (_resultText != null) _resultText.text = $"<color=green>{name} sentezlendi!</color>";
                        Refresh();
                    }
                });
            }
        }
    }
}
