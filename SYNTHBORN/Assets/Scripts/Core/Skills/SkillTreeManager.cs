using UnityEngine;
using Synthborn.Core.Persistence;

namespace Synthborn.Core.Skills
{
    /// <summary>
    /// Manages skill tree progression: unlock nodes, spend points, apply stats.
    /// </summary>
    public static class SkillTreeManager
    {
        private static SkillTreeData _treeData;

        public static void SetTreeData(SkillTreeData data) => _treeData = data;

        /// <summary>Check if a node is unlocked.</summary>
        public static bool IsUnlocked(string nodeId)
        {
            var ch = SaveManager.Character;
            return ch != null && ch.unlockedSkillNodes.Contains(nodeId);
        }

        /// <summary>Check if a node can be unlocked (has points, prerequisite met).</summary>
        public static bool CanUnlock(string nodeId)
        {
            var ch = SaveManager.Character;
            if (ch == null || _treeData == null) return false;
            if (IsUnlocked(nodeId)) return false;

            var node = _treeData.GetNode(nodeId);
            if (node == null) return false;
            if (ch.unspentStatPoints < node.skillPointCost) return false;

            // Check prerequisite
            if (!string.IsNullOrEmpty(node.prerequisiteNodeId))
                if (!IsUnlocked(node.prerequisiteNodeId)) return false;

            return true;
        }

        /// <summary>Unlock a skill node. Returns false if can't.</summary>
        public static bool TryUnlock(string nodeId)
        {
            if (!CanUnlock(nodeId)) return false;

            var ch = SaveManager.Character;
            var node = _treeData.GetNode(nodeId);

            ch.unspentStatPoints -= node.skillPointCost;
            ch.unlockedSkillNodes.Add(nodeId);
            SaveManager.SaveSlot();
            return true;
        }

        /// <summary>Reset all skills, refund points. Costs gold.</summary>
        public static bool TryReset(int goldCost)
        {
            var ch = SaveManager.Character;
            if (ch == null || _treeData == null) return false;
            if (ch.gold < goldCost) return false;

            // Count refunded points
            int refund = 0;
            foreach (var nodeId in ch.unlockedSkillNodes)
            {
                var node = _treeData.GetNode(nodeId);
                if (node != null) refund += node.skillPointCost;
            }

            ch.gold -= goldCost;
            ch.unspentStatPoints += refund;
            ch.unlockedSkillNodes.Clear();
            SaveManager.SaveSlot();
            return true;
        }

        /// <summary>Apply all unlocked skill bonuses to a CombatStatBlock.</summary>
        public static void ApplyToStats(Stats.CombatStatBlock stats)
        {
            var ch = SaveManager.Character;
            if (ch == null || _treeData == null) return;

            foreach (var nodeId in ch.unlockedSkillNodes)
            {
                var node = _treeData.GetNode(nodeId);
                if (node == null) continue;

                stats.ApplyMutation(
                    hpModifier: node.hpModifier,
                    damageModifier: node.damageModifier,
                    speedModifier: node.speedModifier,
                    critChance: node.critChance,
                    critMultiplierBonus: node.critDamageBonus,
                    attackSpeedModifier: node.attackSpeedModifier,
                    armorFlat: node.armorFlat,
                    dashCdModifier: node.dashCooldownModifier
                );
            }
        }

        /// <summary>Total unlocked nodes count.</summary>
        public static int UnlockedCount
        {
            get
            {
                var ch = SaveManager.Character;
                return ch?.unlockedSkillNodes.Count ?? 0;
            }
        }

        /// <summary>Total nodes in tree.</summary>
        public static int TotalNodes => _treeData?.nodes?.Length ?? 0;
    }
}
