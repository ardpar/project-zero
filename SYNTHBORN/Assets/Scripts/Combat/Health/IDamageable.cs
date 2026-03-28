// Implements: ADR-002 — IDamageable interface (Tier 2)
// Design doc: entity-health-system.md — Interface principle

using Synthborn.Core;
namespace Synthborn.Combat
{
    /// <summary>
    /// Implemented by any entity that can receive damage.
    /// Projectiles and contact damage call <see cref="TakeDamage"/> directly;
    /// they do not reference EntityHealth specifically.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Apply damage to this entity.
        /// Armor reduction, invulnerability checks, and death resolution happen inside.
        /// </summary>
        void TakeDamage(DamageInfo info);
        /// <summary>True once HP has reached zero and the death sequence has started.</summary>
        bool IsDead { get; }
    }
}
