using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Persistence;
using Synthborn.Mutations;

namespace Synthborn.UI
{
    /// <summary>
    /// Shows all mutations in a grid. Discovered ones show name+stats, undiscovered show "???".
    /// </summary>
    public class CollectionScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _gridContainer;
        [SerializeField] private Text _counterText;
        [SerializeField] private MutationDatabase _database;

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
            if (_gridContainer != null)
                foreach (Transform child in _gridContainer) Destroy(child.gameObject);

            if (_database == null || _database.allMutations == null) return;

            int discovered = 0;
            foreach (var m in _database.allMutations)
            {
                if (m == null) continue;
                bool found = UnlockManager.IsDiscovered(m.id);
                if (found) discovered++;
                CreateEntry(m, found);
            }

            if (_counterText != null)
                _counterText.text = $"Discovered: {discovered}/{_database.allMutations.Length}";
        }

        private void CreateEntry(MutationData mutation, bool discovered)
        {
            var go = new GameObject(mutation.id, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_gridContainer, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120, 60);
            go.GetComponent<Image>().color = discovered
                ? GetRarityColor(mutation.rarity)
                : new Color(0.15f, 0.15f, 0.15f);

            var textGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(go.transform, false);
            var t = textGO.GetComponent<Text>();
            t.text = discovered ? $"{mutation.displayName}\n<size=10>{mutation.rarity}</size>" : "???";
            t.fontSize = 12;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var tRT = textGO.GetComponent<RectTransform>();
            tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one; tRT.sizeDelta = Vector2.zero;
        }

        private static Color GetRarityColor(MutationRarity rarity) => rarity switch
        {
            MutationRarity.Common => new Color(0.3f, 0.3f, 0.3f),
            MutationRarity.Uncommon => new Color(0.15f, 0.35f, 0.15f),
            MutationRarity.Rare => new Color(0.15f, 0.25f, 0.5f),
            MutationRarity.Legendary => new Color(0.5f, 0.4f, 0.1f),
            _ => new Color(0.2f, 0.2f, 0.2f)
        };
    }
}
