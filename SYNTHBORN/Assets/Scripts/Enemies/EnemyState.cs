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

        /// <summary>Charger only: winding up before a dash attack.</summary>
        Charging  = 5,

        /// <summary>Charger only: dashing at high speed toward the player.</summary>
        Dashing   = 6,

        /// <summary>Summoner only: stationary, spawning minions.</summary>
        Summoning = 7,

        /// <summary>Entity is dead: collider disabled, pooled return pending.</summary>
        Dead      = 4,
    }
}
