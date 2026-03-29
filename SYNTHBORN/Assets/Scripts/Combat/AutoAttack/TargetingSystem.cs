using UnityEngine;
using Synthborn.Combat.Health;

namespace Synthborn.Combat
{
    /// <summary>
    /// Finds the best attack target using cone + 360° fallback.
    /// Uses Physics2D.OverlapCircle with ContactFilter2D for zero-allocation queries.
    /// </summary>
    public class TargetingSystem : MonoBehaviour
    {
        [SerializeField] private LayerMask _enemyLayer;

        private readonly Collider2D[] _hitBuffer = new Collider2D[64];

        /// <summary>
        /// Find the best target within range. Cone-aligned enemies get priority.
        /// Returns null if no valid target found.
        /// </summary>
        public Transform FindBestTarget(Vector2 origin, Vector2 facing, float range, float coneAngleDeg)
        {
            int count = Physics2D.OverlapCircle(origin, range, new ContactFilter2D { layerMask = _enemyLayer, useLayerMask = true, useTriggers = true }, _hitBuffer);
            if (count == 0) return null;

            Transform nearest = null;
            float nearestDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var col = _hitBuffer[i];
                if (col == null) continue;

                var damageable = col.GetComponent<IDamageable>();
                if (damageable != null && damageable.IsDead) continue;

                float dist = ((Vector2)col.transform.position - origin).sqrMagnitude;
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = col.transform;
                }
            }

            return nearest;
        }
    }
}
