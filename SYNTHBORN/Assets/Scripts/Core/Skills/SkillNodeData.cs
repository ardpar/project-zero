using System;
using UnityEngine;

namespace Synthborn.Core.Skills
{
    public enum SkillBranch { Might, Vitality, Agility, Fortune }

    /// <summary>
    /// Single node in the passive skill tree.
    /// </summary>
    [Serializable]
    public class SkillNodeData
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public SkillBranch branch;
        public int tier; // 0-9, determines position in branch

        [Header("Cost")]
        public int skillPointCost = 1;
        public string prerequisiteNodeId; // empty = no prerequisite

        [Header("Stat Bonuses")]
        public float hpModifier;
        public float damageModifier;
        public float speedModifier;
        public float critChance;
        public float critDamageBonus;
        public float attackSpeedModifier;
        public int armorFlat;
        public float dashCooldownModifier;
        public float xpGainBonus;
        public float goldGainBonus;
        public float dropRateBonus;
    }

    /// <summary>
    /// Full skill tree definition with all 4 branches.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Skills/SkillTreeData")]
    public class SkillTreeData : ScriptableObject
    {
        public SkillNodeData[] nodes;

        /// <summary>Find a node by ID.</summary>
        public SkillNodeData GetNode(string id)
        {
            if (nodes == null) return null;
            foreach (var n in nodes)
                if (n.id == id) return n;
            return null;
        }

        /// <summary>Get all nodes in a branch, sorted by tier.</summary>
        public SkillNodeData[] GetBranch(SkillBranch branch)
        {
            var list = new System.Collections.Generic.List<SkillNodeData>();
            if (nodes == null) return list.ToArray();
            foreach (var n in nodes)
                if (n.branch == branch) list.Add(n);
            list.Sort((a, b) => a.tier.CompareTo(b.tier));
            return list.ToArray();
        }
    }
}
