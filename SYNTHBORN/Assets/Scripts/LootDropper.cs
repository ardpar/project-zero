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

        switch (tierIndex)
        {
            case 0: // Normal — base 5% chance, pressure-boosted
                if (Random.value < 0.05f * (1f + pressureBoost))
                    droppedItem = GetRandomItemOfMinRarity(ItemRarity.Baseline);
                break;
            case 1: // Elite — base 30%, pressure-boosted
                if (Random.value < 0.30f * (1f + pressureBoost))
                    droppedItem = GetRandomItemOfMinRarity(
                        pressureBoost >= 0.3f ? ItemRarity.Reinforced : ItemRarity.Calibrated);
                break;
            case 2: // Stabilized (Boss) — guaranteed, rarity scales with pressure
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

    private ItemData GetStabilizedLoot(int level, float pressureBoost)
    {
        ItemRarity minRarity;
        float roll = Random.value;

        // Pressure boost shifts rarity thresholds
        float legendaryThreshold = 0.10f + pressureBoost * 0.5f; // 10% → 25% at pressure 2
        float epicThreshold = 0.30f + pressureBoost * 0.3f;      // 30% → 39% at pressure 2

        if (level >= 8 && roll < legendaryThreshold)
            minRarity = ItemRarity.ArchitectGrade;
        else if (level >= 4 && roll < epicThreshold)
            minRarity = ItemRarity.Anomalous;
        else
            minRarity = ItemRarity.Reinforced;

        return GetRandomItemOfMinRarity(minRarity);
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
