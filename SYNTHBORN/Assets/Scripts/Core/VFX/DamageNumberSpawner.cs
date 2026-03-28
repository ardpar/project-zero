using UnityEngine;
using Synthborn.Core.Events;

namespace Synthborn.Core.VFX
{
    /// <summary>
    /// Spawns floating damage numbers when damage is dealt.
    /// </summary>
    public class DamageNumberSpawner : MonoBehaviour
    {
        [SerializeField] private int _maxNumbers = 20;
        private int _activeCount;

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
            if (_activeCount >= _maxNumbers) return;

            var go = new GameObject("DmgNum");
            var dn = go.AddComponent<DamageNumber>();
            dn.Init(damage, isCrit, position);
            _activeCount++;

            // Track destruction
            Destroy(go, 1f);
        }
    }
}
