using UnityEngine;
using Synthborn.Core.Events;

namespace Synthborn.Progression
{
    /// <summary>
    /// Tracks XP accumulation and level-up logic. Fires events for UI and mutation selection.
    /// Chain level-ups supported (overflow XP carries to next level).
    /// </summary>
    public class XPManager : MonoBehaviour
    {
        [SerializeField] private XPLevelTable _levelTable;

        private int _currentXP;
        private int _level = 1;

        /// <summary>Current accumulated XP toward next level.</summary>
        public int CurrentXP => _currentXP;

        /// <summary>XP required for next level.</summary>
        public int XPToNext => _levelTable.GetXPForLevel(_level);

        /// <summary>Current player level.</summary>
        public int Level => _level;

        /// <summary>XP bar fill ratio (0-1).</summary>
        public float XPRatio => XPToNext > 0 ? (float)_currentXP / XPToNext : 0f;

        /// <summary>Add XP and trigger level-ups if thresholds are met.</summary>
        public void AddXP(int amount)
        {
            _currentXP += amount;
            GameEvents.XPChanged(_currentXP, XPToNext);

            // Chain level-up: while loop handles overflow
            while (_currentXP >= XPToNext)
            {
                _currentXP -= XPToNext;
                _level++;
                GameEvents.LevelUp(_level);
            }

            GameEvents.XPChanged(_currentXP, XPToNext);
        }

        /// <summary>Reset for new run.</summary>
        public void Reset()
        {
            _currentXP = 0;
            _level = 1;
        }
    }
}
