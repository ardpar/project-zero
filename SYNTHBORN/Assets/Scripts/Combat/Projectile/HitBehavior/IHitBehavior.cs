// Implements: ADR-002 — IHitBehavior strategy interface (Tier 4)
// Design doc: projectile-damage-system.md — on_hit_behavior rules 8-9

using Synthborn.Core;
namespace Synthborn.Combat
{
    /// <summary>
    /// Strategy executed when a <see cref="ProjectileController"/> collides with a valid target.
    /// Each concrete behaviour (Destroy, Pierce, AoE, Chain) implements this interface.
    /// </summary>
    public interface IHitBehavior
    {
        /// <summary>
        /// Called immediately after the projectile resolves its hit.
        /// </summary>
        /// <param name="projectile">The projectile that hit.</param>
        /// <param name="target">The entity that was hit (already implements IDamageable).</param>
        /// <param name="baseDamage">Pre-computed DamageInfo for this hit (RawDamage set).</param>
        /// <returns>
        /// True  → projectile should be returned to the pool immediately after this call.
        /// False → projectile continues (pierce still has charges, chain still executing).
        /// </returns>
        bool OnHit(ProjectileController projectile, IDamageable target, DamageInfo baseDamage);
    }
}
