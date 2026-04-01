using UnityEngine;
using Synthborn.Waves;

namespace Synthborn.Lore
{
    /// <summary>
    /// Database of all lore fragments. Referenced by LoreDropper and SignalArchiveScreen.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Lore/Lore Database", fileName = "LoreDatabase")]
    public class LoreDatabase : ScriptableObject
    {
        [Tooltip("All lore fragments in the game.")]
        public LoreFragment[] fragments;

        /// <summary>Find a fragment by its ID. Returns null if not found.</summary>
        public LoreFragment GetById(string id)
        {
            if (fragments == null) return null;
            foreach (var f in fragments)
                if (f != null && f.fragmentId == id) return f;
            return null;
        }

        /// <summary>Get all fragments for a specific biome.</summary>
        public LoreFragment[] GetByBiome(BiomeLayer biome)
        {
            var result = new System.Collections.Generic.List<LoreFragment>();
            if (fragments == null) return result.ToArray();
            foreach (var f in fragments)
                if (f != null && f.biome == biome) result.Add(f);
            return result.ToArray();
        }

        /// <summary>Get all fragments for a specific category.</summary>
        public LoreFragment[] GetByCategory(LoreCategory category)
        {
            var result = new System.Collections.Generic.List<LoreFragment>();
            if (fragments == null) return result.ToArray();
            foreach (var f in fragments)
                if (f != null && f.category == category) result.Add(f);
            return result.ToArray();
        }
    }
}
