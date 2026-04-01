using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Synthborn.Core.Persistence;

namespace Synthborn.Lore
{
    /// <summary>
    /// Signal Archive screen — displays collected lore fragments.
    /// Grouped by category, each fragment readable on selection.
    /// Accessible from Arena Map or Calibration Interval.
    /// </summary>
    public class SignalArchiveScreen : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LoreDatabase _database;
        [SerializeField] private Transform _listContainer;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _contentText;
        [SerializeField] private TMP_Text _categoryText;
        [SerializeField] private TMP_Text _counterText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _panel;

        private void OnEnable()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Hide);
        }

        private void OnDisable()
        {
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(Hide);
        }

        /// <summary>Show the archive and populate the list.</summary>
        public void Show()
        {
            if (_panel != null) _panel.SetActive(true);
            PopulateList();
            ClearDetail();
        }

        /// <summary>Hide the archive.</summary>
        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void PopulateList()
        {
            if (_listContainer == null || _database == null) return;

            // Clear existing entries
            foreach (Transform child in _listContainer)
                Destroy(child.gameObject);

            var ch = SaveManager.Character;
            if (ch == null) return;

            int discovered = 0;
            int total = _database.fragments?.Length ?? 0;

            // Group by category
            var categories = System.Enum.GetValues(typeof(LoreCategory));
            foreach (LoreCategory cat in categories)
            {
                var fragments = _database.GetByCategory(cat);
                if (fragments.Length == 0) continue;

                // Category header
                CreateListItem(cat.ToString(), true, null);

                foreach (var f in fragments)
                {
                    bool isDiscovered = ch.signalArchiveEntries.Contains(f.fragmentId);
                    if (isDiscovered) discovered++;

                    string label = isDiscovered ? f.title : "???";
                    CreateListItem(label, false, isDiscovered ? f : null);
                }
            }

            if (_counterText != null)
                _counterText.text = $"Keşfedilen: {discovered}/{total}";
        }

        private void CreateListItem(string label, bool isHeader, LoreFragment fragment)
        {
            var go = new GameObject(label, typeof(RectTransform));
            go.transform.SetParent(_listContainer, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, isHeader ? 30 : 24);

            if (isHeader)
            {
                var headerGO = new GameObject("Header", typeof(RectTransform), typeof(TextMeshProUGUI));
                headerGO.transform.SetParent(go.transform, false);
                var hrt = headerGO.GetComponent<RectTransform>();
                hrt.anchorMin = Vector2.zero; hrt.anchorMax = Vector2.one;
                hrt.sizeDelta = Vector2.zero;
                var txt = headerGO.GetComponent<TextMeshProUGUI>();
                txt.text = $"<b>— {label} —</b>";
                txt.fontSize = 12;
                txt.color = new Color(0.6f, 0.8f, 1f);
                txt.alignment = TextAlignmentOptions.MidlineLeft;
                txt.raycastTarget = false;
            }
            else
            {
                var btnGO = new GameObject("Entry", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(Button));
                btnGO.transform.SetParent(go.transform, false);
                var brt = btnGO.GetComponent<RectTransform>();
                brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
                brt.sizeDelta = Vector2.zero;
                var txt = btnGO.GetComponent<TextMeshProUGUI>();
                txt.text = fragment != null ? $"  {label}" : "  <color=#666>???</color>";
                txt.fontSize = 11;
                txt.color = fragment != null ? Color.white : Color.gray;
                txt.alignment = TextAlignmentOptions.MidlineLeft;

                if (fragment != null)
                {
                    var captured = fragment;
                    btnGO.GetComponent<Button>().onClick.AddListener(() => ShowDetail(captured));
                }
                else
                {
                    btnGO.GetComponent<Button>().interactable = false;
                }
            }
        }

        private void ShowDetail(LoreFragment fragment)
        {
            if (_titleText != null) _titleText.text = fragment.title;
            if (_contentText != null) _contentText.text = fragment.content;
            if (_categoryText != null) _categoryText.text = fragment.category.ToString();
        }

        private void ClearDetail()
        {
            if (_titleText != null) _titleText.text = "Bir sinyal seç...";
            if (_contentText != null) _contentText.text = "";
            if (_categoryText != null) _categoryText.text = "";
        }
    }
}
