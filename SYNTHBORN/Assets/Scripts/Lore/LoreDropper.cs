using UnityEngine;
using Synthborn.Core.Events;
using Synthborn.Core.Persistence;
using Synthborn.Enemies;
using Synthborn.Waves;

namespace Synthborn.Lore
{
    /// <summary>
    /// Drops lore fragments from enemies.
    /// Boss kills: guaranteed fragment (biome-specific).
    /// Elite kills: 10% chance.
    /// </summary>
    public class LoreDropper : MonoBehaviour
    {
        [SerializeField] private LoreDatabase _database;
        [SerializeField] private float _eliteDropChance = 0.10f;

        private TrialManager _trialManager;

        private void Start()
        {
            _trialManager = FindAnyObjectByType<TrialManager>();
        }

        private void OnEnable() => GameEvents.OnEnemyDied += OnEnemyDied;
        private void OnDisable() => GameEvents.OnEnemyDied -= OnEnemyDied;

        private void OnEnemyDied(Vector2 pos, GameObject enemy, int xp)
        {
            if (_database == null || _database.fragments == null) return;

            var brain = enemy?.GetComponent<EnemyBrain>();
            if (brain?.Data == null) return;

            bool isBoss = brain.Data.Tier == EnemyTier.Boss;
            bool isElite = brain.Data.Tier == EnemyTier.Elite;

            if (isBoss) { TryDropFragment(true); return; }
            if (isElite && Random.value < _eliteDropChance) TryDropFragment(false);
        }

        private void TryDropFragment(bool guaranteed)
        {
            var ch = SaveManager.Character;
            if (ch == null) return;

            BiomeLayer currentBiome = BiomeLayer.Atrium;
            if (_trialManager?.CurrentChamber != null)
                currentBiome = _trialManager.CurrentChamber.biomeLayer;

            var biomeFragments = _database.GetByBiome(currentBiome);
            var candidates = new System.Collections.Generic.List<LoreFragment>();

            foreach (var f in biomeFragments)
                if (!ch.signalArchiveEntries.Contains(f.fragmentId))
                    candidates.Add(f);

            // Fallback: any undiscovered fragment if guaranteed and none left for biome
            if (candidates.Count == 0 && guaranteed)
            {
                foreach (var f in _database.fragments)
                    if (f != null && !ch.signalArchiveEntries.Contains(f.fragmentId))
                        candidates.Add(f);
            }

            if (candidates.Count == 0) return;

            var chosen = candidates[Random.Range(0, candidates.Count)];
            ch.signalArchiveEntries.Add(chosen.fragmentId);
            SaveManager.SaveSlot();

            GameEvents.RaiseLoreFragmentDiscovered(chosen.fragmentId, chosen.title);
        }
    }
}
