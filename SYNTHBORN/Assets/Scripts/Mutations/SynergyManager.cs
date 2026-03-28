using System.Collections.Generic;
using UnityEngine;
using Synthborn.Core.Stats;
using Synthborn.Core.Events;

namespace Synthborn.Mutations
{
    /// <summary>
    /// Checks for synergy activation after each mutation is applied.
    /// Subscribes to OnMutationApplied and evaluates all synergies
    /// against current active tags. Each synergy can only activate once per run.
    /// </summary>
    public class SynergyManager : MonoBehaviour
    {
        [SerializeField] private SynergyData[] _allSynergies;

        private MutationManager _mutationManager;
        private CombatStatBlock _stats;
        private readonly HashSet<string> _activatedSynergies = new();

        /// <summary>Active synergy IDs this run.</summary>
        public IReadOnlyCollection<string> ActivatedSynergies => _activatedSynergies;

        public void Initialize(MutationManager mutationManager, CombatStatBlock stats)
        {
            _mutationManager = mutationManager;
            _stats = stats;
        }

        private void OnEnable()
        {
            GameEvents.OnMutationApplied += OnMutationApplied;
        }

        private void OnDisable()
        {
            GameEvents.OnMutationApplied -= OnMutationApplied;
        }

        private void OnMutationApplied(string mutationId, bool isSlot)
        {
            if (_mutationManager == null || _allSynergies == null) return;

            var activeTags = _mutationManager.GetAllSynergyTags();
            CheckSynergies(activeTags);
        }

        private void CheckSynergies(HashSet<string> activeTags)
        {
            foreach (var synergy in _allSynergies)
            {
                if (synergy == null) continue;
                if (_activatedSynergies.Contains(synergy.id)) continue;
                if (synergy.requiredTags == null || synergy.requiredTags.Length == 0) continue;

                bool allTagsPresent = true;
                foreach (var tag in synergy.requiredTags)
                {
                    if (!activeTags.Contains(tag))
                    {
                        allTagsPresent = false;
                        break;
                    }
                }

                if (allTagsPresent)
                    ActivateSynergy(synergy);
            }
        }

        private void ActivateSynergy(SynergyData synergy)
        {
            _activatedSynergies.Add(synergy.id);

            // Apply bonus stats
            _stats?.ApplyMutation(
                speedModifier: synergy.speedModifier,
                dashCdModifier: synergy.dashCooldownModifier,
                hpModifier: synergy.hpModifier,
                armorFlat: synergy.armorFlat,
                damageModifier: synergy.damageModifier,
                critChance: synergy.critChance,
                critMultiplierBonus: synergy.critMultiplierBonus,
                attackSpeedModifier: synergy.attackSpeedModifier
            );

            // Fire event for UI banner / SFX
            GameEvents.RaiseSynergyActivated(synergy.id, synergy.displayName);

            Debug.Log($"[Synergy] Activated: {synergy.displayName}");
        }

        /// <summary>Reset for new run.</summary>
        public void ResetForNewRun()
        {
            // Remove activated synergy stats
            foreach (var id in _activatedSynergies)
            {
                foreach (var s in _allSynergies)
                {
                    if (s != null && s.id == id)
                    {
                        _stats?.RemoveMutation(
                            speedModifier: s.speedModifier,
                            dashCdModifier: s.dashCooldownModifier,
                            hpModifier: s.hpModifier,
                            armorFlat: s.armorFlat,
                            damageModifier: s.damageModifier,
                            critChance: s.critChance,
                            critMultiplierBonus: s.critMultiplierBonus,
                            attackSpeedModifier: s.attackSpeedModifier
                        );
                        break;
                    }
                }
            }
            _activatedSynergies.Clear();
        }
    }
}
