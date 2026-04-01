using UnityEngine;
using Synthborn.Core.Events;
using Synthborn.Core.Persistence;
using Synthborn.Core.Stats;

namespace Synthborn.Progression
{
    /// <summary>
    /// Manages run-içi adaptation points.
    /// On level-up: awards points. Player allocates via UI.
    /// On death/run-end: resets all points and stat contributions.
    ///
    /// Five parameters (expansion-vision Sistem 6):
    ///   MASS (0)      → DamageModifier
    ///   RESILIENCE (1) → HpModifier
    ///   VELOCITY (2)   → SpeedModifier
    ///   VARIANCE (3)   → CritChance
    ///   YIELD (4)      → XP gain multiplier (applied via XPGem scaling)
    /// </summary>
    public class AdaptationPointManager : MonoBehaviour
    {
        [SerializeField] private AdaptationConfig _config;

        private CombatStatBlock _statBlock;
        private int _unspentPoints;
        private readonly int[] _allocatedPoints = new int[5];

        /// <summary>Parameter indices matching Arena terminology.</summary>
        public const int MASS = 0;
        public const int RESILIENCE = 1;
        public const int VELOCITY = 2;
        public const int VARIANCE = 3;
        public const int YIELD = 4;

        /// <summary>Display names for each parameter.</summary>
        public static readonly string[] ParameterNames = { "MASS", "RESILIENCE", "VELOCITY", "VARIANCE", "YIELD" };

        /// <summary>Number of unspent adaptation points available.</summary>
        public int UnspentPoints => _unspentPoints;

        /// <summary>Points allocated to each parameter (indexed by constant).</summary>
        public int GetAllocated(int param) => _allocatedPoints[param];

        /// <summary>Total stat value for a parameter (allocated * per-point value).</summary>
        public float GetStatValue(int param)
        {
            if (_config == null) return 0f;
            return param switch
            {
                MASS => _allocatedPoints[MASS] * _config.massPerPoint,
                RESILIENCE => _allocatedPoints[RESILIENCE] * _config.resiliencePerPoint,
                VELOCITY => _allocatedPoints[VELOCITY] * _config.velocityPerPoint,
                VARIANCE => _allocatedPoints[VARIANCE] * _config.variancePerPoint,
                YIELD => _allocatedPoints[YIELD] * _config.yieldPerPoint,
                _ => 0f
            };
        }

        /// <summary>XP gain multiplier from YIELD points. Applied by XPGem.</summary>
        public float XPGainMultiplier => 1f + GetStatValue(YIELD);

        /// <summary>Inject CombatStatBlock reference (called by GameBootstrap or player setup).</summary>
        public void Initialize(CombatStatBlock statBlock)
        {
            _statBlock = statBlock;
            ResetAll();
        }

        private void OnEnable()
        {
            GameEvents.OnLevelUp += OnLevelUp;
            GameEvents.OnPlayerDied += OnRunEnd;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelUp -= OnLevelUp;
            GameEvents.OnPlayerDied -= OnRunEnd;
        }

        private void OnLevelUp(int newLevel)
        {
            int points = _config != null ? _config.pointsPerLevel : 1;
            _unspentPoints += points;
            GameEvents.RaiseAdaptationPointsAwarded(_unspentPoints);
        }

        /// <summary>
        /// Allocate one point to a parameter. Returns true if successful.
        /// Immediately applies the stat modification to CombatStatBlock.
        /// </summary>
        public bool AllocatePoint(int paramIndex)
        {
            if (_unspentPoints <= 0) return false;
            if (paramIndex < 0 || paramIndex > 4) return false;
            if (_config == null) return false;

            _unspentPoints--;
            _allocatedPoints[paramIndex]++;

            // Apply stat change to CombatStatBlock
            ApplyStatDelta(paramIndex, 1);

            GameEvents.RaiseAdaptationPointAllocated(paramIndex, _allocatedPoints[paramIndex], _unspentPoints);
            return true;
        }

        /// <summary>Reset all adaptation points and remove their stat contributions.</summary>
        public void ResetAll()
        {
            // Remove all current stat contributions
            for (int i = 0; i < 5; i++)
            {
                if (_allocatedPoints[i] > 0)
                    ApplyStatDelta(i, -_allocatedPoints[i]);
                _allocatedPoints[i] = 0;
            }
            _unspentPoints = 0;
        }

        private void OnRunEnd()
        {
            ResetAll();
            // Clear adaptation data from save on death
            SaveToCharacter();
        }

        // ─── Save / Load Integration ───

        /// <summary>
        /// Save current adaptation state to CharacterSaveData.
        /// Called during calibration interval save.
        /// </summary>
        public void SaveToCharacter()
        {
            var ch = SaveManager.Character;
            if (ch == null) return;
            ch.adaptationUnspentPoints = _unspentPoints;
            System.Array.Copy(_allocatedPoints, ch.adaptationAllocated, 5);
        }

        /// <summary>
        /// Load adaptation state from CharacterSaveData.
        /// Called when resuming a run from save.
        /// </summary>
        public void LoadFromCharacter()
        {
            var ch = SaveManager.Character;
            if (ch == null) return;

            // Reset stats first
            ResetAll();

            _unspentPoints = ch.adaptationUnspentPoints;
            for (int i = 0; i < 5; i++)
            {
                int points = ch.adaptationAllocated[i];
                _allocatedPoints[i] = points;
                if (points > 0)
                    ApplyStatDelta(i, points);
            }
        }

        private void ApplyStatDelta(int paramIndex, int pointsDelta)
        {
            if (_statBlock == null || _config == null) return;

            switch (paramIndex)
            {
                case MASS:
                    _statBlock.ApplyMutation(damageModifier: pointsDelta * _config.massPerPoint);
                    break;
                case RESILIENCE:
                    _statBlock.ApplyMutation(hpModifier: pointsDelta * _config.resiliencePerPoint);
                    break;
                case VELOCITY:
                    _statBlock.ApplyMutation(speedModifier: pointsDelta * _config.velocityPerPoint);
                    break;
                case VARIANCE:
                    _statBlock.ApplyMutation(critChance: pointsDelta * _config.variancePerPoint);
                    break;
                case YIELD:
                    // YIELD doesn't go to CombatStatBlock — it's read directly via XPGainMultiplier
                    break;
            }
        }
    }
}
