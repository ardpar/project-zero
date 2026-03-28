using System.Collections.Generic;
using UnityEngine;
using Synthborn.Core.Pool;
using Synthborn.Core.Stats;
using Synthborn.Core.Events;
using Synthborn.Combat.AutoAttack;
using Synthborn.Combat.Projectile;
using Synthborn.Player;

namespace Synthborn.Combat
{
    /// <summary>
    /// Manages auto-attack slots, targeting, and projectile firing.
    /// Reads stats from CombatStatBlock. Mutations add/remove slots via IAttackModifier.
    /// </summary>
    public class AutoAttackController : MonoBehaviour
    {
        [SerializeField] private AttackConfig _config;
        [SerializeField] private TargetingSystem _targeting;
        [SerializeField] private Transform _playerTransform;

        [Header("Runtime — set by PlayerController")]
        [SerializeField] private Synthborn.Player.PlayerController _playerController;

        private ObjectPool<ProjectileController> _projectilePool;

        /// <summary>Inject pool at runtime (called by GameBootstrap).</summary>
        public void SetPool(ObjectPool<ProjectileController> pool) => _projectilePool = pool;

        private CombatStatBlock _stats;
        private List<AttackSlotState> _slots = new();
        private List<AttackSlotData> _slotDatas = new();

        /// <summary>Inject CombatStatBlock reference.</summary>
        public void Initialize(CombatStatBlock stats)
        {
            _stats = stats;

            // Add default slot
            if (_config.defaultSlot != null)
                AddSlot(_config.defaultSlot);
        }

        /// <summary>Add a new attack slot (called by IAttackModifier).</summary>
        public void AddSlot(AttackSlotData slotData)
        {
            if (_slotDatas.Count >= _config.maxAttackSlots) return;

            _slotDatas.Add(slotData);
            _slots.Add(new AttackSlotState { CooldownRemaining = 0f });
        }

        /// <summary>Remove an attack slot by data reference.</summary>
        public void RemoveSlot(AttackSlotData slotData)
        {
            int idx = _slotDatas.IndexOf(slotData);
            if (idx < 0) return;

            _slotDatas.RemoveAt(idx);
            _slots.RemoveAt(idx);
        }

        private void Update()
        {
            if (Time.timeScale == 0f || _stats == null) return;

            float dt = Time.deltaTime;
            Vector2 origin = _playerTransform.position;
            Vector2 facing = _playerController != null ? _playerController.FacingDirection : Vector2.down;

            for (int i = 0; i < _slots.Count; i++)
            {
                var state = _slots[i];
                state.CooldownRemaining -= dt;

                if (state.CooldownRemaining <= 0f)
                {
                    var slotData = _slotDatas[i];
                    var target = _targeting.FindBestTarget(origin, facing, _config.attackRange, _config.targetingConeAngleDeg);

                    if (target != null)
                    {
                        FireProjectile(origin, target.position, slotData);
                        float effectiveInterval = Mathf.Max(
                            slotData.baseInterval * (1f - _stats.ClampedAttackSpeedModifier),
                            0.1f);
                        state.CooldownRemaining = effectiveInterval;
                    }
                    else
                    {
                        state.CooldownRemaining = 0f; // Don't tick cooldown without target
                    }
                }

                _slots[i] = state;
            }
        }

        private const int MaxActiveProjectiles = 300;

        private void FireProjectile(Vector2 origin, Vector2 targetPos, AttackSlotData slotData)
        {
            if (slotData.projectileData == null || _projectilePool == null) return;
            if (_projectilePool.ActiveCount >= MaxActiveProjectiles) return;

            Vector2 direction = ((Vector2)targetPos - origin).normalized;
            var proj = _projectilePool.Get();
            proj.transform.position = origin;
            proj.SetPool(_projectilePool);

            int baseDamage = Mathf.Max(
                Mathf.RoundToInt(slotData.projectileData.baseDamage * (1f + _stats.DamageModifier)), 1);
            float critChance = slotData.projectileData.baseCritChance + _stats.CritChance;
            float critMult = slotData.projectileData.baseCritMultiplier + _stats.CritMultiplierBonus;

            proj.Fire(direction, slotData.projectileData, baseDamage, critChance, critMult);
        }
    }
}
