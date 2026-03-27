// PROTOTYPE - NOT FOR PRODUCTION
// Question: Is the core loop fun?
// Date: 2026-03-27

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _baseMoveSpeed = 5f;
    [SerializeField] private float _dashDistance = 3f;
    [SerializeField] private float _dashDuration = 0.15f;
    [SerializeField] private float _baseDashCooldown = 3f;

    private Rigidbody2D _rb;
    private Vector2 _moveInput;
    private Vector2 _lastDirection = Vector2.down;

    // Dash state
    private bool _isDashing;
    private float _dashTimer;
    private float _dashCooldownTimer;
    private Vector2 _dashDirection;

    // Modifiers (from mutations)
    private float _speedModifier;
    private float _dashCdModifier;

    public Vector2 MoveDirection => _lastDirection;
    public Vector2 Position => _rb.position;
    public bool IsDashing => _isDashing;
    public float DashCooldownRatio => Mathf.Clamp01(_dashCooldownTimer / EffectiveDashCooldown);

    private float EffectiveSpeed => _baseMoveSpeed * (1f + Mathf.Clamp(_speedModifier, -0.5f, 2f));
    private float EffectiveDashCooldown => Mathf.Max(_baseDashCooldown * (1f - _dashCdModifier), 0.5f);

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        GameEvents.OnMutationSelected += HandleMutation;
    }

    private void OnDisable()
    {
        GameEvents.OnMutationSelected -= HandleMutation;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        // Input
        _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (_moveInput.sqrMagnitude > 0.01f)
            _lastDirection = _moveInput;

        // Dash input
        _dashCooldownTimer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space) && _dashCooldownTimer <= 0f && !_isDashing)
        {
            StartDash();
        }

        // Dash timer
        if (_isDashing)
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f)
                EndDash();
        }
    }

    private void FixedUpdate()
    {
        if (_isDashing)
        {
            _rb.linearVelocity = _dashDirection * (_dashDistance / _dashDuration);
        }
        else
        {
            _rb.linearVelocity = _moveInput * EffectiveSpeed;
        }
    }

    private void StartDash()
    {
        _isDashing = true;
        _dashTimer = _dashDuration;
        _dashDirection = _lastDirection;
        _dashCooldownTimer = EffectiveDashCooldown;

        // Ignore enemy collisions during dash
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), true);
    }

    private void EndDash()
    {
        _isDashing = false;
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), false);
    }

    private void HandleMutation(MutationData data)
    {
        _speedModifier += data.SpeedModifier;
        _dashCdModifier += data.DashCdModifier;
    }
}
