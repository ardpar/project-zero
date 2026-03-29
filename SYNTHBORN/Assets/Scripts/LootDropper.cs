using UnityEngine;
using Synthborn.Core.Events;
using Synthborn.Core.Items;
using Synthborn.Core.Persistence;
using Synthborn.Enemies;

/// <summary>
/// Handles item drops from enemies. Listens to OnEnemyDied,
/// rolls loot based on enemy tier and current level.
/// Attach to GameManager in SampleScene.
/// </summary>
public class LootDropper : MonoBehaviour
{
    [SerializeField] private ItemDatabase _itemDatabase;

    private readonly System.Collections.Generic.List<string> _runLoot = new();

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
    }

    private void OnEnemyDied(Vector2 pos, GameObject enemy, int xp)
    {
        if (_itemDatabase == null || _itemDatabase.allItems == null) return;

        var brain = enemy?.GetComponent<EnemyBrain>();
        if (brain?.Data == null) return;

        int tierIndex = (int)brain.Data.Tier;
        int currentLevel = PlayerPrefs.GetInt("SelectedLevel", 1);

        ItemData droppedItem = null;

        switch (tierIndex)
        {
            case 0: // Normal — 5% chance Common
                if (Random.value < 0.05f)
                    droppedItem = GetRandomItemOfMinRarity(ItemRarity.Common);
                break;
            case 1: // Elite — 30% chance Uncommon/Rare
                if (Random.value < 0.30f)
                    droppedItem = GetRandomItemOfMinRarity(ItemRarity.Uncommon);
                break;
            case 2: // Boss — guaranteed Rare+
                droppedItem = GetBossLoot(currentLevel);
                break;
        }

        if (droppedItem != null)
        {
            InventoryManager.AddToInventory(droppedItem.Id);
            _runLoot.Add(droppedItem.Id);
            GameEvents.RaiseLootDropped(droppedItem.Id, droppedItem.DisplayName, droppedItem.Rarity);
        }
    }

    private ItemData GetBossLoot(int level)
    {
        ItemRarity minRarity;
        float roll = Random.value;

        if (level >= 8 && roll < 0.10f)
            minRarity = ItemRarity.Legendary;
        else if (level >= 4 && roll < 0.30f)
            minRarity = ItemRarity.Epic;
        else
            minRarity = ItemRarity.Rare;

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
