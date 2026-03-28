using UnityEngine;
using Synthborn.Core.Pool;
using Synthborn.Core.Events;

namespace Synthborn.Progression
{
    /// <summary>
    /// Listens for enemy death events and spawns XP gems from the pool.
    /// </summary>
    public class XPGemSpawner : MonoBehaviour
    {
        [SerializeField] private XPManager _xpManager;
        [SerializeField] private Transform _playerTransform;

        private ObjectPool<XPGem> _gemPool;

        /// <summary>Inject pool at runtime (called by GameBootstrap).</summary>
        public void SetPool(ObjectPool<XPGem> pool) => _gemPool = pool;

        private void OnEnable()
        {
            GameEvents.OnEnemyDied += HandleEnemyDied;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemyDied -= HandleEnemyDied;
        }

        private void HandleEnemyDied(Vector2 position, GameObject enemy, int xpValue)
        {
            var gem = _gemPool.Get();
            gem.transform.position = position;
            gem.SetPool(_gemPool);
            gem.SetXPManager(_xpManager);
            gem.Init(xpValue, _playerTransform);
        }
    }
}
