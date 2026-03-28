// Implements: ADR-002 — AttackSlotState runtime struct (Tier 1)
// Design doc: auto-attack-system.md — Core Rules 19-20 (independent cooldowns)
//
// Owned by AutoAttackController as List<AttackSlotState>.
// Kept in sync with the slot data list; one state per active slot.

using UnityEngine;
namespace Synthborn.Combat
{
    /// <summary>
    /// Mutable runtime state for one attack slot.
    /// Kept as a struct in a List for tight memory layout.
    /// </summary>
    public struct AttackSlotState
    {
        /// <summary>Reference to the slot's static definition.</summary>
        public AttackSlotData Data;
        /// <summary>
        /// Seconds remaining until this slot can fire again.
        /// Counts down each frame. Fires when &lt;= 0.
        /// </summary>
        public float CooldownRemaining;
        /// <summary>True while the game is paused — cooldown does not tick.</summary>
        public bool IsPaused;
        public AttackSlotState(AttackSlotData data)
        {
            Data               = data;
            CooldownRemaining  = 0f; // Ready to fire immediately on spawn
            IsPaused           = false;
        }
        /// <summary>Ticks the cooldown down. Safe to call every FixedUpdate frame.</summary>
        /// <param name="deltaTime">Frame delta (use Time.deltaTime or Time.fixedDeltaTime).</param>
        /// <returns>True if the cooldown just reached zero this tick (ready to fire).</returns>
        public bool Tick(float deltaTime)
            if (IsPaused || CooldownRemaining <= 0f) return CooldownRemaining <= 0f;
            CooldownRemaining -= deltaTime;
            return CooldownRemaining <= 0f;
        /// <summary>Resets the cooldown to the effective interval after firing.</summary>
        public void ResetCooldown(float effectiveInterval)
            CooldownRemaining = effectiveInterval;
    }
}
