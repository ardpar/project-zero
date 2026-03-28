// Implements: ADR-002 — PlayerController FSM (Tier 3)
// Design doc: player-controller.md
//
// States: Idle, Moving, Dashing, DashCooldown (parallel sub-state), Dead, Paused
// Movement formula: effective_speed = base_move_speed * (1 + speed_modifier)
//   speed_modifier clamped to [speedModifierMinClamp, speedModifierMaxClamp]
// Dash formula:  dash_speed = dash_distance / dash_duration
//   effective_dash_cd = base_dash_cooldown * (1 - dash_cd_modifier)
//   clamp to [dashCooldownMinClamp, ∞)
//
// Rigidbody2D.linearVelocity used (Unity 6+ API).
// Enemy layer collision is ignored during dash via Physics2D layer matrix.
// This component raises GameEvents (dash start/end/ready) but never calls
// other gameplay systems directly — it is a "provider" per the GDD.

using UnityEngine;
using Synthborn.Core.Events;
using Synthborn.Core.Stats;
using Synthborn.Player.Data;

namespace Synthborn.Player
{
    /// <summary>
    /// Movement FSM for the player entity.
    /// Requires a Rigidbody2D (Kinematic) and CircleCollider2D on the same GameObject.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerController : MonoBehaviour
    {
        // ─────────────────────────────────────────────
        // Inspector wiring
        // ─────────────────────────────────────────────

        [SerializeField] private PlayerConfig _config;

        // ─────────────────────────────────────────────
        // State enum
        // ─────────────────────────────────────────────

        private enum PlayerState { Idle, Moving, Dashing, Dead, Paused }

        private PlayerState _state         = PlayerState.Idle;
        private bool        _dashOnCooldown = false;
        private float       _dashCooldownRemaining;

        // ─────────────────────────────────────────────
        // Dash runtime
        // ─────────────────────────────────────────────

        private Vector2 _dashDirection;
        private float   _dashTimeRemaining;
        private Vector2 _dashStartPosition;

        // ─────────────────────────────────────────────
        // Facing direction — used when dash is pressed with no move input
        // ─────────────────────────────────────────────

        private Vector2 _lastMoveDirection = Vector2.right;

        // ─────────────────────────────────────────────
        // Dependencies
        // ─────────────────────────────────────────────

        private Rigidbody2D        _rb;
        private CircleCollider2D   _collider;
        private PlayerInputHandler _input;
        private CombatStatBlock    _statBlock; // Injected — null = no mutations applied

        // ─────────────────────────────────────────────
        // Public read API (consumed by Auto-Attack, Camera, Enemy AI)
        // ─────────────────────────────────────────────

        /// <summary>Last normalised movement direction. Valid even when standing still.</summary>
        public Vector2 FacingDirection => _lastMoveDirection;

        /// <summary>Current world-space position (forwarded from transform for convenience).</summary>
        public Vector2 Position => _rb.position;

        /// <summary>True while a dash is executing.</summary>
        public bool IsDashing => _state == PlayerState.Dashing;

        /// <summary>Normalised 0-1 progress of the current dash cooldown. 1 = ready.</summary>
        public float DashCooldownProgress =>
            _dashOnCooldown && _config != null
                ? 1f - (_dashCooldownRemaining /
                        _config.GetEffectiveDashCooldown(
                            _statBlock?.ClampedDashCooldownModifier ?? 0f))
                : 1f;

        // ─────────────────────────────────────────────
        // Initialisation
        // ─────────────────────────────────────────────

        /// <summary>
        /// Injects the shared CombatStatBlock so speed/dash formulas use live mutation values.
        /// Call before the first Update. If not called, defaults from PlayerConfig are used.
        /// </summary>
        public void Initialise(CombatStatBlock statBlock)
        {
            _statBlock = statBlock;
        }

        private void Awake()
        {
            _rb       = GetComponent<Rigidbody2D>();
            _collider = GetComponent<CircleCollider2D>();
            _input    = GetComponent<PlayerInputHandler>();

            // GDD rule 6: circle collider radius set from config
            if (_config != null)
                _collider.radius = _config.colliderRadius;
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerDied  += HandleDeath;
            GameEvents.OnGamePaused  += HandlePause;
            GameEvents.OnGameResumed += HandleResume;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDied  -= HandleDeath;
            GameEvents.OnGamePaused  -= HandlePause;
            GameEvents.OnGameResumed -= HandleResume;
        }

        // ─────────────────────────────────────────────
        // Update loop
        // ─────────────────────────────────────────────

        private void Update()
        {
            if (_state == PlayerState.Dead || _state == PlayerState.Paused)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            TickDashCooldown(Time.deltaTime);
            HandleDashInput();
            HandleMovement();
        }

        // ─────────────────────────────────────────────
        // Dash cooldown ticker
        // ─────────────────────────────────────────────

        private void TickDashCooldown(float dt)
        {
            if (!_dashOnCooldown) return;

            _dashCooldownRemaining -= dt;
            if (_dashCooldownRemaining <= 0f)
            {
                _dashCooldownRemaining = 0f;
                _dashOnCooldown        = false;
                GameEvents.RaisePlayerDashReady();
            }
        }

        // ─────────────────────────────────────────────
        // Dash input & execution
        // ─────────────────────────────────────────────

        private void HandleDashInput()
        {
            if (_state == PlayerState.Dashing) return;
            if (!_input.DashPressed)           return;
            if (_dashOnCooldown)               return;

            // GDD rule 15: no move input → dash in facing direction
            Vector2 dir = _input.MoveInput.sqrMagnitude > 0.01f
                ? _input.MoveInput
                : _lastMoveDirection;

            StartDash(dir);
        }

        private void StartDash(Vector2 direction)
        {
            _state             = PlayerState.Dashing;
            _dashDirection     = direction.normalized;
            _dashTimeRemaining = _config.dashDuration;
            _dashStartPosition = _rb.position;

            // GDD rule 17: ignore enemy collisions during dash
            Physics2D.IgnoreLayerCollision(
                gameObject.layer,
                LayerMaskToLayer(_config.enemyLayerMask),
                true);

            GameEvents.RaisePlayerDashStarted(_dashDirection);
        }

        private void HandleMovement()
        {
            if (_state == PlayerState.Dashing)
            {
                ExecuteDash();
                return;
            }

            Vector2 moveInput = _input.MoveInput;

            if (moveInput.sqrMagnitude > 0.01f)
            {
                _lastMoveDirection = moveInput; // GDD rule 5: update facing
                _state             = PlayerState.Moving;

                float speed = _config.GetEffectiveMoveSpeed(
                    _statBlock?.ClampedSpeedModifier ?? 0f);

                _rb.linearVelocity = moveInput * speed;
            }
            else
            {
                // GDD rule 2: instant stop — no deceleration
                _state             = PlayerState.Idle;
                _rb.linearVelocity = Vector2.zero;
            }
        }

        private void ExecuteDash()
        {
            _dashTimeRemaining -= Time.deltaTime;

            float dashSpeed    = _config.GetDashSpeed();
            _rb.linearVelocity = _dashDirection * dashSpeed;

            // GDD edge case: dash stops at arena boundary (handled by solid colliders)
            // GDD rule 16: also check if we have exceeded max distance
            float distanceTravelled = Vector2.Distance(_rb.position, _dashStartPosition);
            bool  timeExpired       = _dashTimeRemaining <= 0f;
            bool  distanceExceeded  = distanceTravelled >= _config.dashDistance;

            if (timeExpired || distanceExceeded)
                EndDash();
        }

        private void EndDash()
        {
            _state             = PlayerState.Idle;
            _rb.linearVelocity = Vector2.zero;

            // GDD rule 17: restore enemy collision
            Physics2D.IgnoreLayerCollision(
                gameObject.layer,
                LayerMaskToLayer(_config.enemyLayerMask),
                false);

            // Start cooldown
            _dashOnCooldown        = true;
            _dashCooldownRemaining = _config.GetEffectiveDashCooldown(
                _statBlock?.ClampedDashCooldownModifier ?? 0f);

            GameEvents.RaisePlayerDashEnded();
        }

        // ─────────────────────────────────────────────
        // Event handlers
        // ─────────────────────────────────────────────

        private void HandleDeath()
        {
            // GDD edge case: death is priority — cancel any in-progress dash
            if (_state == PlayerState.Dashing)
            {
                Physics2D.IgnoreLayerCollision(
                    gameObject.layer,
                    LayerMaskToLayer(_config.enemyLayerMask),
                    false);
                // GDD edge case: dash + death → dash cooldown does NOT start
                _dashOnCooldown = false;
            }

            _state             = PlayerState.Dead;
            _rb.linearVelocity = Vector2.zero;
            _input.DisableInput(); // GDD rule: all input disabled on death
        }

        private void HandlePause()
        {
            // GDD edge case: pause during dash → dash cancelled, cooldown not started
            if (_state == PlayerState.Dashing)
            {
                Physics2D.IgnoreLayerCollision(
                    gameObject.layer,
                    LayerMaskToLayer(_config.enemyLayerMask),
                    false);
                _dashOnCooldown = false;
                GameEvents.RaisePlayerDashEnded(); // Notify VFX to clean up trail
            }

            _state             = PlayerState.Paused;
            _rb.linearVelocity = Vector2.zero;
            _input.DisableInput();
        }

        private void HandleResume()
        {
            if (_state != PlayerState.Paused) return;
            _state = PlayerState.Idle;
            _input.EnableInput();
        }

        // ─────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────

        /// <summary>Extracts the first layer index from a LayerMask.</summary>
        private static int LayerMaskToLayer(LayerMask mask)
        {
            int m = mask.value;
            for (int i = 0; i < 32; i++)
                if ((m & (1 << i)) != 0) return i;
            return 0;
        }
    }
}
