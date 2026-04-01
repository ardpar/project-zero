using UnityEngine;
using Synthborn.Waves;

namespace Synthborn.Lore
{
    /// <summary>
    /// ScriptableObject defining a single lore fragment (Signal Archive entry).
    /// Dropped by enemies/bosses, collected by player, readable in SignalArchiveScreen.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Lore/Lore Fragment", fileName = "LoreFragment")]
    public class LoreFragment : ScriptableObject
    {
        [Tooltip("Unique identifier for save/load.")]
        public string fragmentId;

        [Tooltip("Display title in the Signal Archive.")]
        public string title;

        [Tooltip("Lore category for grouping in the archive.")]
        public LoreCategory category;

        [Tooltip("Which biome this fragment is associated with.")]
        public BiomeLayer biome;

        [Tooltip("Full lore text content.")]
        [TextArea(4, 12)]
        public string content;

        [Tooltip("Sort order within its category (lower = first).")]
        public int sortOrder;
    }

    /// <summary>Lore categories matching Arena narrative framework.</summary>
    public enum LoreCategory
    {
        ArenaOperations,
        ArchitectEra,
        SubjectFiles,
        StabilizedProfiles,
        BiomeData,
        Unclassified
    }
}
