namespace Synthborn.Combat
{
    /// <summary>Pierce through multiple enemies with damage decay.</summary>
    public class PierceOnHit : IHitBehavior
    {
        private int _remainingPierces;
        private readonly float _decayPerPierce;

        public PierceOnHit(int pierceCount, float decayPerPierce)
        {
            _remainingPierces = pierceCount;
            _decayPerPierce = decayPerPierce;
        }
        public bool OnHit(ProjectileController projectile, IDamageable target, DamageInfo damage)
            _remainingPierces--;
            if (_remainingPierces <= 0)
                return true; // no more pierces, return to pool
            // Reduce damage for next hit
            projectile.ApplyDamageDecay(_decayPerPierce);
            return false; // keep flying
    }
}
