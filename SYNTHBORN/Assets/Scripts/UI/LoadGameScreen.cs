using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Synthborn.Core.Persistence;

namespace Synthborn.UI
{
    /// <summary>
    /// Shows 3 save slots in two modes:
    /// - New Game: empty slots are selectable (→ character creation), filled slots show "Overwrite?"
    /// - Load Game: only filled slots are selectable (→ WorldMap), empty slots are grayed out
    /// </summary>
    public class LoadGameScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private Text _titleText;
        [SerializeField] private CharacterCreationScreen _creationScreen;
        [SerializeField] private Font _font;

        private bool _isNewGame;

        public void Show(bool isNewGame)
        {
            if (_panel == null) return;
            _isNewGame = isNewGame;
            _panel.SetActive(true);

            if (_titleText != null)
                _titleText.text = isNewGame ? "SELECT SLOT" : "LOAD GAME";

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

            // Determine if this slot is interactable
            bool interactable;
            if (_isNewGame)
                interactable = true; // All slots available for new game (empty → create, full → overwrite)
            else
                interactable = hasData; // Load mode: only filled slots

            var slotGO = new GameObject($"Slot{slotIndex}", typeof(RectTransform), typeof(Image), typeof(Button));
            slotGO.transform.SetParent(_slotContainer, false);
            var rect = slotGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(500, 80);
            var img = slotGO.GetComponent<Image>();

            if (!interactable)
                img.color = new Color(0.08f, 0.08f, 0.08f); // dark, disabled
            else if (hasData)
                img.color = new Color(0.2f, 0.18f, 0.25f); // filled
            else
                img.color = new Color(0.15f, 0.20f, 0.15f); // empty, available for new

            var btn = slotGO.GetComponent<Button>();
            btn.interactable = interactable;

            // Label
            string label;
            if (hasData)
            {
                string className = GetClassName(data.classType);
                label = $"Slot {slotIndex + 1}: {data.characterName}\n" +
                        $"Lv.{data.characterLevel}  |  {className}  |  Level {data.highestLevelUnlocked - 1} cleared  |  {data.lastPlayedDate}";
            }
            else
            {
                label = _isNewGame
                    ? $"Slot {slotIndex + 1}: Empty — Create New Character"
                    : $"Slot {slotIndex + 1}: Empty";
            }

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
            text.color = interactable ? Color.white : new Color(0.3f, 0.3f, 0.3f);
            text.alignment = TextAnchor.MiddleLeft;
            text.font = _font;
            text.raycastTarget = false;

            int captured = slotIndex;
            bool capturedHasData = hasData;
            btn.onClick.AddListener(() => OnSlotClicked(captured, capturedHasData));
        }

        private void OnSlotClicked(int slot, bool hasData)
        {
            if (_isNewGame)
            {
                // New Game mode: empty → create, filled → overwrite (delete + create)
                if (hasData)
                    SaveManager.DeleteSlot(slot);
                Hide();
                _creationScreen?.Show(slot);
            }
            else
            {
                // Load mode: load existing save → WorldMap
                if (!hasData) return;
                SaveManager.LoadSlot(slot);
                Hide();
                SceneManager.LoadScene("WorldMap");
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
