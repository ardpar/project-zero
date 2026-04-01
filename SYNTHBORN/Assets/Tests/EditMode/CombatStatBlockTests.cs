using NUnit.Framework;
using Synthborn.Core.Stats;

namespace Synthborn.Tests
{
    /// <summary>
    /// Unit tests for CombatStatBlock — the central stat accumulator.
    /// Tests: apply/remove mutations, clamping, reset, edge cases.
    /// </summary>
    public class CombatStatBlockTests
    {
        private CombatStatBlock _block;

        [SetUp]
        public void SetUp()
        {
            _block = new CombatStatBlock();
        }

        // ─── Apply / Remove ───

        [Test]
        public void test_combatstatblock_apply_mutation_adds_damage_modifier()
        {
            // Arrange & Act
            _block.ApplyMutation(damageModifier: 0.5f);

            // Assert
            Assert.AreEqual(0.5f, _block.DamageModifier, 0.001f);
        }

        [Test]
        public void test_combatstatblock_apply_multiple_mutations_accumulates()
        {
            // Arrange & Act
            _block.ApplyMutation(damageModifier: 0.3f, critChance: 0.1f);
            _block.ApplyMutation(damageModifier: 0.2f, critChance: 0.05f);

            // Assert
            Assert.AreEqual(0.5f, _block.DamageModifier, 0.001f);
            Assert.AreEqual(0.15f, _block.CritChance, 0.001f);
        }

        [Test]
        public void test_combatstatblock_remove_mutation_subtracts_correctly()
        {
            // Arrange
            _block.ApplyMutation(speedModifier: 0.4f, armorFlat: 10);

            // Act
            _block.RemoveMutation(speedModifier: 0.4f, armorFlat: 10);

            // Assert
            Assert.AreEqual(0f, _block.SpeedModifier, 0.001f);
            Assert.AreEqual(0, _block.Armor);
        }

        [Test]
        public void test_combatstatblock_apply_all_stats_at_once()
        {
            // Arrange & Act
            _block.ApplyMutation(
                speedModifier: 0.1f,
                dashCdModifier: 0.2f,
                hpModifier: 0.3f,
                armorFlat: 5,
                damageModifier: 0.4f,
                critChance: 0.05f,
                critMultiplierBonus: 0.5f,
                attackSpeedModifier: 0.15f);

            // Assert
            Assert.AreEqual(0.1f, _block.SpeedModifier, 0.001f);
            Assert.AreEqual(0.2f, _block.DashCooldownModifier, 0.001f);
            Assert.AreEqual(0.3f, _block.HpModifier, 0.001f);
            Assert.AreEqual(5, _block.Armor);
            Assert.AreEqual(0.4f, _block.DamageModifier, 0.001f);
            Assert.AreEqual(0.05f, _block.CritChance, 0.001f);
            Assert.AreEqual(0.5f, _block.CritMultiplierBonus, 0.001f);
            Assert.AreEqual(0.15f, _block.AttackSpeedModifier, 0.001f);
        }

        // ─── Clamping ───

        [Test]
        public void test_combatstatblock_speed_modifier_clamped_at_upper_bound()
        {
            // Arrange & Act
            _block.ApplyMutation(speedModifier: 5.0f);

            // Assert — raw is 5.0, clamped is 2.0
            Assert.AreEqual(5.0f, _block.SpeedModifier, 0.001f);
            Assert.AreEqual(2.0f, _block.ClampedSpeedModifier, 0.001f);
        }

        [Test]
        public void test_combatstatblock_speed_modifier_clamped_at_lower_bound()
        {
            // Arrange & Act
            _block.ApplyMutation(speedModifier: -1.0f);

            // Assert — clamped to -0.5
            Assert.AreEqual(-0.5f, _block.ClampedSpeedModifier, 0.001f);
        }

        [Test]
        public void test_combatstatblock_crit_chance_clamped_at_max()
        {
            // Arrange & Act
            _block.ApplyMutation(critChance: 0.8f);

            // Assert — clamped to 0.5
            Assert.AreEqual(0.5f, _block.ClampedCritChance, 0.001f);
        }

        [Test]
        public void test_combatstatblock_damage_modifier_clamped_at_max()
        {
            // Arrange & Act
            _block.ApplyMutation(damageModifier: 5.0f);

            // Assert — clamped to 3.0
            Assert.AreEqual(3.0f, _block.ClampedDamageModifier, 0.001f);
        }

        [Test]
        public void test_combatstatblock_armor_clamped_between_0_and_50()
        {
            // Arrange & Act
            _block.ApplyMutation(armorFlat: 60);

            // Assert
            Assert.AreEqual(50, _block.ClampedArmor);
        }

        [Test]
        public void test_combatstatblock_hp_modifier_clamped_at_lower_bound()
        {
            // Arrange & Act
            _block.ApplyMutation(hpModifier: -0.8f);

            // Assert — clamped to -0.3
            Assert.AreEqual(-0.3f, _block.ClampedHpModifier, 0.001f);
        }

        [Test]
        public void test_combatstatblock_attack_speed_modifier_clamped_at_max()
        {
            // Arrange & Act
            _block.ApplyMutation(attackSpeedModifier: 1.5f);

            // Assert — clamped to 0.9
            Assert.AreEqual(0.9f, _block.ClampedAttackSpeedModifier, 0.001f);
        }

        [Test]
        public void test_combatstatblock_dash_cd_modifier_clamped_at_max()
        {
            // Arrange & Act
            _block.ApplyMutation(dashCdModifier: 0.9f);

            // Assert — clamped to 0.6
            Assert.AreEqual(0.6f, _block.ClampedDashCooldownModifier, 0.001f);
        }

        // ─── Reset ───

        [Test]
        public void test_combatstatblock_reset_clears_all_accumulators()
        {
            // Arrange
            _block.ApplyMutation(
                speedModifier: 0.5f,
                dashCdModifier: 0.3f,
                hpModifier: 0.2f,
                armorFlat: 10,
                damageModifier: 0.4f,
                critChance: 0.1f,
                critMultiplierBonus: 0.5f,
                attackSpeedModifier: 0.2f);

            // Act
            _block.Reset();

            // Assert
            Assert.AreEqual(0f, _block.SpeedModifier, 0.001f);
            Assert.AreEqual(0f, _block.DashCooldownModifier, 0.001f);
            Assert.AreEqual(0f, _block.HpModifier, 0.001f);
            Assert.AreEqual(0, _block.Armor);
            Assert.AreEqual(0f, _block.DamageModifier, 0.001f);
            Assert.AreEqual(0f, _block.CritChance, 0.001f);
            Assert.AreEqual(0f, _block.CritMultiplierBonus, 0.001f);
            Assert.AreEqual(0f, _block.AttackSpeedModifier, 0.001f);
        }

        // ─── Edge Cases ───

        [Test]
        public void test_combatstatblock_zero_mutation_no_change()
        {
            // Arrange & Act
            _block.ApplyMutation(); // all defaults = 0

            // Assert
            Assert.AreEqual(0f, _block.DamageModifier, 0.001f);
            Assert.AreEqual(0f, _block.SpeedModifier, 0.001f);
        }

        [Test]
        public void test_combatstatblock_negative_armor_clamped_to_zero()
        {
            // Arrange
            _block.ApplyMutation(armorFlat: 5);
            _block.RemoveMutation(armorFlat: 10);

            // Assert — raw is -5, clamped is 0
            Assert.AreEqual(-5, _block.Armor);
            Assert.AreEqual(0, _block.ClampedArmor);
        }

        [Test]
        public void test_combatstatblock_remove_more_than_applied_goes_negative()
        {
            // Arrange & Act
            _block.RemoveMutation(damageModifier: 0.3f);

            // Assert — raw is negative, clamped to 0
            Assert.AreEqual(-0.3f, _block.DamageModifier, 0.001f);
            Assert.AreEqual(0f, _block.ClampedDamageModifier, 0.001f);
        }
    }
}
