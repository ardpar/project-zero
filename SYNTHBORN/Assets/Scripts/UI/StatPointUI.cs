using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Persistence;

namespace Synthborn.UI
{
    /// <summary>
    /// Stat point distribution UI integrated into CharacterScreen.
    /// Shows 5 stats with current value and "+" buttons.
    /// Standalone component — attach to a container in the character panel.
    /// </summary>
    public class StatPointUI : MonoBehaviour
    {
        [SerializeField] private Transform _statContainer;
        [SerializeField] private Text _unspentText;
        [SerializeField] private Font _font;

        private static readonly string[] StatNames = { "STR", "VIT", "AGI", "LCK", "WIS" };
        private static readonly string[] StatDesc = { "Damage", "HP", "Speed", "Crit", "XP Gain" };
        private static readonly Color[] StatColors = {
            new Color(1f, 0.4f, 0.4f),
            new Color(0.4f, 1f, 0.4f),
            new Color(0.4f, 0.4f, 1f),
            new Color(1f, 1f, 0.4f),
            new Color(0.4f, 1f, 1f)
        };

        public void Refresh()
        {
            if (_statContainer == null) return;
            foreach (Transform child in _statContainer) Destroy(child.gameObject);

            var ch = SaveManager.Character;
            if (ch == null) return;

            if (_unspentText != null)
                _unspentText.text = ch.unspentStatPoints > 0
                    ? $"<color=yellow>{ch.unspentStatPoints} points available</color>"
                    : "No points available";

            for (int i = 0; i < 5; i++)
                CreateStatRow(i, ch);
        }

        private void CreateStatRow(int index, CharacterSaveData ch)
        {
            var rowGO = new GameObject($"Stat_{StatNames[index]}", typeof(RectTransform));
            rowGO.transform.SetParent(_statContainer, false);
            rowGO.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 28);

            // Stat name + value
            var textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(rowGO.transform, false);
            var tRect = textGO.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero; tRect.anchorMax = new Vector2(0.7f, 1f);
            tRect.sizeDelta = Vector2.zero; tRect.offsetMin = Vector2.zero; tRect.offsetMax = Vector2.zero;
            var text = textGO.GetComponent<Text>();
            text.text = $"<color=#{ColorUtility.ToHtmlStringRGB(StatColors[index])}>{StatNames[index]}</color>  {ch.statPoints[index]}  <size=10>({StatDesc[index]})</size>";
            text.fontSize = 13;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            text.font = _font;
            text.supportRichText = true;
            text.raycastTarget = false;

            // "+" button
            bool canAdd = ch.unspentStatPoints > 0;
            var btnGO = new GameObject("Add", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(rowGO.transform, false);
            var bRect = btnGO.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.75f, 0.1f); bRect.anchorMax = new Vector2(0.95f, 0.9f);
            bRect.sizeDelta = Vector2.zero; bRect.offsetMin = Vector2.zero; bRect.offsetMax = Vector2.zero;
            btnGO.GetComponent<Image>().color = canAdd
                ? new Color(0.2f, 0.4f, 0.2f)
                : new Color(0.12f, 0.12f, 0.12f);
            btnGO.GetComponent<Button>().interactable = canAdd;

            var plusGO = new GameObject("Plus", typeof(RectTransform), typeof(Text));
            plusGO.transform.SetParent(btnGO.transform, false);
            var pRect = plusGO.GetComponent<RectTransform>();
            pRect.anchorMin = Vector2.zero; pRect.anchorMax = Vector2.one; pRect.sizeDelta = Vector2.zero;
            var plus = plusGO.GetComponent<Text>();
            plus.text = "+"; plus.fontSize = 16; plus.color = canAdd ? Color.white : Color.gray;
            plus.alignment = TextAnchor.MiddleCenter; plus.font = _font; plus.raycastTarget = false;

            if (canAdd)
            {
                int captured = index;
                btnGO.GetComponent<Button>().onClick.AddListener(() => AddPoint(captured));
            }
        }

        private void AddPoint(int statIndex)
        {
            var ch = SaveManager.Character;
            if (ch == null || ch.unspentStatPoints <= 0) return;

            ch.statPoints[statIndex]++;
            ch.unspentStatPoints--;
            SaveManager.SaveSlot();
            Refresh();

            // Also refresh parent CharacterScreen if available
            var charScreen = GetComponentInParent<CharacterScreen>();
            if (charScreen != null && charScreen.gameObject.activeSelf)
                charScreen.Show(); // Full refresh
        }
    }
}
