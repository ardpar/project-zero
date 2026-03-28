// Implements: ADR-002 — DamageInfo struct + DamageSource enum (Tier 0)
// Design docs: projectile-damage-system.md (final_damage formula, crit),
//              entity-health-system.md (TakeDamage interface, actual_damage)
//
// Passed through the entire damage pipeline:
//   Projectile/AutoAttack → DamageInfo → IDamageable.TakeDamage → EntityHealth

using UnityEngine;
namespace Synthborn.Core
{
    /// <summary>
    /// Identifies what caused the damage, used by EntityHealth for invulnerability
    /// source filtering and by VFX/audio to play the correct feedback.
    /// </summary>
    public enum DamageSource
    {
        PlayerProjectile,
        EnemyContact,
        EnemyProjectile,
        AoEExplosion,
        ArenaHazard
    }
    /// Immutable value struct carrying all context needed for a single damage event.
    /// Created by the Projectile/Damage system after crit resolution; consumed by
    /// EntityHealth which applies armor reduction and clamps the result.
    public readonly struct DamageInfo
        /// <summary>Pre-armor damage value (already includes damage_modifier and crit_multiplier).</summary>
        public readonly int RawDamage;
        /// <summary>
        /// Post-armor, post-clamp damage actually subtracted from HP.
        /// Set by EntityHealth.TakeDamage; zero until EntityHealth processes the hit.
        /// For the in-flight struct (before EntityHealth), this matches RawDamage.
        /// </summary>
        public readonly int FinalDamage;
        /// <summary>What system produced this damage event.</summary>
        public readonly DamageSource Source;
        /// <summary>True if the hit rolled a critical strike.</summary>
        public readonly bool IsCrit;
        /// <summary>World-space position where the hit occurred (for VFX/popup).</summary>
        public readonly Vector2 HitPosition;
        public DamageInfo(int rawDamage, DamageSource source, bool isCrit, Vector2 hitPosition)
        {
            RawDamage   = rawDamage;
            FinalDamage = rawDamage; // EntityHealth will override via WithFinalDamage()
            Source      = source;
            IsCrit      = isCrit;
            HitPosition = hitPosition;
        }
        /// Returns a copy of this DamageInfo with FinalDamage set to the post-armor value.
        /// Called by EntityHealth after applying armor reduction.
        public DamageInfo WithFinalDamage(int finalDamage) =>
            new DamageInfo(RawDamage, Source, IsCrit, HitPosition, finalDamage);
        // Private ctor used by WithFinalDamage
        private DamageInfo(int rawDamage, DamageSource source, bool isCrit, Vector2 hitPosition, int finalDamage)
            FinalDamage = finalDamage;
}
