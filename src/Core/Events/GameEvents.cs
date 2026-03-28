// Implements: ADR-002 — Static EventBus (Tier 0)
// Design docs: player-controller.md, entity-health-system.md,
//              auto-attack-system.md, projectile-damage-system.md
//
// All cross-system communication flows through static C# events here.
// Subscribers must unsubscribe in OnDestroy to prevent memory leaks.
// Call GameEvents.Cleanup() on run reset to hard-clear all subscriptions.
//
// Naming: On<Noun><PastTense> for events that have already occurred.

using System;
using UnityEngine;

namespace Synthborn.Core.Events
{
    /// <summary>
    /// Central event bus for all gameplay systems.
    /// Systems raise events; other systems react — no direct references required.
    /// </summary>
    public static class GameEvents
    {
        // ─────────────────────────────────────────────
        // Player / Movement
        // ─────────────────────────────────────────────

        /// <summary>
        /// Raised when the player begins a dash.
        /// <para>Vector2: world-space dash direction (normalized).</para>
        /// </summary>
        public static event Action<Vector2> OnPlayerDashStarted;

        /// <summary>
        /// Raised when a dash finishes (either completed or cut short by arena boundary).
        /// </summary>
        public static event Action OnPlayerDashEnded;

        /// <summary>
        /// Raised when the player's dash cooldown finishes and dash is available again.
        /// </summary>
        public static event Action OnPlayerDashReady;

        // ─────────────────────────────────────────────
        // Health / Damage
        // ─────────────────────────────────────────────

        /// <summary>
        /// Raised after a <see cref="Synthborn.Combat.Health.EntityHealth"/> processes a hit.
        /// <para>Vector2: world-space hit position.</para>
        /// <para>int: final damage applied (post-armor).</para>
        /// <para>bool: true if the hit was a critical strike.</para>
        /// </summary>
        public static event Action<Vector2, int, bool> OnDamageDealt;

        /// <summary>
        /// Raised when an enemy's HP reaches zero.
        /// <para>Vector2: world-space death position.</para>
        /// <para>GameObject: the dying enemy's root GameObject.</para>
        /// <para>int: XP value of the enemy.</para>
        /// </summary>
        public static event Action<Vector2, GameObject, int> OnEnemyDied;

        /// <summary>
        /// Raised when the player's HP reaches zero.
        /// </summary>
        public static event Action OnPlayerDied;

        /// <summary>
        /// Raised when the player's current HP changes (damage or healing).
        /// <para>int: new current HP.</para>
        /// <para>int: max HP.</para>
        /// </summary>
        public static event Action<int, int> OnPlayerHPChanged;

        // ─────────────────────────────────────────────
        // Projectile
        // ─────────────────────────────────────────────

        /// <summary>
        /// Raised by a ShooterBrain when it wants to fire a projectile via the pool,
        /// avoiding a direct reference to the pool owner.
        /// <para>Vector2: spawn origin (world space).</para>
        /// <para>Vector2: normalized direction.</para>
        /// <para>ScriptableObject (ProjectileData): data asset to use.</para>
        /// </summary>
        public static event Action<Vector2, Vector2, UnityEngine.Object> OnProjectileRequested;

        // ─────────────────────────────────────────────
        // Pause / Game State
        // ─────────────────────────────────────────────

        /// <summary>
        /// Raised when the game is paused (pause menu, level-up screen, etc.).
        /// </summary>
        public static event Action OnGamePaused;

        /// <summary>
        /// Raised when the game resumes from a paused state.
        /// </summary>
        public static event Action OnGameResumed;

        // ─────────────────────────────────────────────
        // Invocators (internal helpers)
        // ─────────────────────────────────────────────

        public static void RaisePlayerDashStarted(Vector2 direction) =>
            OnPlayerDashStarted?.Invoke(direction);

        public static void RaisePlayerDashEnded() =>
            OnPlayerDashEnded?.Invoke();

        public static void RaisePlayerDashReady() =>
            OnPlayerDashReady?.Invoke();

        public static void RaiseDamageDealt(Vector2 position, int finalDamage, bool isCrit) =>
            OnDamageDealt?.Invoke(position, finalDamage, isCrit);

        public static void RaiseEnemyDied(Vector2 position, GameObject enemy, int xpValue) =>
            OnEnemyDied?.Invoke(position, enemy, xpValue);

        public static void RaisePlayerDied() =>
            OnPlayerDied?.Invoke();

        public static void RaisePlayerHPChanged(int currentHP, int maxHP) =>
            OnPlayerHPChanged?.Invoke(currentHP, maxHP);

        public static void RaiseProjectileRequested(Vector2 origin, Vector2 direction, UnityEngine.Object data) =>
            OnProjectileRequested?.Invoke(origin, direction, data);

        public static void RaiseGamePaused() =>
            OnGamePaused?.Invoke();

        public static void RaiseGameResumed() =>
            OnGameResumed?.Invoke();

        // ─────────────────────────────────────────────
        // Lifecycle
        // ─────────────────────────────────────────────

        /// <summary>
        /// Clears ALL event subscriptions. Call on run reset or scene unload
        /// to prevent stale subscribers from leaking across runs.
        /// </summary>
        public static void Cleanup()
        {
            OnPlayerDashStarted  = null;
            OnPlayerDashEnded    = null;
            OnPlayerDashReady    = null;
            OnDamageDealt        = null;
            OnEnemyDied          = null;
            OnPlayerDied         = null;
            OnPlayerHPChanged    = null;
            OnProjectileRequested = null;
            OnGamePaused         = null;
            OnGameResumed        = null;
        }
    }
}
