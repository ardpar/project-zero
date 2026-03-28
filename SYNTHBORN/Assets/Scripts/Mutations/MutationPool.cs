using System.Collections.Generic;
using UnityEngine;
using Synthborn.Core.Persistence;

namespace Synthborn.Mutations
{
    /// <summary>
    /// Generates weighted-random mutation card selections for level-up.
    /// Filters acquired mutations, respects slot availability, applies
    /// rarity scaling, synergy bias, and pity timer.
    /// </summary>
    public class MutationPool
    {
        private readonly MutationDatabase _database;
        private readonly MutationManager _manager;

        // Pity timer: levels without Rare+ / Legendary
        private int _levelsSinceRare;
        private int _levelsSinceLegendary;
        private const int PityRareThreshold = 5;
        private const int PityLegendaryThreshold = 10;

        // Rarity base weights (GDD)
        private const float CommonWeight = 50f;
        private const float UncommonWeight = 30f;
        private const float RareWeight = 15f;
        private const float LegendaryWeight = 5f;

        // Synergy bias multiplier (GDD: 1.5x)
        private const float SynergyBiasMultiplier = 1.5f;

        public MutationPool(MutationDatabase database, MutationManager manager)
        {
            _database = database;
            _manager = manager;
        }

        /// <summary>
        /// Generate 3 unique mutation cards for level-up selection.
        /// Returns fewer than 3 only if the pool is nearly exhausted.
        /// </summary>
        public List<MutationData> GenerateCards(int playerLevel)
        {
            var candidates = BuildCandidateList();
            if (candidates.Count == 0) return GenerateFallbackCards();

            var weights = CalculateWeights(candidates, playerLevel);
            var selected = new List<MutationData>();
            var usedIndices = new HashSet<int>();

            int cardCount = Mathf.Min(3, candidates.Count);
            for (int i = 0; i < cardCount; i++)
            {
                int idx = WeightedRandomSelect(weights, usedIndices);
                if (idx < 0) break;
                usedIndices.Add(idx);
                selected.Add(candidates[idx]);
            }

            // Update pity timer
            bool hasRarePlus = false;
            bool hasLegendary = false;
            foreach (var card in selected)
            {
                if (card.rarity >= MutationRarity.Rare) hasRarePlus = true;
                if (card.rarity == MutationRarity.Legendary) hasLegendary = true;
            }

            if (hasRarePlus) _levelsSinceRare = 0; else _levelsSinceRare++;
            if (hasLegendary) _levelsSinceLegendary = 0; else _levelsSinceLegendary++;

            return selected;
        }

        /// <summary>Reset pity timers for a new run.</summary>
        public void Reset()
        {
            _levelsSinceRare = 0;
            _levelsSinceLegendary = 0;
        }

        // ─────────────────────────────────────────────
        // Internal
        // ─────────────────────────────────────────────

        private List<MutationData> BuildCandidateList()
        {
            var candidates = new List<MutationData>();
            if (_database.allMutations == null) return candidates;

            foreach (var m in _database.allMutations)
            {
                if (m == null) continue;
                // Skip locked mutations
                if (!UnlockManager.IsUnlocked(m.id)) continue;
                // Skip already acquired
                if (_manager.HasMutation(m.id)) continue;
                // Skip slot mutations for full slots
                if (m.category == MutationCategory.Slot && _manager.IsSlotFull(m.slot)) continue;

                candidates.Add(m);
            }
            return candidates;
        }

        private float[] CalculateWeights(List<MutationData> candidates, int playerLevel)
        {
            var activeTags = _manager.GetAllSynergyTags();
            var weights = new float[candidates.Count];

            // Pity: force at least one Rare if threshold met
            bool pityRare = _levelsSinceRare >= PityRareThreshold;
            bool pityLegendary = _levelsSinceLegendary >= PityLegendaryThreshold;

            for (int i = 0; i < candidates.Count; i++)
            {
                var m = candidates[i];
                float w = GetBaseWeight(m.rarity);

                // Level scaling: higher levels slightly boost rare+ weights
                if (m.rarity >= MutationRarity.Rare)
                    w *= 1f + playerLevel * 0.02f;

                // Synergy bias: if mutation has tags that match active tags
                if (m.synergyTags != null && activeTags.Count > 0)
                {
                    foreach (var tag in m.synergyTags)
                    {
                        if (activeTags.Contains(tag))
                        {
                            w *= SynergyBiasMultiplier;
                            break;
                        }
                    }
                }

                // Pity timer boost
                if (pityLegendary && m.rarity == MutationRarity.Legendary)
                    w *= 10f;
                else if (pityRare && m.rarity >= MutationRarity.Rare)
                    w *= 5f;

                weights[i] = w;
            }

            return weights;
        }

        private static float GetBaseWeight(MutationRarity rarity)
        {
            return rarity switch
            {
                MutationRarity.Common => CommonWeight,
                MutationRarity.Uncommon => UncommonWeight,
                MutationRarity.Rare => RareWeight,
                MutationRarity.Legendary => LegendaryWeight,
                _ => CommonWeight
            };
        }

        private static int WeightedRandomSelect(float[] weights, HashSet<int> excluded)
        {
            float total = 0f;
            for (int i = 0; i < weights.Length; i++)
                if (!excluded.Contains(i)) total += weights[i];

            if (total <= 0f) return -1;

            float roll = Random.Range(0f, total);
            float cumulative = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                if (excluded.Contains(i)) continue;
                cumulative += weights[i];
                if (roll <= cumulative) return i;
            }
            return -1;
        }

        private static List<MutationData> GenerateFallbackCards()
        {
            // GDD: when pool is exhausted, offer generic stat boost cards
            // For now return empty — MutationSelectionUI handles empty case
            return new List<MutationData>();
        }
    }
}
