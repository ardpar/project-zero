using UnityEngine;

namespace Synthborn.Core
{
    /// <summary>
    /// Orthographic camera: SmoothDamp follow with look-ahead,
    /// screen shake API, and arena boundary confinement.
    /// Attach to the Main Camera GameObject.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        // ─────────────────────────────────────────────
        // Config
        // ─────────────────────────────────────────────

        [Header("Follow")]
        [SerializeField] private Transform _target;
        [Tooltip("SmoothDamp time constant. Lower = snappier.")]
        [SerializeField, Range(0.01f, 0.5f)] private float _smoothTime = 0.12f;

        [Header("Look-Ahead")]
        [Tooltip("How far ahead of movement direction the camera shifts.")]
        [SerializeField, Range(0f, 3f)] private float _lookAheadDistance = 1.5f;
        [Tooltip("SmoothDamp time for look-ahead blend.")]
        [SerializeField, Range(0.05f, 1f)] private float _lookAheadSmooth = 0.3f;

        [Header("Arena Confine")]
        [Tooltip("Half-size of the arena in units. Camera won't show beyond these bounds.")]
        [SerializeField] private Vector2 _arenaHalfSize = new(15f, 15f);

        [Header("Shake")]
        [Tooltip("Default shake intensity in units.")]
        [SerializeField, Range(0.05f, 1f)] private float _defaultShakeIntensity = 0.15f;

        // ─────────────────────────────────────────────
        // Runtime
        // ─────────────────────────────────────────────

        private Vector3 _velocity;
        private Vector2 _lookAheadOffset;
        private Vector2 _lookAheadVelocity;
        private Vector2 _previousTargetPos;
        private bool _hasPreviousPos;
        private float _shakeTimer;
        private float _shakeIntensity;
        private Camera _cam;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (_target == null || Time.timeScale == 0f) return;

            Vector2 targetPos = _target.position;

            // Look-ahead: use target's actual movement delta (not camera-to-target)
            Vector2 desiredLookAhead = Vector2.zero;
            if (_hasPreviousPos)
            {
                Vector2 targetDelta = targetPos - _previousTargetPos;
                if (targetDelta.sqrMagnitude > 0.0001f)
                    desiredLookAhead = targetDelta.normalized * _lookAheadDistance;
            }
            _previousTargetPos = targetPos;
            _hasPreviousPos = true;

            _lookAheadOffset = Vector2.SmoothDamp(
                _lookAheadOffset, desiredLookAhead,
                ref _lookAheadVelocity, _lookAheadSmooth);

            Vector2 desiredPos = targetPos + _lookAheadOffset;

            // Arena confine: clamp so camera edges stay within arena bounds
            if (_cam != null && _cam.orthographic)
            {
                float camHeight = _cam.orthographicSize;
                float camWidth = camHeight * _cam.aspect;

                desiredPos.x = Mathf.Clamp(desiredPos.x, -_arenaHalfSize.x + camWidth, _arenaHalfSize.x - camWidth);
                desiredPos.y = Mathf.Clamp(desiredPos.y, -_arenaHalfSize.y + camHeight, _arenaHalfSize.y - camHeight);
            }

            // SmoothDamp
            Vector3 target3D = new Vector3(desiredPos.x, desiredPos.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, target3D, ref _velocity, _smoothTime);

            // Shake
            if (_shakeTimer > 0f)
            {
                _shakeTimer -= Time.unscaledDeltaTime;
                Vector2 offset = Random.insideUnitCircle * _shakeIntensity;
                transform.position += (Vector3)offset;
            }
        }

        // ─────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────

        /// <summary>Trigger screen shake with default intensity.</summary>
        public void TriggerShake(float duration = 0.15f)
        {
            _shakeTimer = duration;
            _shakeIntensity = _defaultShakeIntensity;
        }

        /// <summary>Trigger screen shake with custom intensity.</summary>
        public void TriggerShake(float duration, float intensity)
        {
            _shakeTimer = duration;
            _shakeIntensity = intensity;
        }

        /// <summary>Set follow target at runtime.</summary>
        public void SetTarget(Transform target) => _target = target;
    }
}
