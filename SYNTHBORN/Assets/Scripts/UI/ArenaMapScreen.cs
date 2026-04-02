using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Synthborn.Core.Persistence;
using Synthborn.Waves;

namespace Synthborn.UI
{
    /// <summary>
    /// Arena Map hub: 10x10 Trial Chamber grid with biome color-coding,
    /// pressure ratings, locked/open/completed states.
    /// Replaces WorldMapScreen's level grid for the Trial Chamber system.
    /// </summary>
    public class ArenaMapScreen : MonoBehaviour
    {
        [Header("Character Info")]
        [SerializeField] private Text _charInfoText;

        [Header("Chamber Grid")]
        [SerializeField] private Transform _chamberGridContainer;
        [SerializeField] private int _totalChambers = 100;

        [Header("Chamber Definitions")]
        [SerializeField] private TrialChamberData[] _chambers;

        [Header("Biome Configs")]
        [SerializeField] private BiomeConfig[] _biomeConfigs;

        [Header("Room Info Panel")]
        [SerializeField] private GameObject _roomInfoPanel;
        [SerializeField] private Text _roomInfoTitle;
        [SerializeField] private Text _roomInfoDetails;
        [SerializeField] private Button _roomInfoStartButton;
        [SerializeField] private Button _roomInfoCloseButton;

        [Header("Navigation")]
        [SerializeField] private Button _saveQuitButton;

        [SerializeField] private Font _font;

        private int _selectedChamber;

        private void Start()
        {
            if (SaveManager.Character != null)
            {
                SaveManager.SaveSlot();
                SaveManager.Save();
            }

            RefreshCharacterInfo();
            RefreshChamberGrid();

            if (_roomInfoPanel != null) _roomInfoPanel.SetActive(false);

            if (_saveQuitButton != null)
                _saveQuitButton.onClick.AddListener(OnSaveQuit);

            if (_roomInfoStartButton != null)
                _roomInfoStartButton.onClick.AddListener(OnStartTrial);

            if (_roomInfoCloseButton != null)
                _roomInfoCloseButton.onClick.AddListener(() => _roomInfoPanel?.SetActive(false));
        }

        private void OnDestroy()
        {
            if (_saveQuitButton != null)
                _saveQuitButton.onClick.RemoveAllListeners();
            if (_roomInfoStartButton != null)
                _roomInfoStartButton.onClick.RemoveAllListeners();
            if (_roomInfoCloseButton != null)
                _roomInfoCloseButton.onClick.RemoveAllListeners();
        }

        private void RefreshCharacterInfo()
        {
            var ch = SaveManager.Character;
            if (ch == null || _charInfoText == null) return;

            string className = ch.classType switch
            {
                0 => "Dense Lattice", 1 => "Severed Thread", 2 => "Null Cascade", 3 => "Balanced Frame", _ => "?"
            };

            _charInfoText.text = $"{ch.characterName}  |  {className}  |  Lv.{ch.characterLevel}  |  Fragment: {ch.gold}";
        }

        private void RefreshChamberGrid()
        {
            if (_chamberGridContainer == null) return;
            foreach (Transform child in _chamberGridContainer) Destroy(child.gameObject);

            var ch = SaveManager.Character;
            if (ch == null) return;

            for (int i = 1; i <= _totalChambers; i++)
                CreateChamberButton(i, ch);
        }

        private void CreateChamberButton(int chamberNumber, CharacterSaveData ch)
        {
            bool unlocked = ch.IsChamberUnlocked(chamberNumber);
            bool completed = ch.IsChamberCompleted(chamberNumber);

            var chamber = FindChamber(chamberNumber);
            BiomeLayer biome = chamber != null ? chamber.biomeLayer : GetDefaultBiome(chamberNumber);
            int pressure = chamber != null ? chamber.pressureRating : GetDefaultPressure(chamberNumber);

            var btnGO = new GameObject($"Chamber_{chamberNumber}", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(_chamberGridContainer, false);
            var rect = btnGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(58, 58);

            // Color by biome + state
            var biomeConfig = FindBiomeConfig(biome);
            var img = btnGO.GetComponent<Image>();
            if (completed)
                img.color = biomeConfig != null ? biomeConfig.mapCompletedColor : new Color(0.15f, 0.35f, 0.15f);
            else if (unlocked)
                img.color = biomeConfig != null ? biomeConfig.mapAccentColor : new Color(0.25f, 0.20f, 0.35f);
            else
                img.color = biomeConfig != null ? biomeConfig.mapLockedColor : new Color(0.1f, 0.1f, 0.1f);

            var btn = btnGO.GetComponent<Button>();
            btn.interactable = unlocked;

            // Chamber number + pressure stars
            var textGO = new GameObject("Num", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(btnGO.transform, false);
            var tRect = textGO.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;
            var text = textGO.GetComponent<Text>();

            string starStr = new string('\u2605', pressure);
            string statusIcon = completed ? "\n<size=10>\u2713</size>" : (!unlocked ? "" : "");
            text.text = $"{chamberNumber}\n<size=7>{starStr}</size>{statusIcon}";
            text.fontSize = 14;
            text.color = unlocked ? Color.white : new Color(0.4f, 0.4f, 0.4f);
            text.alignment = TextAnchor.MiddleCenter;
            text.font = _font;
            text.supportRichText = true;
            text.raycastTarget = false;

            if (unlocked)
            {
                int captured = chamberNumber;
                btn.onClick.AddListener(() => OnChamberSelected(captured));
            }
        }

        private void OnChamberSelected(int chamberNumber)
        {
            _selectedChamber = chamberNumber;

            if (_roomInfoPanel == null)
            {
                // No info panel — go directly
                StartTrial(chamberNumber);
                return;
            }

            _roomInfoPanel.SetActive(true);
            var chamber = FindChamber(chamberNumber);
            var ch = SaveManager.Character;
            bool completed = ch != null && ch.IsChamberCompleted(chamberNumber);

            if (_roomInfoTitle != null)
            {
                string name = chamber != null ? chamber.levelName : $"Deneme Odas\u0131 {chamberNumber}";
                _roomInfoTitle.text = name;
            }

            if (_roomInfoDetails != null)
            {
                BiomeLayer biome = chamber != null ? chamber.biomeLayer : GetDefaultBiome(chamberNumber);
                int pressure = chamber != null ? chamber.pressureRating : GetDefaultPressure(chamberNumber);
                var biomeConfig = FindBiomeConfig(biome);
                string biomeName = biomeConfig != null ? biomeConfig.displayName : biome.ToString();
                string stars = new string('\u2605', pressure);
                string status = completed ? "Tamamland\u0131" : "Hen\u00fcz tamamlanmad\u0131";
                string lore = biomeConfig != null ? biomeConfig.loreDescription : "";
                int waves = chamber != null ? chamber.waveCount : 5;
                int estMinutes = Mathf.CeilToInt(waves * 1.5f);

                // Enemy type summary
                string enemies = "";
                if (chamber?.spawnPool != null && chamber.spawnPool.Length > 0)
                {
                    var names = new System.Collections.Generic.List<string>();
                    foreach (var entry in chamber.spawnPool)
                    {
                        if (entry.EnemyData != null && !names.Contains(entry.EnemyData.name))
                            names.Add(entry.EnemyData.name.Replace("Data", ""));
                    }
                    enemies = string.Join(", ", names);
                }

                _roomInfoDetails.text =
                    $"Biome: {biomeName}\n<size=11><color=#888>{lore}</color></size>\n" +
                    $"Bas\u0131n\u00e7: {stars}  |  Dalga: {waves}  |  ~{estMinutes} dk\n" +
                    $"D\u00fc\u015fmanlar: {enemies}\n" +
                    $"Durum: {status}";
            }

            if (_roomInfoStartButton != null)
                _roomInfoStartButton.gameObject.SetActive(true);
        }

        private void OnStartTrial()
        {
            StartTrial(_selectedChamber);
        }

        private void StartTrial(int chamberNumber)
        {
            RunSessionData.SelectedChamber = chamberNumber;
            RunSessionData.SelectedLevel = 0; // Clear legacy level
            SceneManager.LoadScene("SampleScene");
        }

        private void OnSaveQuit()
        {
            SaveManager.SaveSlot();
            SaveManager.Save();
            SceneManager.LoadScene("MainMenu");
        }

        // ─── Helpers ───

        private TrialChamberData FindChamber(int chamberNumber)
        {
            if (_chambers == null) return null;
            foreach (var c in _chambers)
            {
                if (c != null && c.chamberNumber == chamberNumber)
                    return c;
            }
            return null;
        }

        private BiomeConfig FindBiomeConfig(BiomeLayer layer)
        {
            if (_biomeConfigs == null) return null;
            foreach (var bc in _biomeConfigs)
            {
                if (bc != null && bc.biomeLayer == layer)
                    return bc;
            }
            return null;
        }

        /// <summary>Default biome for chambers without SO definitions.</summary>
        private static BiomeLayer GetDefaultBiome(int chamberNumber)
        {
            if (chamberNumber <= 16) return BiomeLayer.Atrium;
            if (chamberNumber <= 33) return BiomeLayer.AssayChambers;
            if (chamberNumber <= 50) return BiomeLayer.DeepArchive;
            if (chamberNumber <= 67) return BiomeLayer.CollapseStratum;
            if (chamberNumber <= 84) return BiomeLayer.CorruptionLayer;
            return BiomeLayer.NullChamber;
        }

        /// <summary>Default pressure for chambers without SO definitions.</summary>
        private static int GetDefaultPressure(int chamberNumber)
        {
            return Mathf.Clamp((chamberNumber - 1) / 20 + 1, 1, 5);
        }
    }
}
