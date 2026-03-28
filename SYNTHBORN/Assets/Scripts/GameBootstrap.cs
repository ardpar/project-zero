using UnityEngine;
using Synthborn.Core.Pool;
using Synthborn.Core.Stats;
using Synthborn.Core.Events;
using Synthborn.Combat;
using Synthborn.Combat.Health;
using Synthborn.Enemies;
using Synthborn.Player;
using Synthborn.Progression;
using Synthborn.Waves;

namespace Synthborn.Core
{
    /// <summary>
    /// Creates object pools and wires runtime dependencies that can't be set in Inspector.
    /// Attach to the GameManager GameObject. Runs before all other scripts (Script Execution Order: -100).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private GameObject _xpGemPrefab;
        [SerializeField] private GameObject _hpOrbPrefab;

        [Header("Pool Sizes")]
        [SerializeField] private int _enemyPoolSize = 50;
        [SerializeField] private int _projectilePoolSize = 100;
        [SerializeField] private int _xpGemPoolSize = 100;
        [SerializeField] private int _hpOrbPoolSize = 20;

        [Header("Scene References")]
        [SerializeField] private WaveSpawner _waveSpawner;
        [SerializeField] private XPGemSpawner _xpGemSpawner;
        [SerializeField] private AutoAttackController _autoAttack;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private EntityHealth _playerHealth;

        private void Awake()
        {
            var stats = new CombatStatBlock();

            // Create pools (no pre-warm — lazy instantiate to avoid layer/state issues)
            var enemyPool = new ObjectPool<EnemyBrain>(
                () => Instantiate(_enemyPrefab).GetComponent<EnemyBrain>(), 0);

            var projectilePool = new ObjectPool<ProjectileController>(
                () => Instantiate(_projectilePrefab).GetComponent<ProjectileController>(), 0);

            var xpGemPool = new ObjectPool<XPGem>(
                () => Instantiate(_xpGemPrefab).GetComponent<XPGem>(), 0);

            // Wire WaveSpawner
            if (_waveSpawner != null)
                _waveSpawner.SetPool(enemyPool);

            // Wire AutoAttackController
            if (_autoAttack != null)
            {
                _autoAttack.SetPool(projectilePool);
                _autoAttack.Initialize(stats);
            }

            // Wire XPGemSpawner
            if (_xpGemSpawner != null)
                _xpGemSpawner.SetPool(xpGemPool);

            // HP Orb pool + spawner
            if (_hpOrbPrefab != null)
            {
                var hpOrbPool = new ObjectPool<HPOrb>(
                    () => Instantiate(_hpOrbPrefab).GetComponent<HPOrb>(),
                    _hpOrbPoolSize);

                GameEvents.OnHPOrbRequested += (pos) =>
                {
                    var orb = hpOrbPool.Get();
                    orb.transform.position = pos;
                    orb.SetPool(hpOrbPool);
                    orb.Init(_playerController.transform, _playerHealth);
                };
            }

            // Wire player damage event — enemies fire this, EntityHealth consumes it
            if (_playerHealth != null)
            {
                var ph = _playerHealth;
                GameEvents.OnPlayerDamageRequested += (info) => ph.TakeDamage(info);
            }

            // Wire Player
            if (_playerController != null)
                _playerController.Initialise(stats);

            if (_playerHealth != null)
                _playerHealth.InitialiseAsPlayer(stats);
        }
    }
}
