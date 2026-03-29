namespace Synthborn.Waves
{
    /// <summary>
    /// The six Arena eras, ordered from outermost (newest) to innermost (oldest).
    /// Each biome represents a layer of the Arena's history.
    /// </summary>
    public enum BiomeLayer
    {
        /// <summary>Era 1 — Active Operation. Rooms 1-16.</summary>
        Atrium = 0,

        /// <summary>Era 2 — Peak Use. Rooms 17-33.</summary>
        AssayChambers = 1,

        /// <summary>Era 3 — Post-Architect. Rooms 34-50.</summary>
        DeepArchive = 2,

        /// <summary>Era 4 — Unknown Incident. Rooms 51-67.</summary>
        CollapseStratum = 3,

        /// <summary>Era 5 — Current Drift. Rooms 68-84.</summary>
        CorruptionLayer = 4,

        /// <summary>Era ? — Off-System. Rooms 85-100.</summary>
        NullChamber = 5
    }
}
