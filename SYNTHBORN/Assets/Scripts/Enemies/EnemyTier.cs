namespace Synthborn.Enemies
{
    /// <summary>
    /// Classifies an enemy's power tier, which drives HP and XP multipliers.
    /// Normal = 1x, Elite = 3x HP / 5x XP, Boss = 10x HP / 20x XP.
    /// Values are consumed by EntityHealth scaling and XPGemSpawner.
    /// </summary>
    public enum EnemyTier
    {
        Normal = 0,
        Elite  = 1,
        Boss   = 2,
    }
}
