using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Synthborn.Core.Events;
using Synthborn.Core.Items;
using Synthborn.Core.Persistence;
using Synthborn.Waves;

namespace Synthborn.UI
{
    /// <summary>
    /// Shows level complete screen between levels.
    /// Displays gold earned, level reached, and continue button.
    /// Heals player 30% HP on display.
    /// </summary>
    public class LevelTransitionScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _infoText;
        [SerializeField] private Button _continueButton;
        [SerializeField] private LevelManager _levelManager;

        private readonly List<(string name, Color color)> _lootThisRun = new();

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);
            if (_continueButton != null)
                _continueButton.onClick.AddListener(OnContinue);
        }

        private void OnDestroy()
        {
            if (_continueButton != null)
                _continueButton.onClick.RemoveListener(OnContinue);
        }

        private void OnEnable()
        {
            GameEvents.OnLevelCleared += OnLevelCleared;
            GameEvents.OnLootDropped += OnLootDropped;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelCleared -= OnLevelCleared;
            GameEvents.OnLootDropped -= OnLootDropped;
        }

        private void OnLootDropped(string id, string name, int rarity)
        {
            Color col = ((ItemRarity)rarity) switch
            {
                ItemRarity.Common => Color.white,
                ItemRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
                ItemRarity.Rare => new Color(0.3f, 0.5f, 1f),
                ItemRarity.Epic => new Color(0.7f, 0.3f, 0.9f),
                ItemRarity.Legendary => new Color(1f, 0.85f, 0.2f),
                _ => Color.white
            };
            _lootThisRun.Add((name, col));
        }

        private void OnLevelCleared(int level)
        {
            Time.timeScale = 0f;
            if (_panel != null) _panel.SetActive(true);

            if (_titleText != null)
                _titleText.text = $"DENEME ODASI {level} TAMAMLANDI";

            var ch = SaveManager.Character;
            string charInfo = ch != null
                ? $"Character Lv.{ch.characterLevel}  |  XP: {ch.characterXP}/{ch.XPToNextLevel}"
                : "";

            // Build loot list from collected events
            string lootInfo = "";
            if (_lootThisRun.Count > 0)
            {
                lootInfo = "\n\nLoot:";
                foreach (var (name, col) in _lootThisRun)
                    lootInfo += $"\n  <color=#{ColorUtility.ToHtmlStringRGB(col)}>{name}</color>";
            }

            if (_infoText != null)
            {
                _infoText.text = $"Fragment: {FragmentManager.RunFragments}\n{charInfo}\n\nHP restored +30%{lootInfo}";
                _infoText.supportRichText = true;
            }

            _lootThisRun.Clear();

            GameEvents.RaisePlayerHealRequested(0.3f);
        }

        private void OnContinue()
        {
            if (_panel != null) _panel.SetActive(false);

            if (_levelManager != null)
                _levelManager.ReturnToWorldMap();
        }
    }
}
