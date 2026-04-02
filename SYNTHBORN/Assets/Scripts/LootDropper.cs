using UnityEngine;
using Synthborn.Core.Events;
using Synthborn.Core.Items;
using Synthborn.Core.Persistence;
using Synthborn.Enemies;
using Synthborn.Waves;

/// <summary>
/// Handles item drops from enemies. Listens to OnEnemyDied,
/// rolls loot based on enemy tier, current chamber pressure, and biome.
/// Attach to GameManager in SampleScene.
/// </summary>
public class LootDropper : MonoBehaviour
{
    [SerializeField] private ItemDatabase _itemDatabase;
    [SerializeField] private LootConfig _lootConfig;

    private readonly System.Collections.Generic.List<string> _runLoot = new();
    private TrialManager _trialManager;

    public System.Collections.Generic.IReadOnlyList<string> RunLoot => _runLoot;

    private void OnEnable()
    {
        GameEvents.OnEnemyDied += OnEnemyDied;
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyDied -= OnEnemyDied;
    }

    private void Start()
    {
        _runLoot.Clear();
        if (_itemDatabase != null) InventoryManager.SetDatabase(_itemDatabase);
        _trialManager = FindAnyObjectByType<TrialManager>();
    }

    private void OnEnemyDied(Vector2 pos, GameObject enemy, int xp)
    {
        if (_itemDatabase == null || _itemDatabase.allItems == null) return;

        var brain = enemy?.GetComponent<EnemyBrain>();
        if (brain?.Data == null) return;

        int tierIndex = (int)brain.Data.Tier;
        int currentLevel = PlayerPrefs.GetInt("SelectedLevel", 1);

        // Get pressure from TrialManager for rarity scaling
        float pressureBoost = 0f;
        if (_trialManager != null && _trialManager.IsTrialActive && _trialManager.CurrentChamber != null)
            pressureBoost = _trialManager.CurrentChamber.pressureRating * 0.15f;

        ItemData droppedItem = null;

        float normalChance = _lootConfig != null ? _lootConfig.normalDropChance : 0.05f;
        float eliteChance = _lootConfig != null ? _lootConfig.eliteDropChance : 0.30f;
        float eliteUpgrade = _lootConfig != null ? _lootConfig.eliteRarityUpgradeThreshold : 0.3f;

        switch (tierIndex)
        {
            case 0: // Normal
                if (Random.value < normalChance * (1f + pressureBoost))
                    droppedItem = GetRandomItemOfMinRarity(ItemRarity.Baseline);
                break;
            case 1: // Elite
                if (Random.value < eliteChance * (1f + pressureBoost))
                    droppedItem = GetRandomItemOfMinRarity(
                        pressureBoost >= eliteUpgrade ? ItemRarity.Reinforced : ItemRarity.Calibrated);
                break;
            case 2: // Stabilized (Boss) — guaranteed
                droppedItem = GetStabilizedLoot(currentLevel, pressureBoost);
                break;
        }

        if (droppedItem != null)
        {
            InventoryManager.AddToInventory(droppedItem.Id);
            _runLoot.Add(droppedItem.Id);
            GameEvents.RaiseLootDropped(droppedItem.Id, droppedItem.DisplayName, droppedItem.Rarity);
        }
    }

    /// <summary>Boss-specific Architect-Grade item IDs, keyed by EnemyData asset name.</summary>
    private static readonly System.Collections.Generic.Dictionary<string, string> BossArchitectItems = new()
    {
        { "SummonerData", "architect_summoner_crown" },
        { "CavernGuardianData", "architect_guardian_plate" },
        { "HellLordData", "architect_helllord_blade" },
        { "JungleBeastData", "architect_junglebeast_array" },
        { "TempleKeeperData", "architect_templekeeper_frame" }
    };

    private ItemData GetStabilizedLoot(int level, float pressureBoost)
    {
        ItemRarity minRarity;
        float roll = Random.value;

        float legBase = _lootConfig != null ? _lootConfig.legendaryBaseThreshold : 0.10f;
        float legScale = _lootConfig != null ? _lootConfig.legendaryPressureScale : 0.5f;
        float epicBase = _lootConfig != null ? _lootConfig.epicBaseThreshold : 0.30f;
        float epicScale = _lootConfig != null ? _lootConfig.epicPressureScale : 0.3f;
        int legMinLvl = _lootConfig != null ? _lootConfig.legendaryMinLevel : 8;
        int epicMinLvl = _lootConfig != null ? _lootConfig.epicMinLevel : 4;

        float legendaryThreshold = legBase + pressureBoost * legScale;
        float epicThreshold = epicBase + pressureBoost * epicScale;

        if (level >= legMinLvl && roll < legendaryThreshold)
        {
            var bossItem = GetBossSpecificItem();
            if (bossItem != null) return bossItem;
            minRarity = ItemRarity.ArchitectGrade;
        }
        else if (level >= epicMinLvl && roll < epicThreshold)
            minRarity = ItemRarity.Anomalous;
        else
            minRarity = ItemRarity.Reinforced;

        return GetRandomItemOfMinRarity(minRarity);
    }

    private ItemData GetBossSpecificItem()
    {
        if (_trialManager == null || !_trialManager.IsTrialActive) return null;
        var bossData = _trialManager.CurrentChamber?.bossData;
        if (bossData == null) return null;

        if (BossArchitectItems.TryGetValue(bossData.name, out string itemId))
            return _itemDatabase.GetById(itemId);

        return null;
    }

    private readonly System.Collections.Generic.List<ItemData> _candidates = new();

    private ItemData GetRandomItemOfMinRarity(ItemRarity minRarity)
    {
        _candidates.Clear();
        foreach (var item in _itemDatabase.allItems)
            if (item != null && (int)item.Rarity >= (int)minRarity)
                _candidates.Add(item);

        if (_candidates.Count == 0)
            foreach (var item in _itemDatabase.allItems)
                if (item != null) _candidates.Add(item);

        return _candidates.Count > 0 ? _candidates[Random.Range(0, _candidates.Count)] : null;
    }
}
