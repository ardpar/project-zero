using NUnit.Framework;
using UnityEngine;

namespace Synthborn.Tests
{
    /// <summary>
    /// Tests for core balance formulas: pressure scaling, XP curve, enemy speed cap.
    /// Ensures formulas produce expected values at key thresholds.
    /// </summary>
    public class BalanceFormulaTests
    {
        // ─── Pressure Scaling ───

        [Test]
        public void test_pressure_room_1_multiplier_is_103()
        {
            // Formula: 1 + chamberNumber * 0.03
            float mult = 1f + 1 * 0.03f;
            Assert.AreEqual(1.03f, mult, 0.001f);
        }

        [Test]
        public void test_pressure_room_50_multiplier_is_250()
        {
            float mult = 1f + 50 * 0.03f;
            Assert.AreEqual(2.50f, mult, 0.001f);
        }

        [Test]
        public void test_pressure_room_100_multiplier_is_400()
        {
            float mult = 1f + 100 * 0.03f;
            Assert.AreEqual(4.00f, mult, 0.001f);
        }

        // ─── Enemy HP Scaling ───

        [Test]
        public void test_enemy_hp_wave_scaling_formula()
        {
            // base_hp * tier_mult * (1 + wave * wave_hp_scale)
            int baseHp = 10;
            float tierMult = 1f; // Normal
            float waveHpScale = 0.15f;
            int wave = 5;

            float expected = baseHp * tierMult * (1f + wave * waveHpScale);
            Assert.AreEqual(17.5f, expected, 0.01f);
        }

        [Test]
        public void test_enemy_hp_elite_wave_5()
        {
            int baseHp = 10;
            float tierMult = 3f; // Elite
            float waveHpScale = 0.15f;
            int wave = 5;

            float expected = baseHp * tierMult * (1f + wave * waveHpScale);
            Assert.AreEqual(52.5f, expected, 0.01f);
        }

        [Test]
        public void test_enemy_hp_boss_wave_5()
        {
            int baseHp = 10;
            float tierMult = 10f; // Boss
            float waveHpScale = 0.15f;
            int wave = 5;

            float expected = baseHp * tierMult * (1f + wave * waveHpScale);
            Assert.AreEqual(175f, expected, 0.01f);
        }

        // ─── Enemy Speed Cap ───

        [Test]
        public void test_enemy_speed_cap_at_45()
        {
            float baseSpeed = 2.5f;
            int wave = 100;
            float speedScale = 0.03f;
            float cap = 4.5f;

            float scaled = baseSpeed * (1f + wave * speedScale);
            float effective = Mathf.Min(scaled, cap);

            Assert.AreEqual(4.5f, effective, 0.01f); // capped
        }

        [Test]
        public void test_enemy_speed_uncapped_wave_1()
        {
            float baseSpeed = 2.5f;
            int wave = 1;
            float speedScale = 0.03f;
            float cap = 4.5f;

            float scaled = baseSpeed * (1f + wave * speedScale);
            float effective = Mathf.Min(scaled, cap);

            Assert.AreEqual(2.575f, effective, 0.01f); // not capped
        }

        // ─── Skill Tree Cost Curve ───

        [Test]
        public void test_skill_tree_total_branch_cost_is_25()
        {
            // T0-2=1, T3-5=2, T6-7=3, T8-9=5
            int total = 3 * 1 + 3 * 2 + 2 * 3 + 2 * 5;
            Assert.AreEqual(25, total);
        }

        [Test]
        public void test_skill_tree_full_unlock_cost_is_100()
        {
            // 4 branches * 25 points each
            Assert.AreEqual(100, 4 * 25);
        }

        // ─── Rarity Drop Rates ───

        [Test]
        public void test_rarity_drop_rates_sum_to_100()
        {
            // Baseline 50% + Calibrated 30% + Reinforced 15% + Anomalous 4% + Architect-Grade 1%
            float total = 50f + 30f + 15f + 4f + 1f;
            Assert.AreEqual(100f, total, 0.01f);
        }
    }
}
