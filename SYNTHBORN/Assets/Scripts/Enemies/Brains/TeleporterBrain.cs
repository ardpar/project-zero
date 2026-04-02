using System.Collections;
using UnityEngine;
using Synthborn.Core.Pool;
using Synthborn.Core.Events;
using Synthborn.Core.Data;

namespace Synthborn.Enemies
{
    /// <summary>
    /// Three-state brain: Chase → Teleporting → (Chase) → Dead.
    ///
    /// Behaviour:
    ///   Chase       — moves toward the player until within TeleportRange.
    ///   Teleporting — blinks to a random position near the player,
    ///                 deals burst damage on arrival. Short cooldown before next blink.
    ///   Dead        — base class handles pool return.
    ///
    /// Design intent: unpredictable positioning, forces awareness.
    /// Player must watch for teleport VFX cue and dodge.
    /// </summary>
    public class TeleporterBrain : EnemyBrain
    {
        [Header("Teleporter Config")]
        [SerializeField] private float _teleportRange = 5f;
        [SerializeField] private float _blinkRadius = 2f;
        [SerializeField] private float _blinkCooldown = 2f;
        [SerializeField] private int _blinkDamage = 8;
        [SerializeField] private float _blinkDamageRadius = 1.5f;

        private float _blinkTimer;

        public override void Initialize(Transform player, int waveNumber, ObjectPool<EnemyBrain> pool, EnemyData overrideData = null)
        {
            _blinkTimer = _blinkCooldown * 0.5f; // First blink comes faster
            base.Initialize(player, waveNumber, pool, overrideData);
        }

        protected override void EnterState(EnemyState newState)
        {
            switch (newState)
            {
                case EnemyState.Teleporting:
                    rb.linearVelocity = Vector2.zero;
                    break;
            }
        }

        protected override void Tick()
        {
            switch (CurrentState)
            {
                case EnemyState.Chase:
                    TickChase();
                    break;
                case EnemyState.Teleporting:
                    TickTeleporting();
                    break;
            }
        }

        private void TickChase()
        {
            float sqrDist = SqrDistanceToPlayer();
            if (sqrDist <= _teleportRange * _teleportRange)
            {
                _blinkTimer += Time.fixedDeltaTime;
                if (_blinkTimer >= _blinkCooldown)
                {
                    SetState(EnemyState.Teleporting);
                    return;
                }
            }

            ChasePlayer();
            TickContactDamage();
        }

        private void TickTeleporting()
        {
            // Blink to random position near player
            if (playerTransform == null) { SetState(EnemyState.Chase); return; }

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _blinkRadius;
            Vector2 blinkTarget = (Vector2)playerTransform.position + offset;

            rb.position = blinkTarget;
            _blinkTimer = 0f;

            // Burst damage on arrival
            if (playerTransform != null)
            {
                float dist = ((Vector2)playerTransform.position - rb.position).sqrMagnitude;
                if (dist <= _blinkDamageRadius * _blinkDamageRadius)
                {
                    var info = new DamageInfo(_blinkDamage, DamageSource.EnemyContact, false, rb.position);
                    GameEvents.RaisePlayerDamageRequested(info);
                }
            }

            SetState(EnemyState.Chase);
        }

        private float SqrDistanceToPlayer()
        {
            if (playerTransform == null) return float.MaxValue;
            return (rb.position - (Vector2)playerTransform.position).sqrMagnitude;
        }
    }
}
