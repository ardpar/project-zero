// Implements: ADR-002 — AttackConfig ScriptableObject (Tier 1)
// Design doc: auto-attack-system.md — Tuning Knobs section
//
// Global auto-attack system parameters (range, cone angle, slot cap).
// Per-slot parameters live in AttackSlotData assets.

using UnityEngine;
namespace Synthborn.Combat
{
    /// <summary>
    /// ScriptableObject holding global tuning parameters for the Auto-Attack system.
    /// Create via Assets > Create > Synthborn/Combat > Attack Config.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Combat/Attack Config", fileName = "AttackConfig")]
    public class AttackConfig : ScriptableObject
    {
        [Header("Targeting")]
        [Tooltip("Maximum target acquisition range in units. GDD default: 8.0. Safe range: 4.0-12.0.")]
        [Range(4f, 12f)]
        public float attackRange = 8.0f;
        [Tooltip("Half-angle of the targeting cone in degrees. GDD default: 60. Safe range: 30-90.")]
        [Range(30f, 90f)]
        public float targetingConeAngleDeg = 60f;
        [Header("Slots")]
        [Tooltip("Maximum number of simultaneous active attack slots. GDD default: 6. Safe range: 4-8.")]
        [Range(4, 8)]
        public int maxAttackSlots = 6;
        [Header("Speed Limits")]
        [Tooltip("Minimum attack interval regardless of modifiers. GDD default: 0.1 sn.")]
        [Range(0.05f, 0.2f)]
        public float minAttackInterval = 0.1f;
        [Tooltip("Maximum clamp for attack_speed_modifier. GDD default: 0.9.")]
        [Range(0.5f, 0.95f)]
        public float attackSpeedModifierMaxClamp = 0.9f;
        /// <summary>
        /// Computes effective attack interval:
        /// <c>effective_interval = max(base_interval * (1 - attack_speed_modifier), min_attack_interval)</c>
        /// </summary>
        public float GetEffectiveInterval(float baseInterval, float attackSpeedModifier)
        {
            float clampedMod = Mathf.Clamp(attackSpeedModifier, 0f, attackSpeedModifierMaxClamp);
            return Mathf.Max(baseInterval * (1f - clampedMod), minAttackInterval);
        }
    }
}
