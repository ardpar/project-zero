using System.Collections;
using UnityEngine;
using Synthborn.Core.Pool;
using Synthborn.Core.Events;
using Synthborn.Core.Data;

namespace Synthborn.Enemies
{
    /// <summary>
    /// Four-state brain: Chase → Charging → Dashing → (Chase) → Dead.
    ///
    /// Behaviour:
    ///   Chase    — moves slowly toward the player, waiting for charge cooldown.
    ///   Charging — stops, shows warning, locks dash direction for WindUpDuration.
    ///   Dashing  — moves at DashSpeedMultiplier toward locked direction for DashDuration.
    ///              Deals DashDamage on contact during dash.
    ///   Dead     — base class handles pool return.
    /// </summary>
    public class ChargerBrain : EnemyBrain
    {
        private ChargerData _chargerData;
        private float _cooldownTimer;
        private Vector2 _dashDirection;
        private Coroutine _chargeCoroutine;
        private GameObject _warningInstance;

        public override void Initialize(Transform player, int waveNumber, ObjectPool<EnemyBrain> pool, EnemyData overrideData = null)
        {
            _chargerData = data as ChargerData;
            if (_chargerData == null)
                Debug.LogError($"[ChargerBrain] EnemyData on {name} must be a ChargerData asset.", this);

            base.Initialize(player, waveNumber, pool, overrideData);
            _cooldownTimer = _chargerData != null ? _chargerData.ChargeCooldown * 0.5f : 1f;
        }

        public override void OnPoolReturn()
        {
            DestroyWarning();
            if (_chargeCoroutine != null) StopCoroutine(_chargeCoroutine);
            base.OnPoolReturn();
        }

        protected override void EnterState(EnemyState newState)
        {
            switch (newState)
            {
                case EnemyState.Charging:
                    rb.linearVelocity = Vector2.zero;
                    SpawnWarning();
                    _chargeCoroutine = StartCoroutine(WindUpSequence());
                    break;

                case EnemyState.Dead:
                    if (_chargeCoroutine != null) StopCoroutine(_chargeCoroutine);
                    DestroyWarning();
                    break;
            }
        }

        protected override void Tick()
        {
            if (_chargerData == null) return;

            switch (CurrentState)
            {
                case EnemyState.Chase:
                    TickChase();
                    break;
                case EnemyState.Dashing:
                    TickDash();
                    break;
            }
        }

        private void TickChase()
        {
            _cooldownTimer -= Time.fixedDeltaTime;

            float dist = DistanceToPlayer();
            if (_cooldownTimer <= 0f && dist <= _chargerData.ChargeRange)
            {
                SetState(EnemyState.Charging);
                return;
            }

            ChasePlayer();
        }

        private IEnumerator WindUpSequence()
        {
            // Lock direction toward player at start of wind-up
            if (playerTransform != null)
                _dashDirection = ((Vector2)playerTransform.position - rb.position).normalized;

            yield return new WaitForSeconds(_chargerData.WindUpDuration);

            if (CurrentState == EnemyState.Dead) yield break;

            DestroyWarning();
            SetState(EnemyState.Dashing);
            _chargeCoroutine = StartCoroutine(DashSequence());
        }

        private IEnumerator DashSequence()
        {
            float elapsed = 0f;
            float dashSpeed = effectiveSpeed * _chargerData.DashSpeedMultiplier;

            while (elapsed < _chargerData.DashDuration)
            {
                if (CurrentState == EnemyState.Dead) yield break;
                rb.linearVelocity = _dashDirection * dashSpeed;
                TickDashDamage();
                elapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            rb.linearVelocity = Vector2.zero;
            _cooldownTimer = _chargerData.ChargeCooldown;
            SetState(EnemyState.Chase);
        }

        private void TickDash()
        {
            // Dash movement handled by coroutine
        }

        private void TickDashDamage()
        {
            if (playerTransform == null) return;
            var playerCol = playerTransform.GetComponent<Collider2D>();
            if (playerCol == null) return;

            var result = col.Distance(playerCol);
            if (!result.isOverlapped) return;

            var info = new DamageInfo(_chargerData.DashDamage, DamageSource.EnemyContact, false, rb.position);
            GameEvents.RaisePlayerDamageRequested(info);
        }

        private float DistanceToPlayer()
        {
            if (playerTransform == null) return float.MaxValue;
            return Vector2.Distance(rb.position, (Vector2)playerTransform.position);
        }

        private void SpawnWarning()
        {
            if (_chargerData.WarningPrefab == null) return;
            _warningInstance = Instantiate(_chargerData.WarningPrefab, transform.position, Quaternion.identity);
        }

        private void DestroyWarning()
        {
            if (_warningInstance == null) return;
            Destroy(_warningInstance);
            _warningInstance = null;
        }
    }
}
