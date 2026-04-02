using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthborn.Core.Events;
using Synthborn.Core.Persistence;

namespace Synthborn.UI
{
    /// <summary>
    /// Data-driven tutorial system. Progresses through TutorialStep SOs
    /// based on game events. Non-blocking overlay with skip option.
    /// Persists completion in SaveManager.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private TutorialStep[] _steps;
        [SerializeField] private TutorialOverlay _overlay;

        private int _currentStepIndex;
        private bool _active;
        private bool _waitingForAction;
        private readonly HashSet<TutorialTrigger> _firedTriggers = new();

        /// <summary>Whether the tutorial is currently running.</summary>
        public bool IsActive => _active;

        private void Start()
        {
            if (SaveManager.Data.tutorialCompleted) return;
            if (_steps == null || _steps.Length == 0) return;

            _active = true;
            _currentStepIndex = 0;
            ProcessCurrentStep();
        }

        private void OnEnable()
        {
            GameEvents.OnEnemyDied += OnKill;
            GameEvents.OnXPGemCollected += OnXPCollected;
            GameEvents.OnLevelUp += OnLevelUp;
            GameEvents.OnWaveCleared += OnWaveCleared;
            GameEvents.OnBossSpawned += OnBossSpawned;
            GameEvents.OnPlayerDashStarted += OnDash;
            GameEvents.OnCalibrationIntervalStarted += OnCalibration;
            GameEvents.OnPlayerDied += OnPlayerDied;
            GameEvents.OnWaveStarted += OnWaveStarted;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemyDied -= OnKill;
            GameEvents.OnXPGemCollected -= OnXPCollected;
            GameEvents.OnLevelUp -= OnLevelUp;
            GameEvents.OnWaveCleared -= OnWaveCleared;
            GameEvents.OnBossSpawned -= OnBossSpawned;
            GameEvents.OnPlayerDashStarted -= OnDash;
            GameEvents.OnCalibrationIntervalStarted -= OnCalibration;
            GameEvents.OnPlayerDied -= OnPlayerDied;
            GameEvents.OnWaveStarted -= OnWaveStarted;
        }

        // ─── Event Handlers ───

        private void OnWaveStarted(int wave)
        {
            if (wave == 1) FireTrigger(TutorialTrigger.FirstEnemySpawned);
        }

        private void OnKill(Vector2 p, GameObject e, int x)
        {
            FireTrigger(TutorialTrigger.FirstKill);
        }

        private void OnXPCollected(int v) => FireTrigger(TutorialTrigger.FirstXPCollected);
        private void OnLevelUp(int l) => FireTrigger(TutorialTrigger.FirstLevelUp);
        private void OnWaveCleared() => FireTrigger(TutorialTrigger.FirstWaveCleared);
        private void OnBossSpawned() => FireTrigger(TutorialTrigger.BossSpawned);
        private void OnDash(Vector2 d) => FireTrigger(TutorialTrigger.PlayerDashed);
        private void OnCalibration() => FireTrigger(TutorialTrigger.CalibrationStarted);

        private void OnPlayerDied()
        {
            if (!_active) return;
            CompleteTutorial();
        }

        // ─── Core Logic ───

        private void FireTrigger(TutorialTrigger trigger)
        {
            if (!_active) return;

            _firedTriggers.Add(trigger);

            // Check if waiting for action completion
            if (_waitingForAction && _currentStepIndex < _steps.Length)
            {
                var step = _steps[_currentStepIndex];
                if (step.requiresAction && step.completionTrigger == trigger)
                {
                    _waitingForAction = false;
                    AdvanceStep();
                    return;
                }
            }

            // Check if current step is waiting for this trigger
            if (_currentStepIndex < _steps.Length)
            {
                var step = _steps[_currentStepIndex];
                if (step.trigger == trigger)
                {
                    StartCoroutine(ShowStep(step));
                }
            }
        }

        private void ProcessCurrentStep()
        {
            if (_currentStepIndex >= _steps.Length)
            {
                CompleteTutorial();
                return;
            }

            var step = _steps[_currentStepIndex];

            if (step.trigger == TutorialTrigger.Immediate || step.trigger == TutorialTrigger.AfterPrevious)
            {
                StartCoroutine(ShowStep(step));
            }
            // Otherwise wait for the trigger event to fire
            else if (_firedTriggers.Contains(step.trigger))
            {
                StartCoroutine(ShowStep(step));
            }
        }

        private IEnumerator ShowStep(TutorialStep step)
        {
            if (step.delayBeforeShow > 0)
                yield return new WaitForSecondsRealtime(step.delayBeforeShow);

            if (_overlay != null)
                yield return _overlay.ShowHintExternal(step.hintText, step.displayDuration);

            if (step.requiresAction)
            {
                _waitingForAction = true;
                // Wait until the action trigger fires (handled in FireTrigger)
                yield break;
            }

            AdvanceStep();
        }

        private void AdvanceStep()
        {
            _currentStepIndex++;
            ProcessCurrentStep();
        }

        /// <summary>Skip the entire tutorial.</summary>
        public void SkipTutorial()
        {
            if (!_active) return;
            StopAllCoroutines();
            if (_overlay != null) _overlay.HideImmediate();
            CompleteTutorial();
        }

        private void CompleteTutorial()
        {
            _active = false;
            SaveManager.Data.tutorialCompleted = true;
            SaveManager.Save();
        }
    }
}
