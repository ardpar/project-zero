using UnityEngine;
using Synthborn.Core.Events;
using Synthborn.Core.Pool;

namespace Synthborn.Core.VFX
{
    /// <summary>
    /// Spawns floating damage numbers when damage is dealt.
    /// Uses ObjectPool to avoid per-hit allocations.
    /// </summary>
    public class DamageNumberSpawner : MonoBehaviour
    {
        [SerializeField] private int _maxNumbers = 20;

        private ObjectPool<DamageNumber> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<DamageNumber>(() =>
            {
                var go = new GameObject("DmgNum");
                go.transform.SetParent(transform);
                var dn = go.AddComponent<DamageNumber>();
                dn.SetPool(_pool);
                return dn;
            }, _maxNumbers);
        }

        private void OnEnable()
        {
            GameEvents.OnDamageDealt += SpawnNumber;
        }

        private void OnDisable()
        {
            GameEvents.OnDamageDealt -= SpawnNumber;
        }

        private void SpawnNumber(Vector2 position, int damage, bool isCrit)
        {
            if (_pool.ActiveCount >= _maxNumbers) return;

            var dn = _pool.Get();
            dn.Init(damage, isCrit, position);
        }
    }
}
