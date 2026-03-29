using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Persistence;

namespace Synthborn.UI
{
    /// <summary>
    /// Shows all achievements with unlock status.
    /// </summary>
    public class AchievementScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _listContainer;
        [SerializeField] private Text _counterText;
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

            if (_counterText != null)
                _counterText.text = $"{AchievementManager.UnlockedCount}/{AchievementManager.TotalCount}";

            foreach (var ach in AchievementManager.All)
            {
                bool unlocked = AchievementManager.IsUnlocked(ach.Id);
                CreateRow(ach, unlocked);
            }
        }

        private void CreateRow(AchievementDef ach, bool unlocked)
        {
            var go = new GameObject(ach.Id, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_listContainer, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(500, 45);
            var img = go.GetComponent<Image>();
            img.color = unlocked
                ? new Color(0.2f, 0.3f, 0.2f)
                : new Color(0.15f, 0.15f, 0.15f);
            img.raycastTarget = false;

            // Icon
            string icon = unlocked ? "\u2713" : "\u2717";
            Color iconColor = unlocked ? Color.green : Color.red;
            CreateText(go.transform, icon, 20, iconColor, 30, TextAnchor.MiddleCenter);

            // Name + Description
            string label = unlocked
                ? $"{ach.Name}\n<size=10>{ach.Description}</size>"
                : $"???\n<size=10>{ach.Description}</size>";
            CreateText(go.transform, label, 14, unlocked ? Color.white : Color.gray, 450, TextAnchor.MiddleLeft);
        }

        private void CreateText(Transform parent, string text, int fontSize, Color color, float width, TextAnchor align)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, 40);
            var t = go.GetComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = align;
            t.font = _font;
            t.supportRichText = true;
            t.raycastTarget = false;
        }
    }
}
