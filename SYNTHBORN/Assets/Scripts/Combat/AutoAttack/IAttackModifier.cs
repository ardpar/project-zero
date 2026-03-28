namespace Synthborn.Combat
{
    /// <summary>
    /// Structural attack modifier pushed by mutations (new slots, pierce, chain).
    /// Numeric modifiers (speed, damage, crit) go through CombatStatBlock instead.
    /// </summary>
    public interface IAttackModifier
    {
        string ModifierId { get; }
        void Apply(AutoAttackController controller);
        void Remove(AutoAttackController controller);
    }
}
