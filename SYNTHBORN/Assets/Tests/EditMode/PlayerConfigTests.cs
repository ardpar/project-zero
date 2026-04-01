using NUnit.Framework;
using UnityEngine;
using Synthborn.Player.Data;

namespace Synthborn.Tests
{
    /// <summary>
    /// Unit tests for PlayerConfig formulas — speed, dash speed, dash cooldown.
    /// </summary>
    public class PlayerConfigTests
    {
        private PlayerConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<PlayerConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        // ─── Effective Move Speed ───

        [Test]
        public void test_playerconfig_effective_speed_zero_modifier_returns_base()
        {
            // base = 5.0, modifier = 0 → 5.0 * (1 + 0) = 5.0
            Assert.AreEqual(5.0f, _config.GetEffectiveMoveSpeed(0f), 0.001f);
        }

        [Test]
        public void test_playerconfig_effective_speed_positive_modifier()
        {
            // modifier = 0.5 → 5.0 * 1.5 = 7.5
            Assert.AreEqual(7.5f, _config.GetEffectiveMoveSpeed(0.5f), 0.001f);
        }

        [Test]
        public void test_playerconfig_effective_speed_max_modifier_clamped()
        {
            // modifier = 3.0 → clamped to 2.0 → 5.0 * 3.0 = 15.0
            Assert.AreEqual(15.0f, _config.GetEffectiveMoveSpeed(3.0f), 0.001f);
        }

        [Test]
        public void test_playerconfig_effective_speed_negative_modifier_clamped()
        {
            // modifier = -1.0 → clamped to -0.5 → 5.0 * 0.5 = 2.5
            Assert.AreEqual(2.5f, _config.GetEffectiveMoveSpeed(-1.0f), 0.001f);
        }

        // ─── Dash Speed ───

        [Test]
        public void test_playerconfig_dash_speed_formula()
        {
            // distance = 3.0, duration = 0.15 → 3.0 / 0.15 = 20.0
            Assert.AreEqual(20.0f, _config.GetDashSpeed(), 0.001f);
        }

        // ─── Effective Dash Cooldown ───

        [Test]
        public void test_playerconfig_dash_cooldown_zero_modifier()
        {
            // base = 3.0, modifier = 0 → 3.0 * 1.0 = 3.0
            Assert.AreEqual(3.0f, _config.GetEffectiveDashCooldown(0f), 0.001f);
        }

        [Test]
        public void test_playerconfig_dash_cooldown_positive_modifier_reduces()
        {
            // modifier = 0.5 → 3.0 * 0.5 = 1.5
            Assert.AreEqual(1.5f, _config.GetEffectiveDashCooldown(0.5f), 0.001f);
        }

        [Test]
        public void test_playerconfig_dash_cooldown_clamped_at_minimum()
        {
            // modifier = 0.95 → 3.0 * 0.05 = 0.15, clamped to 0.5
            Assert.AreEqual(0.5f, _config.GetEffectiveDashCooldown(0.95f), 0.001f);
        }
    }
}
