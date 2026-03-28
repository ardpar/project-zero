namespace Synthborn.Mutations
{
    /// <summary>Body slot categories for slot mutations.</summary>
    public enum MutationSlot
    {
        None = 0,   // Passive mutations use None
        Arms = 1,
        Legs = 2,
        Back = 3,
        Head = 4
    }

    /// <summary>Mutation category: slot (visual+mechanic) or passive (stats+effects).</summary>
    public enum MutationCategory
    {
        Slot,
        Passive
    }

    /// <summary>Mutation rarity tier. Affects drop weight and power level.</summary>
    public enum MutationRarity
    {
        Common,     // ~50% base weight, +10-15% stat boost
        Uncommon,   // ~30%, +20-30% or simple special effect
        Rare,       // ~15%, +40%+ or powerful special effect
        Legendary   // ~5%, game-changing or strong synergy enabler
    }
}
