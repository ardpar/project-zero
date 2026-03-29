using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Events;
using Synthborn.Mutations;

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
            GameEvents.OnPlayerDied += OnPlayerDied;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelUp -= OnLevelUp;
            GameEvents.OnPlayerDied -= OnPlayerDied;
        }

        private void OnLevelUp(int newLevel)
        {
            _playerLevel = newLevel;
            Show();
        }

        private void Show()
        {
            // Guard against double-open on rapid level-up
            if (_panel != null && _panel.activeSelf) return;

            _currentCards = _pool.GenerateCards(_playerLevel);
            if (_currentCards.Count == 0) return;

            // Pause game
            Time.timeScale = 0f;
            GameEvents.RaiseGamePaused();
            if (_panel != null) _panel.SetActive(true);

            // Populate and animate cards
            for (int i = 0; i < _cardButtons.Length; i++)
            {
                if (i < _currentCards.Count)
                {
                    _cardButtons[i].gameObject.SetActive(true);
                    var card = _currentCards[i];

                    // Slide-in animation (staggered)
                    StartCoroutine(SlideInCard(_cardButtons[i].GetComponent<RectTransform>(), i * 0.1f));

                    if (_cardTitles.Length > i && _cardTitles[i] != null)
                        _cardTitles[i].text = card.displayName;

                    if (_cardDescriptions.Length > i && _cardDescriptions[i] != null)
                        _cardDescriptions[i].text = card.description + "\n" + BuildStatText(card);

                    if (_cardRarities.Length > i && _cardRarities[i] != null)
                    {
                        string slotLabel = card.category == MutationCategory.Slot ? $" [{card.slot}]" : " [Passive]";
                        string synergyHint = GetSynergyHint(card);
                        _cardRarities[i].text = card.rarity + slotLabel + synergyHint;
                    }

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

        private void OnPlayerDied()
        {
            // If mutation selection is open when the player dies,
            // close it so the game-over screen can take over.
            if (_panel != null && _panel.activeSelf)
            {
                StopAllCoroutines();
                Hide();
                // Don't restore timeScale — RunManager handles it for game-over
            }
        }

        private void OnCardSelected(int index)
        {
            if (_currentCards == null || index >= _currentCards.Count) return;

            var selected = _currentCards[index];
            _mutationManager.ApplyMutation(selected);

            // Resume game
            Hide();
            Time.timeScale = 1f;
            GameEvents.RaiseGameResumed();
        }

        private IEnumerator SlideInCard(RectTransform rt, float delay)
        {
            if (rt == null) yield break;

            Vector2 targetPos = rt.anchoredPosition;
            Vector2 startPos = targetPos + new Vector2(0, -400f); // Start below
            rt.anchoredPosition = startPos;

            // Stagger delay (unscaled since timeScale=0)
            if (delay > 0f)
                yield return new WaitForSecondsRealtime(delay);

            // Slide up (0.3s ease-out)
            float duration = 0.3f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, t / duration);
                rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, progress);
                yield return null;
            }
            rt.anchoredPosition = targetPos;
        }

        private static string BuildStatText(MutationData m)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (m.damageModifier != 0) parts.Add($"DMG {m.damageModifier:+0%;-0%}");
            if (m.attackSpeedModifier != 0) parts.Add($"ATK SPD {m.attackSpeedModifier:+0%;-0%}");
            if (m.speedModifier != 0) parts.Add($"SPD {m.speedModifier:+0%;-0%}");
            if (m.critChance != 0) parts.Add($"CRIT {m.critChance:+0%;-0%}");
            if (m.critMultiplierBonus != 0) parts.Add($"CRIT x{m.critMultiplierBonus:+0.0}");
            if (m.hpModifier != 0) parts.Add($"HP {m.hpModifier:+0%;-0%}");
            if (m.armorFlat != 0) parts.Add($"ARMOR {m.armorFlat:+0;-0}");
            if (m.dashCooldownModifier != 0) parts.Add($"DASH CD {m.dashCooldownModifier:+0%;-0%}");
            return parts.Count > 0 ? string.Join("  ", parts) : "";
        }

        private string GetSynergyHint(MutationData card)
        {
            if (_mutationManager == null || card.synergyTags == null) return "";
            var activeTags = _mutationManager.GetAllSynergyTags();
            foreach (var tag in card.synergyTags)
            {
                if (activeTags.Contains(tag))
                    return "\n<color=yellow>Synergy!</color>";
            }
            return "";
        }

        private static Color GetRarityColor(MutationRarity rarity)
        {
            return rarity switch
            {
                MutationRarity.Baseline => new Color(0.7f, 0.7f, 0.7f),      // Gray
                MutationRarity.Calibrated => new Color(0.2f, 0.8f, 0.2f),    // Green
                MutationRarity.Reinforced => new Color(0.3f, 0.5f, 1.0f),        // Blue
                MutationRarity.ArchitectGrade => new Color(1.0f, 0.8f, 0.0f),   // Gold
                _ => Color.white
            };
        }
    }
}
