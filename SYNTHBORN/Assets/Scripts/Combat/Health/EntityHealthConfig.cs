// Implements: ADR-002 — EntityHealthConfig ScriptableObject (Tier 1)
// Design doc: entity-health-system.md — Tuning Knobs section
//
// Shared by both player and enemy EntityHealth components.
// Designers set base values here; CombatStatBlock supplies runtime modifiers.

using UnityEngine;

namespace Synthborn.Combat.Health
{
    /// <summary>
    /// ScriptableObject holding all tunable parameters for EntityHealth.
    /// One asset per entity type (e.g. PlayerHealthConfig, BatHealthConfig).
    /// Create via Assets > Create > Synthborn > Entity Health Config.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Entity Health Config", fileName = "EntityHealthConfig")]
    public class EntityHealthConfig : ScriptableObject
    {
        [Header("HP")]
        [Tooltip("Base maximum HP. GDD player default: 100. Safe range: 50-200.")]
        [Min(1)]
        public int baseMaxHp = 100;

        [Header("Armor")]
        [Tooltip("Starting flat armor value. GDD player default: 0 (gained via mutations).")]
        [Min(0)]
        public int baseArmor = 0;

        [Header("Invulnerability")]
        [Tooltip("Enable post-hit invulnerability window. Enable for player; disable for enemies (GDD rule 10 + Note).")]
        public bool useInvulnerability = true;

        [Tooltip("Duration of the post-hit invulnerability window in seconds. GDD default: 0.5. Safe range: 0.2-1.0.")]
        [Range(0.1f, 2.0f)]
        public float invulnerabilityWindow = 0.5f;

        [Header("HP Scaling (Enemies only — ignored for player)")]
        [Tooltip("Wave-scaling factor. GDD default: 0.15 (15% HP per wave). Safe range: 0.1-0.3.")]
        [Range(0.0f, 0.3f)]
        public float waveHpScale = 0.15f;
    }
}
