using UnityEngine;
using Synthborn.Core.Pool;

namespace Synthborn.Enemies
{
    /// <summary>
    /// Two-state brain: Chase → Dead.
    /// Like ChaserBrain but drops poison trail segments while moving.
    /// Uses a shared static pool for PoisonTrailSegment to avoid per-drop allocations.
    ///
    /// Behaviour:
    ///   Chase — moves toward the player, drops poison trail every TrailInterval.
    ///           Trail segments persist for TrailDuration and deal DPS to player.
    ///   Dead  — base class handles pool return.
    /// </summary>
    public class PoisonerBrain : EnemyBrain
    {
        private PoisonerData _poisonerData;
        private float _trailTimer;

        private static ObjectPool<PoisonTrailSegment> _trailPool;
        private static bool _poolInitialized;

        public override void Initialize(Transform player, int waveNumber, ObjectPool<EnemyBrain> pool, EnemyData overrideData = null)
        {
            _trailTimer = 0f;
            base.Initialize(player, waveNumber, pool, overrideData);
            _poisonerData = data as PoisonerData;

            EnsureTrailPool();
        }

        private void EnsureTrailPool()
        {
            if (_poolInitialized && _trailPool != null) return;
            if (_poisonerData?.TrailPrefab == null) return;

            var prefab = _poisonerData.TrailPrefab;
            _trailPool = new ObjectPool<PoisonTrailSegment>(() =>
            {
                var go = Instantiate(prefab);
                go.name = "PoisonTrail_Pooled";
                var segment = go.GetComponent<PoisonTrailSegment>();
                if (segment == null) segment = go.AddComponent<PoisonTrailSegment>();
                segment.SetPool(_trailPool);
                return segment;
            }, 20);
            _poolInitialized = true;
        }

        protected override void EnterState(EnemyState newState) { }

        protected override void Tick()
        {
            if (_poisonerData == null) return;
            if (CurrentState != EnemyState.Chase) return;

            ChasePlayer();
            TickContactDamage();
            TickTrail();
        }

        private void TickTrail()
        {
            _trailTimer += Time.fixedDeltaTime;
            if (_trailTimer < _poisonerData.TrailInterval) return;
            _trailTimer = 0f;

            DropTrailSegment();
        }

        private void DropTrailSegment()
        {
            if (_poisonerData.TrailPrefab == null) return;

            PoisonTrailSegment trail;
            if (_trailPool != null)
            {
                trail = _trailPool.Get();
                trail.transform.position = transform.position;
                trail.transform.rotation = Quaternion.identity;
            }
            else
            {
                // Fallback if pool not initialized
                var segment = Instantiate(_poisonerData.TrailPrefab, transform.position, Quaternion.identity);
                trail = segment.GetComponent<PoisonTrailSegment>();
                if (trail == null) return;
            }

            trail.Initialize(_poisonerData.TrailDuration, _poisonerData.TrailDamagePerSecond, _poisonerData.TrailRadius);
        }
    }
}
