using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Synthborn.Core;
using Synthborn.Core.Persistence;

namespace Synthborn.UI
{
    /// <summary>
    /// Character creation: name input + class selection + create button.
    /// </summary>
    public class CharacterCreationScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private InputField _nameInput;
        [SerializeField] private Transform _classCardContainer;
        [SerializeField] private Button _createButton;
        [SerializeField] private Text _errorText;
        [SerializeField] private ClassData[] _classes;
        [SerializeField] private Font _font;

        private int _selectedClass;
        private int _targetSlot;

        public void Show(int saveSlot)
        {
            _targetSlot = saveSlot;
            _selectedClass = 0;
            if (_panel != null) _panel.SetActive(true);
            if (_nameInput != null) _nameInput.text = "";
            if (_errorText != null) _errorText.text = "";
            RefreshClassCards();
            PopupEscHandler.Register(_panel, Hide);
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            PopupEscHandler.Unregister();
        }

        private void Awake()
        {
            if (_createButton != null)
                _createButton.onClick.AddListener(OnCreate);
        }

        private void OnDestroy()
        {
            if (_createButton != null)
                _createButton.onClick.RemoveListener(OnCreate);
        }

        private void OnCreate()
        {
            string charName = _nameInput != null ? _nameInput.text.Trim() : "";
            if (string.IsNullOrEmpty(charName))
            {
                if (_errorText != null) _errorText.text = "Enter a name!";
                return;
            }

            SaveManager.CreateCharacter(_targetSlot, charName, _selectedClass);
            Hide();
            SceneManager.LoadScene("WorldMap");
        }

        private void RefreshClassCards()
        {
            if (_classCardContainer == null || _classes == null) return;
            foreach (Transform child in _classCardContainer) Destroy(child.gameObject);

            for (int i = 0; i < _classes.Length; i++)
            {
                if (_classes[i] == null) continue;
                CreateClassCard(_classes[i], i);
            }
        }

        private void CreateClassCard(ClassData classData, int index)
        {
            bool selected = index == _selectedClass;

            var cardGO = new GameObject(classData.ClassName, typeof(RectTransform), typeof(Image), typeof(Button));
            cardGO.transform.SetParent(_classCardContainer, false);
            var rect = cardGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160, 180);
            var img = cardGO.GetComponent<Image>();
            img.color = selected ? new Color(0.3f, 0.25f, 0.4f) : new Color(0.18f, 0.15f, 0.22f);

            // Name
            var nameGO = new GameObject("Name", typeof(RectTransform), typeof(Text));
            nameGO.transform.SetParent(cardGO.transform, false);
            var nRect = nameGO.GetComponent<RectTransform>();
            nRect.anchorMin = new Vector2(0, 0.75f);
            nRect.anchorMax = new Vector2(1, 1);
            nRect.sizeDelta = Vector2.zero;
            nRect.offsetMin = Vector2.zero;
            nRect.offsetMax = Vector2.zero;
            var nText = nameGO.GetComponent<Text>();
            nText.text = classData.ClassName.ToUpper();
            nText.fontSize = 16;
            nText.color = classData.ClassColor;
            nText.alignment = TextAnchor.MiddleCenter;
            nText.font = _font;
            nText.raycastTarget = false;

            // Description
            string desc = !string.IsNullOrEmpty(classData.Description) ? classData.Description : "";
            var descGO = new GameObject("Desc", typeof(RectTransform), typeof(Text));
            descGO.transform.SetParent(cardGO.transform, false);
            var dRect = descGO.GetComponent<RectTransform>();
            dRect.anchorMin = new Vector2(0, 0.55f);
            dRect.anchorMax = new Vector2(1, 0.75f);
            dRect.sizeDelta = Vector2.zero; dRect.offsetMin = Vector2.zero; dRect.offsetMax = Vector2.zero;
            var dText = descGO.GetComponent<Text>();
            dText.text = desc; dText.fontSize = 9; dText.color = new Color(0.7f, 0.7f, 0.7f);
            dText.alignment = TextAnchor.MiddleCenter; dText.font = _font; dText.raycastTarget = false;

            // Stats
            string stats = "";
            if (classData.HpModifier != 0) stats += $"HP: {classData.HpModifier:+0%;-0%}\n";
            if (classData.DamageModifier != 0) stats += $"DMG: {classData.DamageModifier:+0%;-0%}\n";
            if (classData.SpeedModifier != 0) stats += $"SPD: {classData.SpeedModifier:+0%;-0%}\n";
            if (classData.CritChance != 0) stats += $"CRIT: {classData.CritChance:+0%;-0%}\n";
            if (classData.ArmorFlat != 0) stats += $"ARM: {classData.ArmorFlat:+0;-0}\n";
            if (string.IsNullOrEmpty(stats)) stats = "Balanced";

            var statsGO = new GameObject("Stats", typeof(RectTransform), typeof(Text));
            statsGO.transform.SetParent(cardGO.transform, false);
            var sRect = statsGO.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0, 0.15f);
            sRect.anchorMax = new Vector2(1, 0.55f);
            sRect.sizeDelta = Vector2.zero;
            sRect.offsetMin = Vector2.zero;
            sRect.offsetMax = Vector2.zero;
            var sText = statsGO.GetComponent<Text>();
            sText.text = stats;
            sText.fontSize = 12;
            sText.color = Color.white;
            sText.alignment = TextAnchor.MiddleCenter;
            sText.font = _font;
            sText.raycastTarget = false;

            // Select indicator
            if (selected)
            {
                var selGO = new GameObject("Selected", typeof(RectTransform), typeof(Text));
                selGO.transform.SetParent(cardGO.transform, false);
                var selRect = selGO.GetComponent<RectTransform>();
                selRect.anchorMin = new Vector2(0, 0);
                selRect.anchorMax = new Vector2(1, 0.15f);
                selRect.sizeDelta = Vector2.zero;
                selRect.offsetMin = Vector2.zero;
                selRect.offsetMax = Vector2.zero;
                var selText = selGO.GetComponent<Text>();
                selText.text = "SELECTED";
                selText.fontSize = 10;
                selText.color = Color.green;
                selText.alignment = TextAnchor.MiddleCenter;
                selText.font = _font;
                selText.raycastTarget = false;
            }

            // S12-11: Hover animation on class cards
            if (cardGO.GetComponent<ButtonHoverEffect>() == null)
                cardGO.AddComponent<ButtonHoverEffect>();

            int captured = index;
            cardGO.GetComponent<Button>().onClick.AddListener(() => {
                _selectedClass = captured;
                RefreshClassCards();
            });
        }
    }
}
