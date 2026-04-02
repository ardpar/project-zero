using UnityEngine;

namespace Synthborn.UI
{
    /// <summary>
    /// Defines when and what to show for a single tutorial hint.
    /// TutorialManager progresses through steps based on trigger events.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/UI/Tutorial Step", fileName = "TutorialStep")]
    public class TutorialStep : ScriptableObject
    {
        [Tooltip("Unique step ID for tracking completion.")]
        public string stepId;

        [Tooltip("What game event triggers this step to show.")]
        public TutorialTrigger trigger;

        [Tooltip("Hint text shown to the player (Turkish, Arena terminology).")]
        [TextArea(2, 4)]
        public string hintText;

        [Tooltip("Seconds to wait before showing this step after trigger fires.")]
        public float delayBeforeShow = 0.5f;

        [Tooltip("How long the hint stays on screen.")]
        public float displayDuration = 4f;

        [Tooltip("If true, waits for a specific action before advancing (e.g., player must dash).")]
        public bool requiresAction;

        [Tooltip("The action trigger that completes this step (only if requiresAction is true).")]
        public TutorialTrigger completionTrigger;
    }

    /// <summary>Events that can trigger or complete a tutorial step.</summary>
    public enum TutorialTrigger
    {
        /// <summary>Shows immediately on tutorial start.</summary>
        Immediate,
        /// <summary>After previous step completes.</summary>
        AfterPrevious,
        /// <summary>When first enemy appears on screen.</summary>
        FirstEnemySpawned,
        /// <summary>When player kills first enemy.</summary>
        FirstKill,
        /// <summary>When player collects first XP gem.</summary>
        FirstXPCollected,
        /// <summary>When player levels up for the first time.</summary>
        FirstLevelUp,
        /// <summary>When mutation selection screen appears.</summary>
        MutationScreenOpened,
        /// <summary>When first wave is cleared.</summary>
        FirstWaveCleared,
        /// <summary>When boss spawns.</summary>
        BossSpawned,
        /// <summary>When calibration interval starts.</summary>
        CalibrationStarted,
        /// <summary>When player dashes.</summary>
        PlayerDashed
    }
}
