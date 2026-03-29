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
using Synthborn.Core.Data;

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
        /// <summary>
        /// Raised when an enemy dies. Second param is the enemy GameObject.
        /// Use GetComponent to access EnemyData if needed.
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
        // XP / Progression
        // ─────────────────────────────────────────────

        public static event Action<int> OnXPGemCollected;
        public static event Action<int, int> OnXPChanged;
        public static event Action<int> OnLevelUp;

        public static void XPGemCollected(int xpValue) =>
            OnXPGemCollected?.Invoke(xpValue);

        public static void XPChanged(int currentXP, int xpToNext) =>
            OnXPChanged?.Invoke(currentXP, xpToNext);

        public static void LevelUp(int newLevel) =>
            OnLevelUp?.Invoke(newLevel);

        // ─────────────────────────────────────────────
        // Mutations
        // ─────────────────────────────────────────────

        /// <summary>Raised when a mutation is applied. bool = true if slot mutation.</summary>
        public static event Action<string, bool> OnMutationApplied;

        public static void RaiseMutationApplied(string mutationId, bool isSlot) =>
            OnMutationApplied?.Invoke(mutationId, isSlot);

        /// <summary>Raised when a synergy activates. string1=id, string2=displayName.</summary>
        public static event Action<string, string> OnSynergyActivated;

        public static void RaiseSynergyActivated(string synergyId, string displayName) =>
            OnSynergyActivated?.Invoke(synergyId, displayName);

        // ─────────────────────────────────────────────
        // HP Orb
        // ─────────────────────────────────────────────

        public static event Action<Vector2> OnHPOrbRequested;

        public static void RaiseHPOrbRequested(Vector2 position) =>
            OnHPOrbRequested?.Invoke(position);

        // ─────────────────────────────────────────────
        // Waves
        // ─────────────────────────────────────────────

        public static event Action<int> OnWaveStarted;
        public static event Action OnWaveCleared;

        public static void WaveStarted(int waveNumber) =>
            OnWaveStarted?.Invoke(waveNumber);

        public static void WaveCleared() =>
            OnWaveCleared?.Invoke();

        // ─────────────────────────────────────────────
        // Boss
        // ─────────────────────────────────────────────

        public static event Action OnBossSpawned;
        public static event Action OnBossDefeated;

        public static void BossSpawned() =>
            OnBossSpawned?.Invoke();

        public static void RaiseBossDefeated() =>
            OnBossDefeated?.Invoke();

        // ─────────────────────────────────────────────
        // VFX / SFX Requests
        // ─────────────────────────────────────────────

        public static event Action<GameObject, Vector2> OnVfxRequested;
        public static event Action<UnityEngine.Object, Vector2> OnSfxRequested;
        public static event Action<DamageInfo> OnPlayerDamageRequested;

        public static void RaiseVfxRequested(GameObject vfxPrefab, Vector2 position) =>
            OnVfxRequested?.Invoke(vfxPrefab, position);

        public static void RaiseSfxRequested(UnityEngine.Object sfxClip, Vector2 position) =>
            OnSfxRequested?.Invoke(sfxClip, position);

        public static void RaisePlayerDamageRequested(DamageInfo info) =>
            OnPlayerDamageRequested?.Invoke(info);

        // ─────────────────────────────────────────────
        // Lifecycle
        // ─────────────────────────────────────────────

        /// <summary>
        // ─── Loot ───
        public static event Action<string, string, int> OnLootDropped; // itemId, displayName, rarity
        public static void RaiseLootDropped(string id, string name, Items.ItemRarity rarity) =>
            OnLootDropped?.Invoke(id, name, (int)rarity);

        // ─── Heal ───
        public static event Action<float> OnPlayerHealRequested;
        public static void RaisePlayerHealRequested(float fraction) => OnPlayerHealRequested?.Invoke(fraction);

        // ─── Materials ───
        public static event Action<string, Vector2> OnMaterialAwarded; // materialName, worldPos
        public static void MaterialAwarded(string name, Vector2 pos) => OnMaterialAwarded?.Invoke(name, pos);

        // ─── Substrate Fragments ───
        public static event Action<int> OnFragmentChanged;
        public static void FragmentChanged(int total) => OnFragmentChanged?.Invoke(total);
        // Legacy aliases
        public static event Action<int> OnGoldChanged { add => OnFragmentChanged += value; remove => OnFragmentChanged -= value; }
        public static void GoldChanged(int total) => FragmentChanged(total);

        // ─── Level/Stage ───
        public static event Action<int, string> OnLevelStarted;
        public static void LevelStarted(int level, string name) => OnLevelStarted?.Invoke(level, name);

        public static event Action<int> OnLevelCleared;
        public static void LevelCleared(int level) => OnLevelCleared?.Invoke(level);

        // ─── Trial Chambers ───
        public static event Action<int> OnChamberStarted; // chamberNumber
        public static event Action<int> OnChamberCleared; // chamberNumber
        public static event Action OnCalibrationIntervalStarted;
        public static event Action OnReturnToArenaMap;

        public static void ChamberStarted(int chamber) => OnChamberStarted?.Invoke(chamber);
        public static void ChamberCleared(int chamber) => OnChamberCleared?.Invoke(chamber);
        public static void CalibrationIntervalStarted() => OnCalibrationIntervalStarted?.Invoke();
        public static void ReturnToArenaMap() => OnReturnToArenaMap?.Invoke();

        /// Clears ALL event subscriptions. Call on run reset or scene unload
        /// to prevent stale subscribers from leaking across runs.
        /// </summary>
        public static void Cleanup()
        {
            OnPlayerDashStarted   = null;
            OnPlayerDashEnded     = null;
            OnPlayerDashReady     = null;
            OnDamageDealt         = null;
            OnEnemyDied           = null;
            OnPlayerDied          = null;
            OnPlayerHPChanged     = null;
            OnProjectileRequested = null;
            OnGamePaused          = null;
            OnGameResumed         = null;
            OnXPGemCollected      = null;
            OnXPChanged           = null;
            OnLevelUp             = null;
            OnWaveStarted         = null;
            OnWaveCleared         = null;
            OnBossSpawned         = null;
            OnBossDefeated        = null;
            OnVfxRequested        = null;
            OnSfxRequested        = null;
            OnPlayerDamageRequested = null;
            OnHPOrbRequested        = null;
            OnMutationApplied       = null;
            OnSynergyActivated      = null;
            OnLootDropped           = null;
            OnPlayerHealRequested   = null;
            OnFragmentChanged       = null;
            OnMaterialAwarded       = null;
            OnLevelStarted          = null;
            OnLevelCleared          = null;
            OnChamberStarted        = null;
            OnChamberCleared        = null;
            OnCalibrationIntervalStarted = null;
            OnReturnToArenaMap      = null;
        }
    }
}
