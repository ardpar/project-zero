using UnityEngine;

namespace Synthborn.Core
{
    public enum DamageSource
    {
        PlayerProjectile,
        EnemyContact,
        EnemyProjectile,
        AoEExplosion,
        ArenaHazard
    }

    /// <summary>Immutable damage event data passed through the damage pipeline.</summary>
    public readonly struct DamageInfo
    {
        public readonly int RawDamage;
        public readonly int FinalDamage;
        public readonly DamageSource Source;
        public readonly bool IsCrit;
        public readonly Vector2 HitPosition;

        public DamageInfo(int rawDamage, DamageSource source, bool isCrit, Vector2 hitPosition)
        {
            RawDamage = rawDamage;
            FinalDamage = rawDamage;
            Source = source;
            IsCrit = isCrit;
            HitPosition = hitPosition;
        }

        /// <summary>Returns a copy with FinalDamage set to post-armor value.</summary>
        public DamageInfo WithFinalDamage(int finalDamage)
        {
            return new DamageInfo(RawDamage, Source, IsCrit, HitPosition, finalDamage);
        }

        private DamageInfo(int rawDamage, DamageSource source, bool isCrit, Vector2 hitPosition, int finalDamage)
        {
            RawDamage = rawDamage;
            FinalDamage = finalDamage;
            Source = source;
            IsCrit = isCrit;
            HitPosition = hitPosition;
        }
    }
}
