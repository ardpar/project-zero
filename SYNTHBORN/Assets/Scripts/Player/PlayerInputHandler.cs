// Implements: ADR-002 — PlayerInputHandler (Tier 3)
// Design doc: player-controller.md — Input rules 18-20
//
// Wraps Unity Input System actions. Provides a clean value API to
// PlayerController so the controller never touches InputAction directly.
// Supports simultaneous keyboard+mouse and gamepad (GDD rule 19).

using UnityEngine;
using UnityEngine.InputSystem;

namespace Synthborn.Player
{
    /// <summary>
    /// Reads raw Unity Input System values and exposes clean properties
    /// consumed by <see cref="PlayerController"/>.
    /// Attach to the same GameObject as PlayerController.
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputHandler : MonoBehaviour
    {
        // ─────────────────────────────────────────────
        // Cached input references
        // ─────────────────────────────────────────────

        private InputAction _moveAction;
        private InputAction _dashAction;

        // ─────────────────────────────────────────────
        // Public read API
        // ─────────────────────────────────────────────

        /// <summary>
        /// Normalised movement vector this frame.
        /// GDD rule 20: diagonal input is normalised so it is not faster.
        /// Returns Vector2.zero when no input is held.
        /// </summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>True during the frame the dash button was first pressed.</summary>
        public bool DashPressed { get; private set; }

        // ─────────────────────────────────────────────
        // Lifecycle
        // ─────────────────────────────────────────────

        private void Awake()
        {
            PlayerInput playerInput = GetComponent<PlayerInput>();
            _moveAction = playerInput.actions["Move"];
            _dashAction = playerInput.actions["Dash"];
        }

        private void Update()
        {
            // GDD rule 20: normalise so diagonal (1,1) == magnitude 1
            MoveInput  = _moveAction.ReadValue<Vector2>().normalized;
            DashPressed = _dashAction.WasPressedThisFrame();
        }

        // ─────────────────────────────────────────────
        // Enable/Disable (maps enabled state = input state)
        // ─────────────────────────────────────────────

        /// <summary>Disables all input reads. Called on player death or pause.</summary>
        public void DisableInput()
        {
            _moveAction?.Disable();
            _dashAction?.Disable();
            MoveInput   = Vector2.zero;
            DashPressed = false;
        }

        /// <summary>Re-enables input reads. Called on game resume.</summary>
        public void EnableInput()
        {
            _moveAction?.Enable();
            _dashAction?.Enable();
        }
    }
}
