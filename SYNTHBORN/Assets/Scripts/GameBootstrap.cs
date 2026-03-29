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
using Synthborn.Mutations;
using Synthborn.Core.Persistence;
using Synthborn.UI;

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
        [SerializeField] private int _hpOrbPoolSize = 20;

        [Header("Scene References")]
        [SerializeField] private WaveSpawner _waveSpawner;
        [SerializeField] private XPGemSpawner _xpGemSpawner;
        [SerializeField] private AutoAttackController _autoAttack;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private EntityHealth _playerHealth;
        [SerializeField] private MutationManager _mutationManager;
        [SerializeField] private MutationSelectionUI _mutationUI;
        [SerializeField] private MutationDatabase _mutationDatabase;
        [SerializeField] private SynergyManager _synergyManager;
        [SerializeField] private SpriteCompositor _spriteCompositor;

        private void Awake()
        {
            // Force collision matrix — Library rebuild can reset these
            int player = LayerMask.NameToLayer("Player");       // 6
            int enemy = LayerMask.NameToLayer("Enemy");         // 7
            int projectile = LayerMask.NameToLayer("Projectile"); // 8
            int wall = LayerMask.NameToLayer("Wall");           // 9
            int xpGem = LayerMask.NameToLayer("XPGem");         // 10
            int enemyProj = LayerMask.NameToLayer("EnemyProjectile"); // 11

            Physics2D.IgnoreLayerCollision(player, projectile, true);
            Physics2D.IgnoreLayerCollision(player, player, true);
            Physics2D.IgnoreLayerCollision(enemy, enemy, true);
            Physics2D.IgnoreLayerCollision(enemy, xpGem, true);
            Physics2D.IgnoreLayerCollision(projectile, projectile, true);
            Physics2D.IgnoreLayerCollision(projectile, wall, true);
            Physics2D.IgnoreLayerCollision(projectile, xpGem, true);
            Physics2D.IgnoreLayerCollision(xpGem, wall, true);
            Physics2D.IgnoreLayerCollision(xpGem, xpGem, true);

            // Enemy projectile: hits Player, ignores Enemy/Projectile/XPGem/Wall
            if (enemyProj >= 0)
            {
                Physics2D.IgnoreLayerCollision(enemyProj, enemy, true);
                Physics2D.IgnoreLayerCollision(enemyProj, projectile, true);
                Physics2D.IgnoreLayerCollision(enemyProj, xpGem, true);
                Physics2D.IgnoreLayerCollision(enemyProj, wall, true);
                Physics2D.IgnoreLayerCollision(enemyProj, enemyProj, true);
                // Player <-> EnemyProjectile = COLLIDE (default)
            }

            var stats = new CombatStatBlock();

            // S10-07: Apply persistent upgrades at run start
            UpgradeManager.ApplyToStats(stats);

            // Apply starter form stats
            var forms = Resources.FindObjectsOfTypeAll<StarterFormData>();
            int formIndex = SaveManager.Data.selectedStarterForm;
            if (forms != null && formIndex >= 0 && formIndex < forms.Length)
                forms[formIndex].ApplyToStats(stats);

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

            // Wire MutationManager
            if (_mutationManager != null)
                _mutationManager.Initialize(stats);

            // Wire SynergyManager
            if (_synergyManager != null && _mutationManager != null)
                _synergyManager.Initialize(_mutationManager, stats);

            // Wire SpriteCompositor
            if (_spriteCompositor != null && _mutationManager != null && _mutationDatabase != null)
                _spriteCompositor.Initialize(_mutationManager, _mutationDatabase);

            // Wire MutationSelectionUI
            if (_mutationUI != null && _mutationManager != null && _mutationDatabase != null)
                _mutationUI.Initialize(_mutationManager, _mutationDatabase);

            // Wire gold drop on enemy death
            GoldManager.ResetRun();
            GameEvents.OnEnemyDied += (pos, enemy, xp) =>
            {
                var brain = enemy != null ? enemy.GetComponent<Synthborn.Enemies.EnemyBrain>() : null;
                if (brain?.Data != null)
                {
                    int tierIndex = (int)brain.Data.Tier;
                    int gold = GoldManager.RollGoldDrop(tierIndex);
                    GoldManager.AddGold(gold);
                }
            };

            // Wire player heal event
            if (_playerHealth != null)
            {
                var phForHeal = _playerHealth;
                GameEvents.OnPlayerHealRequested += (fraction) => phForHeal.HealFraction(fraction);
            }

            // Wire Player
            if (_playerController != null)
                _playerController.Initialise(stats);

            if (_playerHealth != null)
                _playerHealth.InitialiseAsPlayer(stats);
        }

        private void OnDestroy()
        {
            GameEvents.Cleanup();
        }
    }
}
