using UnityEngine;
using Synthborn.Core.Data;
using Synthborn.Combat.Projectile.HitBehavior;
using Synthborn.Combat.Health;

namespace Synthborn.Combat
{
    /// <summary>Default hit behavior: destroy projectile on first hit.</summary>
    public class DestroyOnHit : IHitBehavior
    {
        public bool OnHit(ProjectileController projectile, IDamageable target, DamageInfo damage)
        {
            return true; // true = return to pool
        }
    }
}
