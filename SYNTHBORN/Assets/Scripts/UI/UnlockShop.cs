using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Persistence;
using Synthborn.Mutations;

namespace Synthborn.UI
{
    /// <summary>
    /// Main Menu unlock shop. Lists all mutations with lock/unlock state.
    /// Player spends Cells to unlock new mutations for future runs.
    /// </summary>
    public class UnlockShop : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _itemContainer;
        [SerializeField] private Text _cellsText;
        [SerializeField] private MutationDatabase _database;
        [SerializeField] private GameObject _itemPrefab;

        public void Show()
        {
            if (_panel == null) return;
            _panel.SetActive(true);
            RefreshUI();
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void RefreshUI()
        {
            if (_cellsText != null)
                _cellsText.text = $"Cells: {UnlockManager.Cells}";

            // Clear existing items
            if (_itemContainer != null)
            {
                foreach (Transform child in _itemContainer)
                    Destroy(child.gameObject);
            }

            if (_database == null || _database.allMutations == null) return;

            foreach (var mutation in _database.allMutations)
            {
                if (mutation == null) continue;
                CreateShopItem(mutation);
            }
        }

        private void CreateShopItem(MutationData mutation)
        {
            var go = new GameObject(mutation.id, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_itemContainer, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(280, 40);

            bool unlocked = UnlockManager.IsUnlocked(mutation.id);
            int cost = UnlockManager.GetUnlockCost((int)mutation.rarity);

            var img = go.GetComponent<Image>();
            img.color = unlocked ? new Color(0.15f, 0.3f, 0.15f) : new Color(0.3f, 0.15f, 0.15f);

            // Name + cost text
            var textGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(go.transform, false);
            var t = textGO.GetComponent<Text>();
            t.text = unlocked ? $"✓ {mutation.displayName}" : $"{mutation.displayName} — {cost} Cells";
            t.fontSize = 14;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleLeft;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var tRT = textGO.GetComponent<RectTransform>();
            tRT.anchorMin = Vector2.zero;
            tRT.anchorMax = Vector2.one;
            tRT.sizeDelta = new Vector2(-10, 0);
            tRT.anchoredPosition = new Vector2(5, 0);

            if (!unlocked)
            {
                var btn = go.GetComponent<Button>();
                string id = mutation.id;
                int c = cost;
                btn.onClick.AddListener(() =>
                {
                    if (UnlockManager.TryUnlock(id, c))
                        RefreshUI();
                });
            }
        }
    }
}
