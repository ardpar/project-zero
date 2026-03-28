using UnityEngine;
using Synthborn.Core.Pool;
using Synthborn.Core.Events;
using Synthborn.Core.Data;
using Synthborn.Combat.Health;

namespace Synthborn.Enemies
{
    /// <summary>
    /// Abstract base for all enemy behaviour components.
    ///
    /// Responsibilities:
    ///   - State machine scaffolding (current state, transition helpers)
    ///   - Contact-damage timer (fires DamageInfo to player at contact_damage_interval)
    ///   - Speed cap enforcement (player_speed * SpeedCapFraction)
    ///   - IPoolable lifecycle (OnPoolGet resets state, OnPoolReturn disables physics)
    ///   - Death handling: disables collider, fires GameEvents.OnEnemyDied, returns self to pool
    ///
    /// Concrete brains override <see cref="Tick"/> and <see cref="EnterState"/>.
    ///
    /// DEPENDENCIES (injected via [SerializeField]):
    ///   - EnemyData             — stats and VFX/SFX refs
    ///   - EnemyScalingConfig    — tier multipliers, speed cap
    ///   - EntityHealth          — HP component on same GameObject
    ///   - Rigidbody2D           — physics body
    ///   - Collider2D            — hitbox (disabled on death)
    ///   - Transform playerTransform — set by WaveSpawner after spawn
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(EntityHealth))]
    public abstract class EnemyBrain : MonoBehaviour, IPoolable
    {
        // ------------------------------------------------------------------ //
        // Serialized Dependencies
        // ------------------------------------------------------------------ //

        /// <summary>Data asset for this enemy type.</summary>
        [SerializeField] protected EnemyData data;

        /// <summary>Public read access to the enemy data asset.</summary>
        public EnemyData Data => data;

        /// <summary>Global scaling config (HP multipliers, speed cap).</summary>
        [SerializeField] protected EnemyScalingConfig scalingConfig;

        // ------------------------------------------------------------------ //
        // Runtime References (set by Initialize or cached in Awake)
        // ------------------------------------------------------------------ //

        protected Rigidbody2D   rb;
        protected Collider2D    col;
        protected EntityHealth  health;

        /// <summary>
        /// Player transform injected by WaveSpawner after spawning this enemy.
        /// Never null during active play — WaveSpawner guarantees assignment.
        /// </summary>
        protected Transform playerTransform;

        // ------------------------------------------------------------------ //
        // State
        // ------------------------------------------------------------------ //

        /// <summary>Current AI state. Read-only from outside; brains use SetState.</summary>
        public EnemyState CurrentState { get; private set; } = EnemyState.Dead;

        /// <summary>Wave number used to compute effective speed. Set by WaveSpawner.</summary>
        protected int currentWave = 1;

        /// <summary>Effective speed after wave scaling and hard cap (computed in Initialize).</summary>
        protected float effectiveSpeed;

        // Contact damage
        private const float ContactDamageInterval = 0.5f; // GDD default; tuning knob lives in EnemyData but interval is global
        private float _contactDamageTimer;

        // Death guard — prevents re-entrant death from double-subscribing events
        private bool _isDead;

        // Pool back-reference — set by Initialize so we can self-return
        private ObjectPool<EnemyBrain> _pool;

        // ------------------------------------------------------------------ //
        // Unity Lifecycle
        // ------------------------------------------------------------------ //

        protected virtual void Awake()
        {
            rb     = GetComponent<Rigidbody2D>();
            col    = GetComponent<Collider2D>();
            health = GetComponent<EntityHealth>();
        }

        protected virtual void OnEnable()
        {
            if (health != null)
                health.OnDeath += HandleDeath;
        }

        protected virtual void OnDisable()
        {
            if (health != null)
                health.OnDeath -= HandleDeath;
        }

        private void FixedUpdate()
        {
            if (CurrentState == EnemyState.Dead) return;
            if (playerTransform == null) return;

            Tick();
        }

        // ------------------------------------------------------------------ //
        // Public API
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Called by WaveSpawner immediately after fetching this brain from its pool.
        /// Sets wave-scaled stats, player reference, and transitions to Chase.
        /// </summary>
        /// <param name="player">The player's Transform for targeting.</param>
        /// <param name="waveNumber">Current wave number (1-based).</param>
        /// <param name="pool">Pool to return this object to on death.</param>
        public virtual void Initialize(Transform player, int waveNumber, ObjectPool<EnemyBrain> pool)
        {
            playerTransform = player;
            currentWave     = waveNumber;
            _pool           = pool;
            _isDead         = false;

            effectiveSpeed = ComputeEffectiveSpeed(waveNumber);

            SetState(EnemyState.Chase);
        }

        // ------------------------------------------------------------------ //
        // IPoolable
        // ------------------------------------------------------------------ //

        /// <summary>Called when this object is retrieved from the pool. Enables physics.</summary>
        public virtual void OnPoolGet()
        {
            gameObject.SetActive(true);
            col.enabled        = true;
            rb.linearVelocity  = Vector2.zero;
            _contactDamageTimer = 0f;
            _isDead            = false;
        }

        /// <summary>Called when this object is returned to the pool. Disables physics and rendering.</summary>
        public virtual void OnPoolReturn()
        {
            rb.linearVelocity = Vector2.zero;
            col.enabled       = false;
            gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------ //
        // Abstract / Virtual — override in concrete brains
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Per-FixedUpdate behaviour tick.
        /// Called only while CurrentState != Dead and playerTransform != null.
        /// </summary>
        protected abstract void Tick();

        /// <summary>
        /// Called once when transitioning into a new state.
        /// Override to play entrance animations, reset timers, etc.
        /// </summary>
        protected virtual void EnterState(EnemyState newState) { }

        // ------------------------------------------------------------------ //
        // Protected Helpers
        // ------------------------------------------------------------------ //

        /// <summary>Transitions to a new state and calls EnterState.</summary>
        protected void SetState(EnemyState newState)
        {
            if (CurrentState == newState) return;
            CurrentState = newState;
            EnterState(newState);
        }

        /// <summary>
        /// Moves the Rigidbody2D directly toward the player at <see cref="effectiveSpeed"/>.
        /// Uses linearVelocity (Unity 6+ API).
        /// </summary>
        protected void ChasePlayer()
        {
            if (playerTransform == null) return;

            Vector2 dir = ((Vector2)playerTransform.position - rb.position).normalized;
            rb.linearVelocity = dir * effectiveSpeed;
        }

        /// <summary>
        /// Applies contact damage to the player if the interval has elapsed.
        /// Call every Tick() from Chase (or any state where contact is possible).
        /// </summary>
        protected void TickContactDamage()
        {
            if (data == null || col == null) return;

            _contactDamageTimer += Time.fixedDeltaTime;
            if (_contactDamageTimer < ContactDamageInterval) return;

            _contactDamageTimer = 0f;

            // Physical overlap check — only damage when colliders actually touch
            if (playerTransform == null) return;
            var playerCol = playerTransform.GetComponent<Collider2D>();
            if (playerCol == null) return;

            // Use Distance check for trigger+non-trigger overlap
            var result = col.Distance(playerCol);
            if (!result.isOverlapped) return;

            var info = new DamageInfo(data.ContactDamage, DamageSource.EnemyContact, false, rb.position);
            GameEvents.RaisePlayerDamageRequested(info);
        }

        /// <summary>
        /// Computes wave-scaled speed capped to player_speed * SpeedCapFraction.
        /// effective_speed = base_speed * (1 + wave * speedScalePerWave)
        /// Hard cap: result never exceeds scalingConfig.SpeedCapFraction * player speed.
        /// Since we don't have a direct player-speed reference here, the cap is
        /// stored as an absolute value on scalingConfig.AbsoluteSpeedCap instead.
        /// </summary>
        private float ComputeEffectiveSpeed(int waveNumber)
        {
            float scaled = data.MoveSpeed * (1f + waveNumber * data.SpeedScalePerWave);
            // Hard cap — scalingConfig expresses cap as a fraction of player base speed (5.0 u/s)
            float cap = scalingConfig.SpeedCapFraction * 5f; // Player base speed = 5.0 (PlayerConfig default)
            return Mathf.Min(scaled, cap);
        }

        // ------------------------------------------------------------------ //
        // Death
        // ------------------------------------------------------------------ //

        private void HandleDeath(EntityHealth _)
        {
            if (_isDead) return;
            _isDead = true;

            SetState(EnemyState.Dead);

            rb.linearVelocity = Vector2.zero;
            col.enabled = false;

            // Compute XP to pass to listeners
            float xpMult = scalingConfig.GetXpMultiplier(data.Tier);
            int xpValue  = Mathf.RoundToInt(data.BaseXp * xpMult);

            // Fire global event — XPGemSpawner and WaveSpawner listen
            GameEvents.RaiseEnemyDied(transform.position, gameObject, xpValue);

            // HP orb drop chance (GDD: 5% default per EnemyData)
            if (Random.value < data.HpDropChance)
                GameEvents.RaiseHPOrbRequested(transform.position);

            // Spawn VFX / play SFX via event (handled by VFX/Audio systems)
            if (data.DeathVfx != null)
                GameEvents.RaiseVfxRequested(data.DeathVfx, transform.position);
            if (data.DeathSfx != null)
                GameEvents.RaiseSfxRequested(data.DeathSfx, transform.position);

            // Return to pool next frame to avoid destroying during physics step
            StartCoroutine(ReturnToPoolNextFrame());
        }

        private System.Collections.IEnumerator ReturnToPoolNextFrame()
        {
            yield return null;
            _pool?.Return(this);
        }
    }
}
