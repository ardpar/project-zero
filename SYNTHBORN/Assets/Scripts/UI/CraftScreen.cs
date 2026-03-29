using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Persistence;
using Synthborn.Core.Items;

namespace Synthborn.UI
{
    /// <summary>
    /// Synthesis Lab: shows materials, 3 synthesis recipes, synthesis buttons.
    /// </summary>
    public class CraftScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _materialsText;
        [SerializeField] private Transform _recipeContainer;
        [SerializeField] private Text _resultText;
        [SerializeField] private Font _font;

        private ItemDatabase _db;

        public void Show()
        {
            if (_panel == null) return;
            _db = Resources.Load<ItemDatabase>("ItemDatabase");
            if (_db != null) InventoryManager.SetDatabase(_db);
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

            if (_materialsText != null)
                _materialsText.text = $"Residual Compound: {ch.scrapMetal}  |  Mutation Residue: {ch.darkCrystals}  |  Stabilized Core: {ch.bossEssences}";

            if (_resultText != null)
                _resultText.text = "";

            if (_recipeContainer == null) return;
            foreach (Transform child in _recipeContainer) Destroy(child.gameObject);

            // Recipe 1: 3 Compound → Common Component
            CreateRecipe("Temel Sentez", "3x Residual Compound \u2192 Rastgele Common Komponent",
                ch.scrapMetal >= 3, () => {
                    if (CraftingManager.CraftCommon(_db))
                    {
                        if (_resultText != null) _resultText.text = "<color=white>Common komponent sentezlendi!</color>";
                        Refresh();
                    }
                });

            // Recipe 2: 2 Residue + 1 Compound → Rare Component
            CreateRecipe("Nadir Sentez", "2x Mutation Residue + 1x Compound \u2192 Rastgele Rare Komponent",
                ch.darkCrystals >= 2 && ch.scrapMetal >= 1, () => {
                    if (CraftingManager.CraftRare(_db))
                    {
                        if (_resultText != null) _resultText.text = "<color=#3355FF>Rare komponent sentezlendi!</color>";
                        Refresh();
                    }
                });

            // Recipe 3: 1 Core + 3 Residue → Legendary Component
            CreateRecipe("Efsanevi Sentez", "1x Stabilized Core + 3x Mutation Residue \u2192 Legendary Komponent",
                ch.bossEssences >= 1 && ch.darkCrystals >= 3, () => {
                    if (CraftingManager.CraftLegendary(_db))
                    {
                        if (_resultText != null) _resultText.text = "<color=#FFD700>Legendary komponent sentezlendi!</color>";
                        Refresh();
                    }
                });
        }

        private void CreateRecipe(string name, string desc, bool canCraft, System.Action onCraft)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_recipeContainer, false);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 65);
            go.GetComponent<Image>().color = canCraft
                ? new Color(0.18f, 0.16f, 0.25f)
                : new Color(0.10f, 0.10f, 0.12f);

            var textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(go.transform, false);
            var tRect = textGO.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero; tRect.anchorMax = new Vector2(0.7f, 1f);
            tRect.sizeDelta = Vector2.zero; tRect.offsetMin = new Vector2(10, 3); tRect.offsetMax = new Vector2(-5, -3);
            var text = textGO.GetComponent<Text>();
            text.text = $"<b>{name}</b>\n<size=10>{desc}</size>";
            text.fontSize = 14; text.color = canCraft ? Color.white : Color.gray;
            text.alignment = TextAnchor.MiddleLeft; text.font = _font;
            text.supportRichText = true; text.raycastTarget = false;

            var btnGO = new GameObject("CraftBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(go.transform, false);
            var bRect = btnGO.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.72f, 0.15f); bRect.anchorMax = new Vector2(0.98f, 0.85f);
            bRect.sizeDelta = Vector2.zero; bRect.offsetMin = Vector2.zero; bRect.offsetMax = Vector2.zero;
            btnGO.GetComponent<Image>().color = canCraft
                ? new Color(0.2f, 0.4f, 0.2f) : new Color(0.12f, 0.12f, 0.12f);
            btnGO.GetComponent<Button>().interactable = canCraft;

            var btGO = new GameObject("T", typeof(RectTransform), typeof(Text));
            btGO.transform.SetParent(btnGO.transform, false);
            var btRect = btGO.GetComponent<RectTransform>();
            btRect.anchorMin = Vector2.zero; btRect.anchorMax = Vector2.one; btRect.sizeDelta = Vector2.zero;
            var bt = btGO.GetComponent<Text>();
            bt.text = "SENTEZLE"; bt.fontSize = 14; bt.color = canCraft ? Color.white : Color.gray;
            bt.alignment = TextAnchor.MiddleCenter; bt.font = _font; bt.raycastTarget = false;

            if (canCraft) btnGO.GetComponent<Button>().onClick.AddListener(() => onCraft());
        }
    }
}
