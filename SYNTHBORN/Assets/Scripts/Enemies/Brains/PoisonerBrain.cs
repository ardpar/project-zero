using UnityEngine;
using Synthborn.Core.Pool;

namespace Synthborn.Enemies
{
    /// <summary>
    /// Two-state brain: Chase → Dead.
    /// Like ChaserBrain but drops poison trail segments while moving.
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

        public override void Initialize(Transform player, int waveNumber, ObjectPool<EnemyBrain> pool, EnemyData overrideData = null)
        {
            _trailTimer = 0f;
            base.Initialize(player, waveNumber, pool, overrideData);
            _poisonerData = data as PoisonerData;
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

            var segment = Instantiate(_poisonerData.TrailPrefab, transform.position, Quaternion.identity);

            // Configure the trail segment
            var trail = segment.GetComponent<PoisonTrailSegment>();
            if (trail != null)
            {
                trail.Initialize(_poisonerData.TrailDuration, _poisonerData.TrailDamagePerSecond, _poisonerData.TrailRadius);
            }
            else
            {
                // Fallback: destroy after duration
                Destroy(segment, _poisonerData.TrailDuration);
            }
        }
    }
}
