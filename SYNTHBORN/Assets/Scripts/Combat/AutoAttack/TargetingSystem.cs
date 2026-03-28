using UnityEngine;
using Synthborn.Combat.Health;

namespace Synthborn.Combat
{
    /// <summary>
    /// Finds the best attack target using cone + 360° fallback.
    /// Uses Physics2D.OverlapCircleNonAlloc for zero-allocation queries.
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
            int count = Physics2D.OverlapCircleNonAlloc(origin, range, _hitBuffer, _enemyLayer);
            if (count == 0) return null;

            Transform best = null;
            float bestScore = float.MaxValue;
            float cosThreshold = Mathf.Cos(coneAngleDeg * Mathf.Deg2Rad);

            for (int i = 0; i < count; i++)
            {
                var col = _hitBuffer[i];
                if (col == null) continue;

                var damageable = col.GetComponent<IDamageable>();
                if (damageable != null && damageable.IsDead) continue;

                Vector2 toEnemy = (Vector2)col.transform.position - origin;
                float dist = toEnemy.magnitude;
                if (dist < 0.01f) continue;

                float dot = Vector2.Dot(facing, toEnemy / dist);
                bool inCone = dot > cosThreshold;

                // Cone enemies get half effective distance (2x priority)
                float score = inCone ? dist * 0.5f : dist;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = col.transform;
                }
            }

            return best;
        }
    }
}
