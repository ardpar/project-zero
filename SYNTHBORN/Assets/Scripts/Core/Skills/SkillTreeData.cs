using UnityEngine;

namespace Synthborn.Core.Skills
{
    /// <summary>
    /// Full skill tree definition with all 4 branches.
    /// Must be in its own file matching class name for Unity SO serialization.
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
