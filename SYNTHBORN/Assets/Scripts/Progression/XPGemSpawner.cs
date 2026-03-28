using UnityEngine;
using Synthborn.Core;
using Synthborn.Enemies;

namespace Synthborn.Progression
{
    /// <summary>
    /// Listens for enemy death events and spawns XP gems from the pool.
    /// </summary>
    public class XPGemSpawner : MonoBehaviour
    {
        [SerializeField] private ObjectPool<XPGem> _gemPool;
        [SerializeField] private XPManager _xpManager;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private CombatStatBlock _stats;
        private void OnEnable()
        {
            GameEvents.OnEnemyDied += HandleEnemyDied;
        }
        private void OnDisable()
            GameEvents.OnEnemyDied -= HandleEnemyDied;
        private void HandleEnemyDied(Vector2 position, EnemyData data, int xpValue)
            // Apply XP gain modifier from mutations
            float modifier = _stats != null ? _stats.XPGainModifier : 0f;
            int effectiveXP = Mathf.RoundToInt(xpValue * (1f + Mathf.Clamp(modifier, 0f, 1f)));
            var gem = _gemPool.Get(position);
            gem.SetPool(_gemPool);
            gem.SetXPManager(_xpManager);
            gem.Init(effectiveXP, _playerTransform);
    }
}
