using System.Collections.Generic;
using UnityEngine;
using Synthborn.Core.Stats;
using Synthborn.Core.Events;

namespace Synthborn.Mutations
{
    /// <summary>
    /// Manages active mutations for a run. Tracks slot and passive mutations,
    /// applies/removes stat modifiers to CombatStatBlock, and fires events
    /// for Sprite Compositing and Synergy Matrix.
    /// </summary>
    public class MutationManager : MonoBehaviour
    {
        // ─────────────────────────────────────────────
        // State
        // ─────────────────────────────────────────────

        private CombatStatBlock _stats;
        private readonly Dictionary<MutationSlot, MutationData> _slotMutations = new();
        private readonly List<MutationData> _passiveMutations = new();
        private readonly HashSet<string> _acquiredIds = new();

        // ─────────────────────────────────────────────
        // Public read API
        // ─────────────────────────────────────────────

        /// <summary>All acquired mutation IDs this run.</summary>
        public IReadOnlyCollection<string> AcquiredIds => _acquiredIds;

        /// <summary>All active passive mutations.</summary>
        public IReadOnlyList<MutationData> PassiveMutations => _passiveMutations;

        /// <summary>Get the slot mutation for a given slot. Null if empty.</summary>
        public MutationData GetSlotMutation(MutationSlot slot) =>
            _slotMutations.TryGetValue(slot, out var m) ? m : null;

        /// <summary>True if the given slot is occupied.</summary>
        public bool IsSlotFull(MutationSlot slot) => _slotMutations.ContainsKey(slot);

        /// <summary>Number of filled body slots (max 4).</summary>
        public int FilledSlotCount => _slotMutations.Count;

        /// <summary>True if all 4 body slots are filled.</summary>
        public bool AllSlotsFull => _slotMutations.Count >= 4;

        /// <summary>Total mutation count (slots + passives).</summary>
        public int TotalMutationCount => _slotMutations.Count + _passiveMutations.Count;

        /// <summary>Check if a specific mutation has been acquired.</summary>
        public bool HasMutation(string id) => _acquiredIds.Contains(id);

        /// <summary>All active synergy tags from all mutations.</summary>
        public HashSet<string> GetAllSynergyTags()
        {
            var tags = new HashSet<string>();
            foreach (var kvp in _slotMutations)
                if (kvp.Value.synergyTags != null)
                    foreach (var t in kvp.Value.synergyTags) tags.Add(t);
            foreach (var m in _passiveMutations)
                if (m.synergyTags != null)
                    foreach (var t in m.synergyTags) tags.Add(t);
            return tags;
        }

        // ─────────────────────────────────────────────
        // Initialization
        // ─────────────────────────────────────────────

        /// <summary>Inject CombatStatBlock. Call once at run start.</summary>
        public void Initialize(CombatStatBlock stats)
        {
            _stats = stats;
        }

        /// <summary>Reset all mutations for a new run.</summary>
        public void ResetForNewRun()
        {
            // Remove all stat contributions
            foreach (var kvp in _slotMutations)
                RemoveStats(kvp.Value);
            foreach (var m in _passiveMutations)
                RemoveStats(m);

            _slotMutations.Clear();
            _passiveMutations.Clear();
            _acquiredIds.Clear();
        }

        // ─────────────────────────────────────────────
        // Core API
        // ─────────────────────────────────────────────

        /// <summary>
        /// Apply a mutation. Returns false if the mutation can't be applied
        /// (duplicate, or slot already full).
        /// </summary>
        public bool ApplyMutation(MutationData mutation)
        {
            if (mutation == null) return false;
            if (_acquiredIds.Contains(mutation.id)) return false;

            if (mutation.category == MutationCategory.Slot)
            {
                if (mutation.slot == MutationSlot.None) return false;
                if (_slotMutations.ContainsKey(mutation.slot)) return false;

                _slotMutations[mutation.slot] = mutation;
            }
            else
            {
                _passiveMutations.Add(mutation);
            }

            _acquiredIds.Add(mutation.id);
            ApplyStats(mutation);

            // Fire events for other systems
            GameEvents.RaiseMutationApplied(mutation.id, mutation.category == MutationCategory.Slot);

            return true;
        }

        // ─────────────────────────────────────────────
        // Stat helpers
        // ─────────────────────────────────────────────

        private void ApplyStats(MutationData m)
        {
            if (_stats == null) return;
            _stats.ApplyMutation(
                speedModifier: m.speedModifier,
                dashCdModifier: m.dashCooldownModifier,
                hpModifier: m.hpModifier,
                armorFlat: m.armorFlat,
                damageModifier: m.damageModifier,
                critChance: m.critChance,
                critMultiplierBonus: m.critMultiplierBonus,
                attackSpeedModifier: m.attackSpeedModifier
            );
        }

        private void RemoveStats(MutationData m)
        {
            if (_stats == null) return;
            _stats.RemoveMutation(
                speedModifier: m.speedModifier,
                dashCdModifier: m.dashCooldownModifier,
                hpModifier: m.hpModifier,
                armorFlat: m.armorFlat,
                damageModifier: m.damageModifier,
                critChance: m.critChance,
                critMultiplierBonus: m.critMultiplierBonus,
                attackSpeedModifier: m.attackSpeedModifier
            );
        }
    }
}
