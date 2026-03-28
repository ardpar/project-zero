// Implements: ADR-002 — PlayerConfig ScriptableObject (Tier 1)
// Design doc: player-controller.md — Tuning Knobs section
//
// All movement and dash parameters live here so designers can tune without
// touching code. Defaults match the GDD's specified values exactly.

using UnityEngine;

namespace Synthborn.Player.Data
{
    /// <summary>
    /// ScriptableObject holding all tunable parameters for the Player Controller.
    /// Create via Assets > Create > Synthborn > Player Config.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Player Config", fileName = "PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Base movement speed in units/second. GDD default: 5.0. Safe range: 3.0-8.0.")]
        [Range(3f, 8f)]
        public float baseMoveSpeed = 5.0f;

        [Tooltip("Minimum clamp for speed_modifier. GDD default: -0.5. Prevents full stop/reversal.")]
        [Range(-0.7f, -0.3f)]
        public float speedModifierMinClamp = -0.5f;

        [Tooltip("Maximum clamp for speed_modifier. GDD default: 2.0. Caps max speed at 3x base.")]
        [Range(1.5f, 3.0f)]
        public float speedModifierMaxClamp = 2.0f;

        [Header("Dash")]
        [Tooltip("Dash travel distance in units. GDD default: 3.0. Safe range: 2.0-5.0.")]
        [Range(2f, 5f)]
        public float dashDistance = 3.0f;

        [Tooltip("Time in seconds the dash takes to complete. GDD default: 0.15. Safe range: 0.1-0.25.")]
        [Range(0.1f, 0.25f)]
        public float dashDuration = 0.15f;

        [Tooltip("Base cooldown between dashes in seconds. GDD default: 3.0. Safe range: 2.0-5.0.")]
        [Range(2f, 5f)]
        public float baseDashCooldown = 3.0f;

        [Tooltip("Minimum effective dash cooldown after modifiers. GDD default: 0.5. Prevents dash spam.")]
        [Range(0.3f, 1.0f)]
        public float dashCooldownMinClamp = 0.5f;

        [Header("Collision")]
        [Tooltip("Player circle collider radius in units. GDD default: 0.3. Safe range: 0.2-0.5.")]
        [Range(0.2f, 0.5f)]
        public float colliderRadius = 0.3f;

        [Tooltip("Force applied when an enemy touches the player (knockback strength). GDD default: 0.5.")]
        [Range(0.2f, 1.0f)]
        public float enemyKnockbackForce = 0.5f;

        [Header("Physics Layers")]
        [Tooltip("Layer mask for solid arena boundaries the player collides with.")]
        public LayerMask solidCollisionMask;

        [Tooltip("Layer mask for enemies — used to temporarily ignore during dash.")]
        public LayerMask enemyLayerMask;

        /// <summary>
        /// Computes effective move speed using the formula from player-controller.md:
        /// <c>effective_speed = base_move_speed * (1 + speed_modifier)</c>
        /// speed_modifier is clamped to [speedModifierMinClamp, speedModifierMaxClamp].
        /// </summary>
        public float GetEffectiveMoveSpeed(float speedModifier)
        {
            float clampedModifier = Mathf.Clamp(speedModifier, speedModifierMinClamp, speedModifierMaxClamp);
            return baseMoveSpeed * (1f + clampedModifier);
        }

        /// <summary>
        /// Computes dash speed: <c>dash_speed = dash_distance / dash_duration</c>.
        /// </summary>
        public float GetDashSpeed() => dashDistance / dashDuration;

        /// <summary>
        /// Computes effective dash cooldown:
        /// <c>effective_dash_cd = base_dash_cooldown * (1 - dash_cd_modifier)</c>
        /// clamped to [dashCooldownMinClamp, infinity].
        /// </summary>
        public float GetEffectiveDashCooldown(float dashCdModifier)
        {
            float raw = baseDashCooldown * (1f - dashCdModifier);
            return Mathf.Max(raw, dashCooldownMinClamp);
        }
    }
}
