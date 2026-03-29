// Implements: ADR-002 — EntityHealth MonoBehaviour (Tier 2)
// Design doc: entity-health-system.md
//
// Shared HP/Armor component used by both player and enemies.
// Configuration lives in EntityHealthConfig SO; runtime mutation bonuses
// are supplied via CombatStatBlock injection (player only).
//
// States: Alive → Invulnerable (sub-state) → Dead
// TakeDamage() is the single entry point for all damage.
// OnDeath fires GameEvents; callers must not cache IsDead across frames.
//
// Thread safety: Unity main thread only.

using System;
using System.Collections;
using UnityEngine;
using Synthborn.Core.Data;
using Synthborn.Core.Events;
using Synthborn.Core.Stats;

namespace Synthborn.Combat.Health
{
    /// <summary>
    /// Manages HP and armor for any entity (player or enemy).
    /// Attach to the root GameObject alongside the entity's brain/controller.
    /// </summary>
    public class EntityHealth : MonoBehaviour, IDamageable
    {
        [Header("Config")]
        [SerializeField] private EntityHealthConfig _config;

        [Header("Enemy Wave Scaling (leave 0 for player)")]
        [Tooltip("Set by EnemyInitializer at spawn time. Wave 1 = no bonus.")]
        [SerializeField] private int _waveNumber = 1;

        [Header("Enemy Tier Multiplier (leave 1 for player)")]
        [Tooltip("Normal=1, Elite=3, Boss=10 per entity-health-system GDD.")]
        [SerializeField] private float _tierMultiplier = 1f;

        // ─────────────────────────────────────────────
        // Runtime state
        // ─────────────────────────────────────────────

        private int   _currentHp;
        private int   _maxHp;
        private int   _armor;
        private bool  _isInvulnerable;
        private bool  _isDead;

        /// <summary>Injected by the player's owner — null for enemies.</summary>
        private CombatStatBlock _statBlock;

        /// <summary>True = this is the player entity (affects events + invulnerability).</summary>
        private bool _isPlayer;

        // ─────────────────────────────────────────────
        // Properties
        // ─────────────────────────────────────────────

        public bool IsDead     => _isDead;
        public int  CurrentHp  => _currentHp;
        public int  MaxHp      => _maxHp;
        public int  Armor      => _armor;

        // ─────────────────────────────────────────────
        // Initialisation
        // ─────────────────────────────────────────────

        /// <summary>
        /// Initialise as a player entity, supplying the shared CombatStatBlock
        /// so hp_modifier and armor from mutations are reflected.
        /// </summary>
        public void InitialiseAsPlayer(CombatStatBlock statBlock)
        {
            _isPlayer  = true;
            _statBlock = statBlock;
            RecomputeStats();
            _currentHp = _maxHp; // Player starts at full HP
        }

        /// <summary>
        /// Initialise as an enemy entity.
        /// <paramref name="waveNumber"/> and <paramref name="tierMultiplier"/> are set
        /// by EnemyInitializer at spawn time per entity-health-system GDD rule 16-18.
        /// </summary>
        public void InitialiseAsEnemy(int waveNumber, float tierMultiplier)
        {
            _isPlayer      = false;
            _statBlock     = null;
            _waveNumber    = waveNumber;
            _tierMultiplier = tierMultiplier;
            RecomputeStats();
            _currentHp     = _maxHp;
        }

        // ─────────────────────────────────────────────
        // Stat computation
        // ─────────────────────────────────────────────

        /// <summary>
        /// Recomputes _maxHp and _armor from config + statBlock (player)
        /// or wave-scaling formula (enemy). Call whenever mutations change.
        /// </summary>
        public void RecomputeStats()
        {
            if (_isPlayer)
            {
                // GDD entity-health-system.md — Effective Max HP formula
                // effective_max_hp = base_max_hp * (1 + hp_modifier)
                float hpMod  = _statBlock != null ? _statBlock.ClampedHpModifier : 0f;
                int newMaxHp = Mathf.RoundToInt(_config.baseMaxHp * (1f + hpMod));
                newMaxHp = Mathf.Max(newMaxHp, 35); // GDD edge case: never below 35

                // GDD edge case: if max_hp decreases, clamp current_hp
                if (newMaxHp < _maxHp)
                    _currentHp = Mathf.Min(_currentHp, newMaxHp);

                _maxHp = newMaxHp;
                _armor = _config.baseArmor + (_statBlock != null ? _statBlock.ClampedArmor : 0);
            }
            else
            {
                // GDD enemy_hp = base_hp * tier_multiplier * (1 + wave_number * wave_hp_scale)
                float scaled = _config.baseMaxHp * _tierMultiplier
                               * (1f + _waveNumber * _config.waveHpScale);
                _maxHp = Mathf.RoundToInt(scaled);
                _maxHp = Mathf.Max(_maxHp, 1);
                _armor = 0; // GDD rule 8: enemies have no armor
            }
        }

        // ─────────────────────────────────────────────
        // IDamageable — public API
        // ─────────────────────────────────────────────

        /// <summary>
        /// Single entry point for all damage.
        /// Applies armor reduction, invulnerability, clamping, and death.
        /// Fires GameEvents.OnDamageDealt and (on death) OnEnemyDied/OnPlayerDied.
        /// </summary>
        public void TakeDamage(DamageInfo info)
        {
            if (_isDead)         return; // GDD edge case: ignore damage on dead entity
            if (_isInvulnerable) return; // GDD rule 10: invulnerability window active

            // GDD formula: actual_damage = max(raw_damage - armor, 1)
            int actualDamage = Mathf.Max(info.RawDamage - _armor, 1);
            DamageInfo resolvedInfo = info.WithFinalDamage(actualDamage);

            _currentHp -= actualDamage;
            _currentHp  = Mathf.Clamp(_currentHp, 0, _maxHp);

            // Broadcast damage event for VFX/HUD
            GameEvents.RaiseDamageDealt(info.HitPosition, actualDamage, info.IsCrit);

            if (_isPlayer)
                GameEvents.RaisePlayerHPChanged(_currentHp, _maxHp);

            if (_currentHp <= 0)
            {
                Die();
                return;
            }

            // GDD rule 10: start invulnerability window (player only if configured)
            if (_config.useInvulnerability)
                StartCoroutine(InvulnerabilityCoroutine());
        }

        /// <summary>
        /// Heals the entity. Current HP cannot exceed max HP.
        /// GDD rule 14: no overheal.
        /// </summary>
        public void Heal(int amount)
        {
            if (_isDead) return;
            _currentHp = Mathf.Clamp(_currentHp + amount, 0, _maxHp);

            if (_isPlayer)
                GameEvents.RaisePlayerHPChanged(_currentHp, _maxHp);
        }

        // ─────────────────────────────────────────────
        // Death
        // ─────────────────────────────────────────────

        private void Die()
        {
            _isDead         = true;
            _isInvulnerable = false;
            _currentHp      = 0;

            // Stop invulnerability coroutine if running (prevents stale state on pool reuse)
            StopAllCoroutines();

            if (_isPlayer)
            {
                GameEvents.RaisePlayerDied();
            }
            else
            {
                // EnemyData XP value is read by EnemyInitializer and stored here at spawn
                GameEvents.RaiseEnemyDied(transform.position, gameObject, _xpValue);
            }
        }

        // ─────────────────────────────────────────────
        // Enemy XP value (set by EnemyInitializer)
        // ─────────────────────────────────────────────

        private int _xpValue;

        /// <summary>Called by EnemyInitializer so Die() can broadcast the correct XP.</summary>
        public void SetXpValue(int xp) => _xpValue = xp;

        // ─────────────────────────────────────────────
        // Invulnerability coroutine
        // ─────────────────────────────────────────────

        private IEnumerator InvulnerabilityCoroutine()
        {
            _isInvulnerable = true;
            yield return new WaitForSeconds(_config.invulnerabilityWindow);
            _isInvulnerable = false;
        }

        // ─────────────────────────────────────────────
        // Pause integration
        // ─────────────────────────────────────────────

        private void OnEnable()
        {
            GameEvents.OnGamePaused  += OnGamePaused;
            GameEvents.OnGameResumed += OnGameResumed;
        }

        private void OnDisable()
        {
            GameEvents.OnGamePaused  -= OnGamePaused;
            GameEvents.OnGameResumed -= OnGameResumed;
        }

        // WaitForSeconds uses scaled time; disabling physics via PauseManager
        // (timeScale=0) automatically pauses this coroutine — no extra work needed.
        private void OnGamePaused()  { /* timeScale handles it */ }
        private void OnGameResumed() { /* timeScale handles it */ }
    }
}
