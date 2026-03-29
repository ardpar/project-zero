using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Synthborn.Core.Persistence;

namespace Synthborn.UI
{
    /// <summary>
    /// Shows 3 save slots. Each shows character name/class/level or "Empty".
    /// Select to load, or pick empty slot for new game.
    /// </summary>
    public class LoadGameScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private CharacterCreationScreen _creationScreen;
        [SerializeField] private Font _font;

        public void Show()
        {
            if (_panel == null) return;
            _panel.SetActive(true);
            RefreshSlots();
            PopupEscHandler.Register(_panel, Hide);
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            PopupEscHandler.Unregister();
        }

        private void RefreshSlots()
        {
            if (_slotContainer == null) return;
            foreach (Transform child in _slotContainer) Destroy(child.gameObject);

            for (int i = 0; i < 3; i++)
                CreateSlot(i);
        }

        private void CreateSlot(int slotIndex)
        {
            var data = SaveManager.PeekSlot(slotIndex);
            bool hasData = data != null;

            var slotGO = new GameObject($"Slot{slotIndex}", typeof(RectTransform), typeof(Image), typeof(Button));
            slotGO.transform.SetParent(_slotContainer, false);
            var rect = slotGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 80);
            var img = slotGO.GetComponent<Image>();
            img.color = hasData ? new Color(0.2f, 0.18f, 0.25f) : new Color(0.12f, 0.12f, 0.15f);

            string label;
            if (hasData)
                label = $"Slot {slotIndex + 1}: {data.characterName}\nLv.{data.characterLevel} | {GetClassName(data.classType)} | {data.lastPlayedDate}";
            else
                label = $"Slot {slotIndex + 1}: Empty\nNew Game";

            var textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(slotGO.transform, false);
            var tRect = textGO.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;
            tRect.offsetMin = new Vector2(15, 5);
            tRect.offsetMax = new Vector2(-15, -5);
            var text = textGO.GetComponent<Text>();
            text.text = label;
            text.fontSize = 16;
            text.color = hasData ? Color.white : Color.gray;
            text.alignment = TextAnchor.MiddleLeft;
            text.font = _font;
            text.raycastTarget = false;

            int captured = slotIndex;
            bool capturedHasData = hasData;
            slotGO.GetComponent<Button>().onClick.AddListener(() => OnSlotClicked(captured, capturedHasData));
        }

        private void OnSlotClicked(int slot, bool hasData)
        {
            if (hasData)
            {
                SaveManager.LoadSlot(slot);
                Hide();
                SceneManager.LoadScene("WorldMap");
            }
            else
            {
                Hide();
                _creationScreen?.Show(slot);
            }
        }

        private static string GetClassName(int classType) => classType switch
        {
            0 => "Warrior",
            1 => "Rogue",
            2 => "Mage",
            3 => "Sentinel",
            _ => "Unknown"
        };
    }
}
