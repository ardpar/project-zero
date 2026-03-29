using System.Collections.Generic;
using UnityEngine;
using Synthborn.Core.Pool;
using Synthborn.Core.Events;

namespace Synthborn.Enemies
{
    /// <summary>
    /// Three-state brain: Chase → Summoning → (Chase) → Dead.
    ///
    /// Behaviour:
    ///   Chase     — moves toward the player until within SummonRange.
    ///   Summoning — stops, spawns minions at SummonInterval (max MaxMinions alive).
    ///               Returns to Chase if player escapes range.
    ///   Dead      — kills all active minions, base class handles pool return.
    /// </summary>
    public class SummonerBrain : EnemyBrain
    {
        private SummonerData _summonerData;
        private float _summonTimer;
        private readonly List<EnemyBrain> _activeMinions = new();
        private ObjectPool<EnemyBrain> _pool;

        public override void Initialize(Transform player, int waveNumber, ObjectPool<EnemyBrain> pool, EnemyData overrideData = null)
        {
            _pool = pool;
            _activeMinions.Clear();

            base.Initialize(player, waveNumber, pool, overrideData);
            _summonerData = data as SummonerData;
        }

        protected override void EnterState(EnemyState newState)
        {
            switch (newState)
            {
                case EnemyState.Summoning:
                    rb.linearVelocity = Vector2.zero;
                    _summonTimer = 0f;
                    break;

                case EnemyState.Dead:
                    KillAllMinions();
                    break;
            }
        }

        protected override void Tick()
        {
            if (_summonerData == null) return;

            // Clean up dead minions from tracking list (reverse loop to avoid lambda alloc)
            for (int i = _activeMinions.Count - 1; i >= 0; i--)
            {
                if (_activeMinions[i] == null || !_activeMinions[i].gameObject.activeSelf)
                    _activeMinions.RemoveAt(i);
            }

            switch (CurrentState)
            {
                case EnemyState.Chase:
                    TickChase();
                    break;
                case EnemyState.Summoning:
                    TickSummoning();
                    break;
            }
        }

        private void TickChase()
        {
            float dist = SqrDistanceToPlayer();
            if (dist <= _summonerData.SummonRange * _summonerData.SummonRange)
            {
                SetState(EnemyState.Summoning);
                return;
            }

            ChasePlayer();
            TickContactDamage();
        }

        private void TickSummoning()
        {
            // Return to chase if player escapes range
            float dist = SqrDistanceToPlayer();
            float escapeRange = _summonerData.SummonRange * 1.2f;
            if (dist > escapeRange * escapeRange)
            {
                SetState(EnemyState.Chase);
                return;
            }

            rb.linearVelocity = Vector2.zero;
            _summonTimer += Time.fixedDeltaTime;

            if (_summonTimer >= _summonerData.SummonInterval)
            {
                _summonTimer = 0f;
                TrySpawnMinion();
            }
        }

        private void TrySpawnMinion()
        {
            if (_activeMinions.Count >= _summonerData.MaxMinions) return;
            if (_pool == null || _summonerData.MinionData == null) return;

            // Spawn minion near summoner
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 1.5f;
            Vector2 spawnPos = rb.position + offset;

            var minion = _pool.Get();
            minion.transform.position = spawnPos;
            minion.Initialize(playerTransform, currentWave, _pool, _summonerData.MinionData);
            _activeMinions.Add(minion);
        }

        private void KillAllMinions()
        {
            foreach (var minion in _activeMinions)
            {
                if (minion != null && minion.gameObject.activeInHierarchy)
                {
                    var mHealth = minion.GetComponent<Synthborn.Combat.Health.EntityHealth>();
                    if (mHealth != null) mHealth.ForceKill();
                }
            }
            _activeMinions.Clear();
        }

        private float SqrDistanceToPlayer()
        {
            if (playerTransform == null) return float.MaxValue;
            return (rb.position - (Vector2)playerTransform.position).sqrMagnitude;
        }
    }
}
