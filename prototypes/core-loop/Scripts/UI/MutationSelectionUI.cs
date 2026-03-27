// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is the core loop fun?
// Date: 2026-03-27

using UnityEngine;
using UnityEngine.UI;

public class MutationSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button[] _cardButtons = new Button[3]; // Assign in inspector
    [SerializeField] private Text[] _cardNames = new Text[3];
    [SerializeField] private Text[] _cardDescriptions = new Text[3];

    private MutationData[] _currentCards;

    private void OnEnable()
    {
        GameEvents.OnLevelUp += ShowSelection;
    }

    private void OnDisable()
    {
        GameEvents.OnLevelUp -= ShowSelection;
    }

    private void Start()
    {
        _panel.SetActive(false);

        for (int i = 0; i < 3; i++)
        {
            int index = i; // Capture for closure
            _cardButtons[i].onClick.AddListener(() => SelectCard(index));
        }
    }

    private void ShowSelection()
    {
        _currentCards = MutationDatabase.GetThreeCards();
        GameEvents.GamePaused();
        _panel.SetActive(true);

        for (int i = 0; i < 3; i++)
        {
            var card = _currentCards[i];
            _cardNames[i].text = card.Name;
            _cardDescriptions[i].text = card.Description;

            // Rarity color
            var colors = _cardButtons[i].colors;
            colors.normalColor = card.Rarity switch
            {
                MutationData.MutationRarity.Common => Color.white,
                MutationData.MutationRarity.Uncommon => new Color(0.3f, 0.9f, 0.3f),
                MutationData.MutationRarity.Rare => new Color(0.3f, 0.5f, 1f),
                MutationData.MutationRarity.Legendary => new Color(1f, 0.8f, 0.2f),
                _ => Color.white
            };
            _cardButtons[i].colors = colors;
        }
    }

    private void SelectCard(int index)
    {
        if (_currentCards == null || index >= _currentCards.Length) return;

        var selected = _currentCards[index];
        GameEvents.MutationSelected(selected);
        Debug.Log($"Mutation selected: {selected.Name} ({selected.Rarity})");

        _panel.SetActive(false);
        GameEvents.GameResumed();
    }
}
