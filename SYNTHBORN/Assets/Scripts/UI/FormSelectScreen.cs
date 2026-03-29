using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Synthborn.Core;
using Synthborn.Core.Persistence;

namespace Synthborn.UI
{
    /// <summary>
    /// Form selection screen shown before run start.
    /// Displays 3 starter form cards with stats and select button.
    /// </summary>
    public class FormSelectScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private StarterFormData[] _forms;
        [SerializeField] private Transform _cardContainer;
        [SerializeField] private Font _font;

        private int _selectedIndex;

        public void Show()
        {
            if (_panel == null) return;
            _selectedIndex = SaveManager.Data.selectedStarterForm;
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
            if (_cardContainer != null)
                foreach (Transform child in _cardContainer) Destroy(child.gameObject);

            if (_forms == null) return;

            for (int i = 0; i < _forms.Length; i++)
            {
                if (_forms[i] == null) continue;
                CreateCard(_forms[i], i);
            }
        }

        private void CreateCard(StarterFormData form, int index)
        {
            bool unlocked = form.IsUnlocked();
            bool selected = index == _selectedIndex;

            var cardGO = new GameObject(form.FormName, typeof(RectTransform), typeof(Image));
            cardGO.transform.SetParent(_cardContainer, false);
            var rect = cardGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 220);
            var img = cardGO.GetComponent<Image>();
            img.color = selected
                ? new Color(0.3f, 0.25f, 0.4f)
                : (unlocked ? new Color(0.18f, 0.15f, 0.22f) : new Color(0.1f, 0.1f, 0.1f));

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

            // Title
            CreateText(cardGO.transform, form.FormName.ToUpper(), 18, new Color(1f, 0.85f, 0.3f), 180, 45);

            // Stats
            string stats = "";
            if (form.HpModifier != 0) stats += $"HP: {form.HpModifier:+0%;-0%}\n";
            if (form.SpeedModifier != 0) stats += $"Speed: {form.SpeedModifier:+0%;-0%}\n";
            if (form.DamageModifier != 0) stats += $"DMG: {form.DamageModifier:+0%;-0%}\n";
            if (form.CritChance != 0) stats += $"Crit: {form.CritChance:+0%;-0%}\n";
            if (form.ArmorFlat != 0) stats += $"Armor: +{form.ArmorFlat}\n";
            if (string.IsNullOrEmpty(stats)) stats = "Standard\nstats";
            CreateText(cardGO.transform, stats, 12, Color.white, 180, 90);

            // Select button
            if (unlocked)
            {
                var btnGO = new GameObject("SelectBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                btnGO.transform.SetParent(cardGO.transform, false);
                var btnRect = btnGO.GetComponent<RectTransform>();
                btnRect.sizeDelta = new Vector2(120, 35);
                var btnImg = btnGO.GetComponent<Image>();
                btnImg.color = selected ? new Color(0.2f, 0.5f, 0.2f) : new Color(0.25f, 0.2f, 0.35f);

                var btnTextGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
                btnTextGO.transform.SetParent(btnGO.transform, false);
                var btRect = btnTextGO.GetComponent<RectTransform>();
                btRect.anchorMin = Vector2.zero;
                btRect.anchorMax = Vector2.one;
                btRect.sizeDelta = Vector2.zero;
                var btnText = btnTextGO.GetComponent<Text>();
                btnText.text = selected ? "SELECTED" : "SELECT";
                btnText.fontSize = 14;
                btnText.color = Color.white;
                btnText.alignment = TextAnchor.MiddleCenter;
                btnText.font = _font;
                btnText.raycastTarget = false;

                int captured = index;
                btnGO.GetComponent<Button>().onClick.AddListener(() => SelectForm(captured));
            }
            else
            {
                CreateText(cardGO.transform, "LOCKED", 14, Color.red, 180, 35);
            }
        }

        private void SelectForm(int index)
        {
            _selectedIndex = index;
            SaveManager.Data.selectedStarterForm = index;
            SaveManager.Save();
            Refresh();
        }

        private void CreateText(Transform parent, string text, int fontSize, Color color, float width, float height)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, height);
            var t = go.GetComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.font = _font;
            t.raycastTarget = false;
        }
    }
}
