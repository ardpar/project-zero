using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Persistence;

namespace Synthborn.UI
{
    /// <summary>
    /// Displays 5 stat upgrades with tier stars, cost, and buy button.
    /// </summary>
    public class UpgradeShop : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _listContainer;
        [SerializeField] private Text _cellsText;
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
            if (_listContainer != null)
                foreach (Transform child in _listContainer) Destroy(child.gameObject);

            if (_cellsText != null)
                _cellsText.text = $"Cells: {UnlockManager.Cells}";

            for (int i = 0; i < UpgradeManager.UpgradeCount; i++)
            {
                var type = (UpgradeManager.UpgradeType)i;
                CreateRow(type);
            }
        }

        private void CreateRow(UpgradeManager.UpgradeType type)
        {
            int level = UpgradeManager.GetLevel(type);
            int cost = UpgradeManager.GetNextCost(type);
            string stars = new string('\u2605', level) + new string('\u2606', UpgradeManager.MaxLevel - level);
            string costStr = cost < 0 ? "MAX" : $"{cost} Cells";
            bool canBuy = cost >= 0 && UnlockManager.Cells >= cost;

            // Row container
            var rowGO = new GameObject($"Upgrade_{type}", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            rowGO.transform.SetParent(_listContainer, false);
            var rowRect = rowGO.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(600, 40);
            var hlg = rowGO.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 15;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;

            // Name
            CreateText(rowGO.transform, UpgradeManager.Names[(int)type], 16, Color.white, 160);

            // Stars
            CreateText(rowGO.transform, stars, 18, new Color(1f, 0.85f, 0.3f), 100);

            // Cost
            Color costColor = cost < 0 ? Color.green : (canBuy ? Color.white : Color.gray);
            CreateText(rowGO.transform, costStr, 14, costColor, 100);

            // Buy button
            if (cost >= 0)
            {
                var btnGO = new GameObject("BuyBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                btnGO.transform.SetParent(rowGO.transform, false);
                var btnRect = btnGO.GetComponent<RectTransform>();
                btnRect.sizeDelta = new Vector2(80, 35);

                var img = btnGO.GetComponent<Image>();
                img.color = canBuy ? new Color(0.25f, 0.20f, 0.35f) : new Color(0.15f, 0.15f, 0.15f);

                // Find and apply button sprite
                var allBtns = Resources.FindObjectsOfTypeAll<Button>();
                foreach (var b in allBtns)
                {
                    var bImg = b.GetComponent<Image>();
                    if (bImg != null && bImg.sprite != null && bImg.type == Image.Type.Sliced)
                    {
                        img.sprite = bImg.sprite;
                        img.type = Image.Type.Sliced;
                        break;
                    }
                }

                var btnTextGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
                btnTextGO.transform.SetParent(btnGO.transform, false);
                var btnTextRect = btnTextGO.GetComponent<RectTransform>();
                btnTextRect.anchorMin = Vector2.zero;
                btnTextRect.anchorMax = Vector2.one;
                btnTextRect.sizeDelta = Vector2.zero;
                var btnText = btnTextGO.GetComponent<Text>();
                btnText.text = "BUY";
                btnText.fontSize = 14;
                btnText.color = canBuy ? Color.white : Color.gray;
                btnText.alignment = TextAnchor.MiddleCenter;
                btnText.font = _font;
                btnText.raycastTarget = false;

                var btn = btnGO.GetComponent<Button>();
                btn.interactable = canBuy;
                var captured = type;
                btn.onClick.AddListener(() => { OnBuy(captured); });
            }
        }

        private void OnBuy(UpgradeManager.UpgradeType type)
        {
            if (UpgradeManager.TryUpgrade(type))
            {
                StartCoroutine(BuyFeedback());
                Refresh();
            }
        }

        private IEnumerator BuyFeedback()
        {
            // Quick scale punch on the list container for tactile feel
            if (_listContainer == null) yield break;
            var t = _listContainer;
            var original = t.localScale;
            float elapsed = 0f;
            float dur = 0.15f;

            // Scale up
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float s = 1f + 0.05f * Mathf.Sin(Mathf.PI * elapsed / dur);
                t.localScale = original * s;
                yield return null;
            }
            t.localScale = original;
        }

        private void CreateText(Transform parent, string text, int fontSize, Color color, float width)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, 35);
            var t = go.GetComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = TextAnchor.MiddleLeft;
            t.font = _font;
            t.raycastTarget = false;
        }
    }
}
