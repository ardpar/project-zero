using UnityEngine;

namespace Synthborn.Enemies
{
    /// <summary>
    /// Two-state brain: Chase → Dead.
    ///
    /// Used directly for the Chaser enemy type.
    /// Also serves as the behaviour base for Runner and Tank variants —
    /// all three use this class; visual and stat differences are entirely
    /// driven by their respective <see cref="EnemyData"/> ScriptableObjects
    /// (different MoveSpeed, BaseHp, HitboxRadius, sprites).
    /// State logic:
    ///   Chase  — moves straight at the player each FixedUpdate, applies
    ///            contact-damage timer every 0.5 s when overlapping.
    ///   Dead   — handled by EnemyBrain base (pool return, event).
    /// </summary>
    public class ChaserBrain : EnemyBrain
    {
        // ------------------------------------------------------------------ //
        // EnemyBrain overrides
        /// <inheritdoc/>
        protected override void EnterState(EnemyState newState)
        {
            if (newState == EnemyState.Chase)
            {
                // Nothing special — physics already enabled by OnPoolGet
            }
        }
        protected override void Tick()
            // ChaserBrain only has one active state
            if (CurrentState != EnemyState.Chase) return;
            ChasePlayer();
            TickContactDamage();
    }
}
