using NUnit.Framework;
using UnityEngine;
using Synthborn.Enemies;

namespace Synthborn.Tests
{
    /// <summary>
    /// Unit tests for EnemyScalingConfig — tier multiplier lookups.
    /// Tests: HP multiplier, XP multiplier, default values.
    /// </summary>
    public class EnemyScalingConfigTests
    {
        private EnemyScalingConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<EnemyScalingConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        // ─── HP Multiplier ───

        [Test]
        public void test_scaling_hp_multiplier_normal_returns_1()
        {
            Assert.AreEqual(1f, _config.GetHpMultiplier(EnemyTier.Normal), 0.001f);
        }

        [Test]
        public void test_scaling_hp_multiplier_elite_returns_3()
        {
            Assert.AreEqual(3f, _config.GetHpMultiplier(EnemyTier.Elite), 0.001f);
        }

        [Test]
        public void test_scaling_hp_multiplier_boss_returns_10()
        {
            Assert.AreEqual(10f, _config.GetHpMultiplier(EnemyTier.Boss), 0.001f);
        }

        // ─── XP Multiplier ───

        [Test]
        public void test_scaling_xp_multiplier_normal_returns_1()
        {
            Assert.AreEqual(1f, _config.GetXpMultiplier(EnemyTier.Normal), 0.001f);
        }

        [Test]
        public void test_scaling_xp_multiplier_elite_returns_5()
        {
            Assert.AreEqual(5f, _config.GetXpMultiplier(EnemyTier.Elite), 0.001f);
        }

        [Test]
        public void test_scaling_xp_multiplier_boss_returns_20()
        {
            Assert.AreEqual(20f, _config.GetXpMultiplier(EnemyTier.Boss), 0.001f);
        }

        // ─── Default Values ───

        [Test]
        public void test_scaling_default_wave_hp_scale_is_015()
        {
            Assert.AreEqual(0.15f, _config.WaveHpScale, 0.001f);
        }

        [Test]
        public void test_scaling_default_speed_cap_fraction_is_09()
        {
            Assert.AreEqual(0.9f, _config.SpeedCapFraction, 0.001f);
        }

        [Test]
        public void test_scaling_default_contact_damage_interval_is_05()
        {
            Assert.AreEqual(0.5f, _config.ContactDamageInterval, 0.001f);
        }

        [Test]
        public void test_scaling_default_absolute_speed_cap_is_45()
        {
            Assert.AreEqual(4.5f, _config.AbsoluteSpeedCap, 0.001f);
        }
    }
}
