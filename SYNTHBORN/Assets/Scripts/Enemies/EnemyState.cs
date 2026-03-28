namespace Synthborn.Enemies
{
    /// <summary>
    /// All possible states an enemy brain can occupy.
    /// Not every brain uses every state — see individual brain docs.
    /// </summary>
    public enum EnemyState
    {
        /// <summary>Moving toward the player.</summary>
        Chase     = 0,

        /// <summary>Shooter only: stationary, firing projectiles at the player.</summary>
        Shooting  = 1,
        /// <summary>Exploder only: standing still, swelling before detonation.</summary>
        Priming   = 2,
        /// <summary>Exploder only: AoE damage applied this frame, then transitions to Dead.</summary>
        Exploding = 3,
        /// <summary>Entity is dead: collider disabled, pooled return pending.</summary>
        Dead      = 4,
    }
}
