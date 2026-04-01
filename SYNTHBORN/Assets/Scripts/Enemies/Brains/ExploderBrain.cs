using System.Collections;
using UnityEngine;
using Synthborn.Core.Pool;
using Synthborn.Core.Events;
using Synthborn.Core.Data;
using Synthborn.Combat.Health;

namespace Synthborn.Enemies
{
    /// <summary>
    /// Four-state brain: Chase → Priming → Exploding → Dead.
    ///
    /// Behaviour:
    ///   Chase    — moves toward the player. No contact damage (explosion is the threat).
    ///   Priming  — stops, spawns a ground warning circle, waits PrimeDuration (1 s default).
    ///              If killed during Priming the explosion does NOT fire (strategic reward).
    ///   Exploding— applies AoE damage to the player only (no friendly fire).
    ///              Damage falloff: explode_damage * (1 - (dist / radius) * 0.5).
    ///              After damage resolves, forces death via EntityHealth.
    ///   Dead     — base class handles pool return and event.
    ///
    /// GDD edge case: "Exploder killed by player before prime = no explosion."
    /// Enforced because HandleDeath (base) sets Dead before Exploding ever fires,
    /// and the Exploding entry only occurs from Priming's timer completing.
    /// </summary>
    public class ExploderBrain : EnemyBrain
    {
        // ------------------------------------------------------------------ //
        // Private State
        // ------------------------------------------------------------------ //

        private ExploderData _exploderData;
        private GameObject   _warningInstance;
        private Coroutine    _primeCoroutine;

        // ------------------------------------------------------------------ //
        // EnemyBrain overrides
        // ------------------------------------------------------------------ //

        /// <inheritdoc/>
        public override void Initialize(Transform player, int waveNumber, ObjectPool<EnemyBrain> pool, EnemyData overrideData = null)
        {
            _exploderData = data as ExploderData;
            if (_exploderData == null)
                Debug.LogError($"[ExploderBrain] EnemyData on {name} must be an ExploderData asset.", this);

            base.Initialize(player, waveNumber, pool, overrideData);
        }

        /// <inheritdoc/>
        public override void OnPoolReturn()
        {
            // Clean up warning circle if it somehow persists
            DestroyWarning();
            base.OnPoolReturn();
        }

        /// <inheritdoc/>
        protected override void EnterState(EnemyState newState)
        {
            switch (newState)
            {
                case EnemyState.Priming:
                    rb.linearVelocity = Vector2.zero;
                    SpawnWarning();
                    _primeCoroutine = StartCoroutine(PrimeCountdown());
                    break;

                case EnemyState.Dead:
                    // If we die while priming, stop the countdown
                    if (_primeCoroutine != null)
                    {
                        StopCoroutine(_primeCoroutine);
                        _primeCoroutine = null;
                    }
                    DestroyWarning();
                    break;
            }
        }

        /// <inheritdoc/>
        protected override void Tick()
        {
            if (_exploderData == null) return;

            switch (CurrentState)
            {
                case EnemyState.Chase:
                    TickChase();
                    break;
                // Priming and Exploding are coroutine-driven; Tick does nothing there.
            }
        }

        // ------------------------------------------------------------------ //
        // State Handlers
        // ------------------------------------------------------------------ //

        private void TickChase()
        {
            float dist = SqrDistanceToPlayer();
            if (dist <= _exploderData.ExplodeRange * _exploderData.ExplodeRange)
            {
                SetState(EnemyState.Priming);
                return;
            }

            ChasePlayer();
            // No contact damage — explosion is the only threat (GDD: no contact dmg for exploders)
        }

        private IEnumerator PrimeCountdown()
        {
            // Wait PrimeDuration, checking each frame that we haven't died
            float elapsed = 0f;
            while (elapsed < _exploderData.PrimeDuration)
            {
                elapsed += Time.deltaTime;
                yield return null;
                // If we died mid-prime (external kill), base HandleDeath already set Dead
                if (CurrentState == EnemyState.Dead) yield break;
            }

            // Prime complete — detonate
            SetState(EnemyState.Exploding);
            DestroyWarning();
            ApplyExplosionDamage();

            // Force death (despawn self)
            health.ForceKill();
        }

        // ------------------------------------------------------------------ //
        // Explosion Logic
        // ------------------------------------------------------------------ //

        private void ApplyExplosionDamage()
        {
            if (playerTransform == null) return;

            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist > _exploderData.ExplodeRadius) return;

            // Falloff formula: explode_damage * (1 - (dist / radius) * 0.5)
            // Centre = full damage, edge = 50% damage
            float falloff     = 1f - (dist / _exploderData.ExplodeRadius) * 0.5f;
            int   finalDamage = Mathf.Max(Mathf.RoundToInt(_exploderData.ExplodeDamage * falloff), 1);

            var info = new DamageInfo(finalDamage, DamageSource.EnemyExplosion, false, transform.position);
            GameEvents.RaisePlayerDamageRequested(info);
        }

        // ------------------------------------------------------------------ //
        // Warning Circle Helpers
        // ------------------------------------------------------------------ //

        private void SpawnWarning()
        {
            if (_exploderData.WarningPrefab == null) return;

            // Reuse cached instance if available; instantiate once per Exploder lifetime
            if (_warningInstance == null)
                _warningInstance = Instantiate(_exploderData.WarningPrefab, transform.position, Quaternion.identity);
            else
                _warningInstance.transform.position = transform.position;

            _warningInstance.SetActive(true);
        }

        private void DestroyWarning()
        {
            if (_warningInstance == null) return;
            _warningInstance.SetActive(false);
        }

        // ------------------------------------------------------------------ //
        // Helpers
        // ------------------------------------------------------------------ //

        private float SqrDistanceToPlayer()
        {
            if (playerTransform == null) return float.MaxValue;
            return (rb.position - (Vector2)playerTransform.position).sqrMagnitude;
        }
    }
}
