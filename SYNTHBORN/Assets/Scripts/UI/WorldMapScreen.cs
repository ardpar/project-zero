using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Synthborn.Core.Persistence;

namespace Synthborn.UI
{
    /// <summary>
    /// World Map hub: level grid, character info, navigation buttons.
    /// Attach to the main Canvas in WorldMap scene.
    /// </summary>
    public class WorldMapScreen : MonoBehaviour
    {
        [Header("Character Info")]
        [SerializeField] private Text _charInfoText;

        [Header("Level Grid")]
        [SerializeField] private Transform _levelGridContainer;
        [SerializeField] private int _totalLevels = 100;

        [Header("Navigation")]
        [SerializeField] private Button _saveQuitButton;

        [SerializeField] private Font _font;

        private void Start()
        {
            // Auto-save when entering WorldMap (progress from last level/death)
            if (SaveManager.Character != null)
            {
                SaveManager.SaveSlot();
                SaveManager.Save();
            }

            RefreshCharacterInfo();
            RefreshLevelGrid();

            if (_saveQuitButton != null)
                _saveQuitButton.onClick.AddListener(OnSaveQuit);
        }

        private void OnDestroy()
        {
            if (_saveQuitButton != null)
                _saveQuitButton.onClick.RemoveListener(OnSaveQuit);
        }

        private void RefreshCharacterInfo()
        {
            var ch = SaveManager.Character;
            if (ch == null || _charInfoText == null) return;

            string className = ch.classType switch
            {
                0 => "Warrior", 1 => "Rogue", 2 => "Mage", 3 => "Sentinel", _ => "?"
            };

            _charInfoText.text = $"{ch.characterName}  |  {className}  |  Lv.{ch.characterLevel}  |  Gold: {ch.gold}";
        }

        private void RefreshLevelGrid()
        {
            if (_levelGridContainer == null) return;
            foreach (Transform child in _levelGridContainer) Destroy(child.gameObject);

            var ch = SaveManager.Character;
            if (ch == null) return;

            for (int i = 1; i <= _totalLevels; i++)
                CreateLevelButton(i, ch);
        }

        private void CreateLevelButton(int levelNumber, CharacterSaveData ch)
        {
            bool unlocked = ch.IsLevelUnlocked(levelNumber);
            bool completed = ch.IsLevelCompleted(levelNumber);

            var btnGO = new GameObject($"Level_{levelNumber}", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(_levelGridContainer, false);
            var rect = btnGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(70, 70);

            var img = btnGO.GetComponent<Image>();
            if (completed)
                img.color = new Color(0.15f, 0.35f, 0.15f); // green
            else if (unlocked)
                img.color = new Color(0.25f, 0.20f, 0.35f); // purple (playable)
            else
                img.color = new Color(0.1f, 0.1f, 0.1f); // dark (locked)

            var btn = btnGO.GetComponent<Button>();
            btn.interactable = unlocked;

            // Level number text
            var textGO = new GameObject("Num", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(btnGO.transform, false);
            var tRect = textGO.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;
            var text = textGO.GetComponent<Text>();
            // S12-12: Difficulty stars based on level number
            int stars = Mathf.Clamp((levelNumber - 1) / 20 + 1, 1, 5);
            string starStr = new string('\u2605', stars);
            string checkmark = completed ? "\n<size=10>\u2713</size>" : "";
            text.text = $"{levelNumber}\n<size=8>{starStr}</size>{checkmark}";
            text.fontSize = 16;
            text.color = unlocked ? Color.white : Color.gray;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = _font;
            text.supportRichText = true;
            text.raycastTarget = false;

            if (unlocked)
            {
                int captured = levelNumber;
                btn.onClick.AddListener(() => OnLevelSelected(captured));
            }
        }

        private void OnLevelSelected(int levelNumber)
        {
            // Store selected level for gameplay scene to read
            PlayerPrefs.SetInt("SelectedLevel", levelNumber);
            PlayerPrefs.Save();
            SceneManager.LoadScene("SampleScene");
        }

        private void OnSaveQuit()
        {
            SaveManager.SaveSlot();
            SaveManager.Save();
            SceneManager.LoadScene("MainMenu");
        }
    }
}
