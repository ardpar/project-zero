using NUnit.Framework;
using UnityEngine;
using Synthborn.Core.Stats;
using Synthborn.Progression;

namespace Synthborn.Tests
{
    /// <summary>
    /// Unit tests for AdaptationConfig — per-point stat gain calculations.
    /// </summary>
    public class AdaptationPointTests
    {
        private AdaptationConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<AdaptationConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void test_adaptation_default_mass_per_point_is_005()
        {
            Assert.AreEqual(0.05f, _config.massPerPoint, 0.001f);
        }

        [Test]
        public void test_adaptation_default_resilience_per_point_is_006()
        {
            Assert.AreEqual(0.06f, _config.resiliencePerPoint, 0.001f);
        }

        [Test]
        public void test_adaptation_default_velocity_per_point_is_004()
        {
            Assert.AreEqual(0.04f, _config.velocityPerPoint, 0.001f);
        }

        [Test]
        public void test_adaptation_default_variance_per_point_is_002()
        {
            Assert.AreEqual(0.02f, _config.variancePerPoint, 0.001f);
        }

        [Test]
        public void test_adaptation_default_yield_per_point_is_008()
        {
            Assert.AreEqual(0.08f, _config.yieldPerPoint, 0.001f);
        }

        [Test]
        public void test_adaptation_default_points_per_level_is_1()
        {
            Assert.AreEqual(1, _config.pointsPerLevel);
        }

        [Test]
        public void test_adaptation_mass_5_points_gives_25_percent_damage()
        {
            // 5 points * 0.05 = 0.25 (25% damage boost)
            float expected = 5 * _config.massPerPoint;
            Assert.AreEqual(0.25f, expected, 0.001f);
        }

        [Test]
        public void test_adaptation_resilience_10_points_gives_60_percent_hp()
        {
            float expected = 10 * _config.resiliencePerPoint;
            Assert.AreEqual(0.60f, expected, 0.001f);
        }
    }
}
