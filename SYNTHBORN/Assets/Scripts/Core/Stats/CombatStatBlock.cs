// Implements: ADR-002 — CombatStatBlock (Tier 0)
// Design docs: entity-health-system.md (armor, hp_modifier),
//              projectile-damage-system.md (damage_modifier, crit),
//              player-controller.md (speed_modifier, dash_cd_modifier),
//              auto-attack-system.md (attack_speed_modifier)
//
// Plain C# class (not MonoBehaviour). Holds all additive stat accumulators
// that mutations feed into. Systems read final values via computed properties.
// Mutations call ApplyMutation() / RemoveMutation() at grant/revoke time.
// Reset() restores everything to base defaults for run start.

using System;
using UnityEngine;

namespace Synthborn.Core.Stats
{
    /// <summary>
    /// Aggregates all runtime stat modifications applied by the Mutation System.
    /// All modifiers are additive accumulators; computed properties apply clamping
    /// and return the final effective value ready for use.
    /// </summary>
    public class CombatStatBlock
    {
        // ─────────────────────────────────────────────
        // Raw accumulators — mutated by IAttackModifier / mutation grants
        // ─────────────────────────────────────────────

        /// <summary>Additive speed modifier sum. Range after clamp: -0.5 to 2.0.</summary>
        public float SpeedModifier { get; private set; }

        /// <summary>Additive dash cooldown reduction sum. Range: 0.0 to 0.6.</summary>
        public float DashCooldownModifier { get; private set; }

        /// <summary>Additive HP modifier sum. Range after clamp: -0.3 to 2.0.</summary>
        public float HpModifier { get; private set; }

        /// <summary>Flat armor accumulator. Range: 0 to 50.</summary>
        public int Armor { get; private set; }

        /// <summary>Additive damage modifier sum. Range: 0.0 to 3.0.</summary>
        public float DamageModifier { get; private set; }

        /// <summary>Additive crit chance accumulator. Range: 0.0 to 0.5 (clamped in property).</summary>
        public float CritChance { get; private set; }

        /// <summary>Additive crit multiplier delta accumulator (added to base 2.0). Range: 0 to +2.0.</summary>
        public float CritMultiplierBonus { get; private set; }

        /// <summary>Additive attack speed modifier sum. Range: 0.0 to 0.9.</summary>
        public float AttackSpeedModifier { get; private set; }

        // ─────────────────────────────────────────────
        // Clamped computed properties
        // ─────────────────────────────────────────────

        // GDD player-controller.md — speed_modifier clamped to [-0.5, 2.0]
        public float ClampedSpeedModifier =>
            Mathf.Clamp(SpeedModifier, -0.5f, 2.0f);

        // GDD player-controller.md — dash_cd_modifier clamped to [0.0, 0.6]
        public float ClampedDashCooldownModifier =>
            Mathf.Clamp(DashCooldownModifier, 0f, 0.6f);

        // GDD entity-health-system.md — hp_modifier clamped to [-0.3, 2.0]
        public float ClampedHpModifier =>
            Mathf.Clamp(HpModifier, -0.3f, 2.0f);

        // GDD entity-health-system.md — armor clamped to [0, 50]
        public int ClampedArmor =>
            Mathf.Clamp(Armor, 0, 50);

        // GDD projectile-damage-system.md — damage_modifier clamped to [0, 3]
        public float ClampedDamageModifier =>
            Mathf.Clamp(DamageModifier, 0f, 3.0f);

        // GDD projectile-damage-system.md — crit_chance clamped to [0, 0.5]
        public float ClampedCritChance =>
            Mathf.Clamp(CritChance, 0f, 0.5f);

        // GDD auto-attack-system.md — attack_speed_modifier clamped to [0, 0.9]
        public float ClampedAttackSpeedModifier =>
            Mathf.Clamp(AttackSpeedModifier, 0f, 0.9f);

        // ─────────────────────────────────────────────
        // Mutation API
        // ─────────────────────────────────────────────

        /// <summary>
        /// Adds one mutation's stat contribution. Typically called when a mutation is granted.
        /// All parameters are optional — pass only the stats the mutation affects.
        /// </summary>
        public void ApplyMutation(
            float speedModifier        = 0f,
            float dashCdModifier       = 0f,
            float hpModifier           = 0f,
            int   armorFlat            = 0,
            float damageModifier       = 0f,
            float critChance           = 0f,
            float critMultiplierBonus  = 0f,
            float attackSpeedModifier  = 0f)
        {
            SpeedModifier         += speedModifier;
            DashCooldownModifier  += dashCdModifier;
            HpModifier            += hpModifier;
            Armor                 += armorFlat;
            DamageModifier        += damageModifier;
            CritChance            += critChance;
            CritMultiplierBonus   += critMultiplierBonus;
            AttackSpeedModifier   += attackSpeedModifier;
        }

        /// <summary>
        /// Removes one mutation's stat contribution. Pass the same values used in ApplyMutation().
        /// </summary>
        public void RemoveMutation(
            float speedModifier        = 0f,
            float dashCdModifier       = 0f,
            float hpModifier           = 0f,
            int   armorFlat            = 0,
            float damageModifier       = 0f,
            float critChance           = 0f,
            float critMultiplierBonus  = 0f,
            float attackSpeedModifier  = 0f)
        {
            SpeedModifier         -= speedModifier;
            DashCooldownModifier  -= dashCdModifier;
            HpModifier            -= hpModifier;
            Armor                 -= armorFlat;
            DamageModifier        -= damageModifier;
            CritChance            -= critChance;
            CritMultiplierBonus   -= critMultiplierBonus;
            AttackSpeedModifier   -= attackSpeedModifier;
        }

        /// <summary>
        /// Resets all accumulators to zero. Call on run start / run reset.
        /// </summary>
        public void Reset()
        {
            SpeedModifier        = 0f;
            DashCooldownModifier = 0f;
            HpModifier           = 0f;
            Armor                = 0;
            DamageModifier       = 0f;
            CritChance           = 0f;
            CritMultiplierBonus  = 0f;
            AttackSpeedModifier  = 0f;
        }
    }
}
