// Implements: ADR-002 — ProjectileData ScriptableObject (Tier 1)
// Design doc: projectile-damage-system.md — Core Rules + Tuning Knobs
//
// One asset per projectile "template". Mutasyon Sistemi can reference different
// assets to change projectile behaviour without code changes.

using UnityEngine;

namespace Synthborn.Combat.Projectile
{
    /// <summary>Which behaviour triggers when the projectile hits a valid target.</summary>
    public enum OnHitBehaviorType
    {
        Destroy,
        Pierce,
        AoE,
        Chain
    }

    /// <summary>
    /// ScriptableObject describing a projectile template.
    /// Shared across all live instances of the same projectile type.
    /// Create via Assets > Create > Synthborn/Combat > Projectile Data.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Combat/Projectile Data", fileName = "ProjectileData")]
    public class ProjectileData : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Travel speed in units/second. GDD default: 10.0. Safe range: 5.0-20.0.")]
        [Range(1f, 30f)]
        public float projectileSpeed = 10.0f;

        [Tooltip("Maximum travel time in seconds before auto-expire. GDD default: 3.0.")]
        [Range(0.5f, 10f)]
        public float maxLifetime = 3.0f;

        [Tooltip("Maximum travel distance in units before auto-expire. GDD default: 12.0.")]
        [Range(1f, 30f)]
        public float maxRange = 12.0f;

        [Header("Damage")]
        [Tooltip("Base damage before modifiers and crit. GDD default: 10. Safe range: 5-25.")]
        [Min(1)]
        public int baseDamage = 10;

        [Tooltip("Base critical hit chance (0-1). GDD default: 0.05.")]
        [Range(0f, 0.5f)]
        public float baseCritChance = 0.05f;

        [Tooltip("Critical hit damage multiplier. GDD default: 2.0. Safe range: 1.5-4.0.")]
        [Range(1.0f, 4.0f)]
        public float baseCritMultiplier = 2.0f;

        [Header("On-Hit Behaviour")]
        public OnHitBehaviorType onHitBehavior = OnHitBehaviorType.Destroy;

        [Header("Pierce (OnHitBehavior = Pierce)")]
        [Tooltip("Number of additional targets the projectile passes through. GDD default: 0.")]
        [Range(0, 10)]
        public int pierceCount = 0;

        [Tooltip("Damage multiplier reduction per pierce target. GDD default: 0.15. Safe range: 0.0-0.3.")]
        [Range(0f, 0.3f)]
        public float pierceDecay = 0.15f;

        [Tooltip("Minimum seconds before the same pierce target can be hit again.")]
        [Range(0.05f, 1f)]
        public float pierceSameTargetCooldown = 0.1f;

        [Header("AoE (OnHitBehavior = AoE)")]
        [Tooltip("Explosion radius in units. Safe range: 1.0-5.0.")]
        [Range(0.5f, 6f)]
        public float aoeRadius = 2.0f;

        [Tooltip("0 = full damage at edge; 1 = zero damage at edge (linear falloff).")]
        [Range(0f, 1f)]
        public float aoeFalloff = 0.5f;

        [Header("Chain (OnHitBehavior = Chain)")]
        [Tooltip("Number of times the projectile chains to a new target.")]
        [Range(0, 6)]
        public int chainCount = 2;

        [Tooltip("Max distance to search for the next chain target in units.")]
        [Range(1f, 10f)]
        public float chainSearchRadius = 5.0f;

        [Header("Homing")]
        [Tooltip("Enable homing behaviour (some mutations). 0 = no homing.")]
        [Range(0f, 360f)]
        public float homingStrengthDegPerSec = 0f;

        [Header("Visual")]
        [Tooltip("Prefab for the projectile's visual representation (sprite, trail, etc.).")]
        public GameObject visualPrefab;
    }
}
