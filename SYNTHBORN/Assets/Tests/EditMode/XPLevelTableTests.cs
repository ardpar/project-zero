using NUnit.Framework;
using UnityEngine;
using Synthborn.Progression;

namespace Synthborn.Tests
{
    /// <summary>
    /// Unit tests for XPLevelTable — the XP progression curve.
    /// Tests: table lookups, beyond-table exponential growth, edge cases.
    /// </summary>
    public class XPLevelTableTests
    {
        private XPLevelTable _table;

        [SetUp]
        public void SetUp()
        {
            _table = ScriptableObject.CreateInstance<XPLevelTable>();
            _table.xpTable = new[] { 20, 25, 30, 35, 40, 50, 60, 70, 80, 100 };
            _table.growthRate = 1.25f;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_table);
        }

        // ─── Table Lookups ───

        [Test]
        public void test_xpleveltable_level_1_returns_first_entry()
        {
            // Assert
            Assert.AreEqual(20, _table.GetXPForLevel(1));
        }

        [Test]
        public void test_xpleveltable_level_5_returns_correct_entry()
        {
            // Assert (index 4 = 40)
            Assert.AreEqual(40, _table.GetXPForLevel(5));
        }

        [Test]
        public void test_xpleveltable_last_level_returns_last_entry()
        {
            // Assert (index 9 = 100)
            Assert.AreEqual(100, _table.GetXPForLevel(10));
        }

        // ─── Beyond Table (Exponential Growth) ───

        [Test]
        public void test_xpleveltable_beyond_table_uses_exponential_growth()
        {
            // Level 11 = first beyond table
            // Expected: 100 * 1.25^1 = 125
            int expected = Mathf.RoundToInt(100 * Mathf.Pow(1.25f, 1));
            Assert.AreEqual(expected, _table.GetXPForLevel(11));
        }

        [Test]
        public void test_xpleveltable_level_12_exponential_growth()
        {
            // Level 12 = 100 * 1.25^2 = 156
            int expected = Mathf.RoundToInt(100 * Mathf.Pow(1.25f, 2));
            Assert.AreEqual(expected, _table.GetXPForLevel(12));
        }

        // ─── Edge Cases ───

        [Test]
        public void test_xpleveltable_level_0_clamped_to_level_1()
        {
            // Level 0 should be treated as level 1
            Assert.AreEqual(20, _table.GetXPForLevel(0));
        }

        [Test]
        public void test_xpleveltable_negative_level_clamped_to_level_1()
        {
            Assert.AreEqual(20, _table.GetXPForLevel(-5));
        }
    }
}
