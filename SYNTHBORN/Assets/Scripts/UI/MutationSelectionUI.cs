using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Events;
using Synthborn.Mutations;
using System.Collections.Generic;

namespace Synthborn.UI
{
    /// <summary>
    /// Level-up mutation card selection screen.
    /// Pauses game, shows 3 cards, player picks one, resumes.
    /// </summary>
    public class MutationSelectionUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button[] _cardButtons;
        [SerializeField] private Text[] _cardTitles;
        [SerializeField] private Text[] _cardDescriptions;
        [SerializeField] private Text[] _cardRarities;
        [SerializeField] private Image[] _cardBackgrounds;

        [Header("Data")]
        [SerializeField] private MutationDatabase _database;

        private MutationManager _mutationManager;
        private MutationPool _pool;
        private List<MutationData> _currentCards;
        private int _playerLevel = 1;

        /// <summary>Inject dependencies at runtime (called by GameBootstrap).</summary>
        public void Initialize(MutationManager manager, MutationDatabase database)
        {
            _mutationManager = manager;
            _database = database;
            _pool = new MutationPool(database, manager);

            // Wire button clicks
            for (int i = 0; i < _cardButtons.Length; i++)
            {
                int idx = i; // capture for closure
                _cardButtons[i].onClick.AddListener(() => OnCardSelected(idx));
            }

            Hide();
        }

        private void OnEnable()
        {
            GameEvents.OnLevelUp += OnLevelUp;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelUp -= OnLevelUp;
        }

        private void OnLevelUp(int newLevel)
        {
            _playerLevel = newLevel;
            Show();
        }

        private void Show()
        {
            _currentCards = _pool.GenerateCards(_playerLevel);
            if (_currentCards.Count == 0)
            {
                // No mutations left — skip selection
                return;
            }

            // Pause game
            Time.timeScale = 0f;
            _panel.SetActive(true);

            // Populate cards
            for (int i = 0; i < _cardButtons.Length; i++)
            {
                if (i < _currentCards.Count)
                {
                    _cardButtons[i].gameObject.SetActive(true);
                    var card = _currentCards[i];

                    if (_cardTitles.Length > i && _cardTitles[i] != null)
                        _cardTitles[i].text = card.displayName;

                    if (_cardDescriptions.Length > i && _cardDescriptions[i] != null)
                        _cardDescriptions[i].text = card.description;

                    if (_cardRarities.Length > i && _cardRarities[i] != null)
                        _cardRarities[i].text = card.rarity.ToString();

                    if (_cardBackgrounds.Length > i && _cardBackgrounds[i] != null)
                        _cardBackgrounds[i].color = GetRarityColor(card.rarity);
                }
                else
                {
                    _cardButtons[i].gameObject.SetActive(false);
                }
            }
        }

        private void Hide()
        {
            if (_panel != null)
                _panel.SetActive(false);
        }

        private void OnCardSelected(int index)
        {
            if (_currentCards == null || index >= _currentCards.Count) return;

            var selected = _currentCards[index];
            _mutationManager.ApplyMutation(selected);

            // Resume game
            Hide();
            Time.timeScale = 1f;
        }

        private static Color GetRarityColor(MutationRarity rarity)
        {
            return rarity switch
            {
                MutationRarity.Common => new Color(0.7f, 0.7f, 0.7f),      // Gray
                MutationRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),    // Green
                MutationRarity.Rare => new Color(0.3f, 0.5f, 1.0f),        // Blue
                MutationRarity.Legendary => new Color(1.0f, 0.8f, 0.0f),   // Gold
                _ => Color.white
            };
        }
    }
}
