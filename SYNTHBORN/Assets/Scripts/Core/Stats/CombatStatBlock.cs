using UnityEngine;

namespace Synthborn.Core
{
    /// <summary>
    /// Aggregates all runtime stat modifications from mutations.
    /// All modifiers are additive. Clamped properties return final values.
    /// </summary>
    public class CombatStatBlock
    {
        public float SpeedModifier { get; private set; }
        public float DashCooldownModifier { get; private set; }
        public float HpModifier { get; private set; }
        public int Armor { get; private set; }
        public float DamageModifier { get; private set; }
        public float CritChance { get; private set; }
        public float CritMultiplierBonus { get; private set; }
        public float AttackSpeedModifier { get; private set; }
        public float XPGainModifier { get; private set; }
        public float MagnetRadiusModifier { get; private set; }
        public float CritMultiplier => 2.0f + CritMultiplierBonus;

        // Clamped properties (ranges from GDDs)
        public float ClampedSpeedModifier => Mathf.Clamp(SpeedModifier, -0.5f, 2.0f);
        public float ClampedDashCooldownModifier => Mathf.Clamp(DashCooldownModifier, 0f, 0.6f);
        public float ClampedHpModifier => Mathf.Clamp(HpModifier, -0.3f, 2.0f);
        public int ClampedArmor => Mathf.Clamp(Armor, 0, 50);
        public float ClampedDamageModifier => Mathf.Clamp(DamageModifier, 0f, 3.0f);
        public float ClampedCritChance => Mathf.Clamp(CritChance, 0f, 0.5f);
        public float ClampedAttackSpeedModifier => Mathf.Clamp(AttackSpeedModifier, 0f, 0.9f);

        /// <summary>Add a mutation's stat contribution.</summary>
        public void ApplyMutation(
            float speedModifier = 0f,
            float dashCdModifier = 0f,
            float hpModifier = 0f,
            int armorFlat = 0,
            float damageModifier = 0f,
            float critChance = 0f,
            float critMultiplierBonus = 0f,
            float attackSpeedModifier = 0f,
            float xpGainModifier = 0f,
            float magnetRadiusModifier = 0f)
        {
            SpeedModifier += speedModifier;
            DashCooldownModifier += dashCdModifier;
            HpModifier += hpModifier;
            Armor += armorFlat;
            DamageModifier += damageModifier;
            CritChance += critChance;
            CritMultiplierBonus += critMultiplierBonus;
            AttackSpeedModifier += attackSpeedModifier;
            XPGainModifier += xpGainModifier;
            MagnetRadiusModifier += magnetRadiusModifier;
        }

        /// <summary>Remove a mutation's stat contribution.</summary>
        public void RemoveMutation(
            float speedModifier = 0f,
            float dashCdModifier = 0f,
            float hpModifier = 0f,
            int armorFlat = 0,
            float damageModifier = 0f,
            float critChance = 0f,
            float critMultiplierBonus = 0f,
            float attackSpeedModifier = 0f,
            float xpGainModifier = 0f,
            float magnetRadiusModifier = 0f)
        {
            SpeedModifier -= speedModifier;
            DashCooldownModifier -= dashCdModifier;
            HpModifier -= hpModifier;
            Armor -= armorFlat;
            DamageModifier -= damageModifier;
            CritChance -= critChance;
            CritMultiplierBonus -= critMultiplierBonus;
            AttackSpeedModifier -= attackSpeedModifier;
            XPGainModifier -= xpGainModifier;
            MagnetRadiusModifier -= magnetRadiusModifier;
        }

        /// <summary>Reset all accumulators to zero (run start).</summary>
        public void Reset()
        {
            SpeedModifier = 0f;
            DashCooldownModifier = 0f;
            HpModifier = 0f;
            Armor = 0;
            DamageModifier = 0f;
            CritChance = 0f;
            CritMultiplierBonus = 0f;
            AttackSpeedModifier = 0f;
            XPGainModifier = 0f;
            MagnetRadiusModifier = 0f;
        }
    }
}
