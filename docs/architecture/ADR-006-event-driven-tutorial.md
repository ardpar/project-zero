# ADR-006: Event-Driven Tutorial System

## Status
Accepted

## Date
2026-04-02

## Context

### Problem Statement
SYNTHBORN's first-run experience requires contextual hints that surface at the right moment — when the first enemy appears, when the player first levels up, when the calibration interval begins — rather than at a fixed time offset from scene load. Early in development a hard-coded coroutine in `TutorialOverlay` (the "legacy sequence") delivered four timed hints based on `WaitForSecondsRealtime`. This approach could not react to actual gameplay events, could not be extended without code changes, and could not be tested in isolation. A data-driven replacement was required that preserved the legacy path as a fallback for scenes that do not wire up the new manager.

### Constraints
- Hint content (text, trigger, timing) must be editable in the Unity Inspector without recompiling.
- The system must be non-blocking — tutorial hints never pause or gate gameplay.
- Skip support must be a first-class feature: one button press stops all hints and persists completion.
- Tutorial state must persist globally in `SaveManager.Data.tutorialCompleted` (not run-scoped). Once completed, the system is permanently inactive for that save slot.
- The legacy `TutorialOverlay` coroutine must remain runnable as a standalone fallback in scenes that do not include a `TutorialManager`.
- All timing must use unscaled time (`WaitForSecondsRealtime`, `Time.unscaledDeltaTime`) so hints display correctly if time scale is modified.

### Requirements
- Tutorial steps must be authored as ScriptableObject assets, ordered in a `TutorialStep[]` array on `TutorialManager`.
- Each step must declare what `TutorialTrigger` event activates it, and optionally a `completionTrigger` the player must perform before the sequence advances.
- `TutorialManager` must subscribe and unsubscribe from `GameEvents` in `OnEnable`/`OnDisable` — not in `Start`/`OnDestroy`.
- `TutorialOverlay` must expose a coroutine-returning `ShowHintExternal(string, float)` method so `TutorialManager` can drive it as a yield point.
- Player death must terminate the tutorial sequence immediately and mark it complete.

## Decision

Two MonoBehaviours and one ScriptableObject implement the system.

`TutorialStep : ScriptableObject` is the data atom. It carries `stepId` (string, for debugging), `trigger` (`TutorialTrigger` enum), `hintText`, `delayBeforeShow`, `displayDuration`, `requiresAction` (bool), and `completionTrigger`. No runtime logic lives here.

`TutorialTrigger` enum maps directly to the `GameEvents` the manager subscribes to. Each enum value corresponds to exactly one event handler in `TutorialManager`. This one-to-one mapping makes the data/code contract explicit and prevents designers from authoring steps that reference triggers no code handles.

`TutorialManager : MonoBehaviour` owns the step array and drives the sequence. It maintains `_currentStepIndex` (int), `_waitingForAction` (bool), and `_firedTriggers` (`HashSet<TutorialTrigger>`). The `HashSet` serves two purposes: steps that fire before their trigger event has been observed are re-checked immediately in `ProcessCurrentStep()`, and triggers that arrive while no step is waiting for them are stored so that a step added later with a matching trigger will not miss it.

`FireTrigger()` is the single routing point for all event-to-tutorial flow. It first checks if the system is waiting for a `requiresAction` completion; if so, it resolves the wait and advances. Otherwise it checks whether the current step's trigger matches. This ordering ensures completion triggers are checked before new step triggers, preventing a completion event from simultaneously triggering the next step's display in the same frame.

`ShowStep()` is a coroutine that yields `WaitForSecondsRealtime` for the delay, then yields `_overlay.ShowHintExternal()` (which itself is a coroutine). If `requiresAction` is set, the coroutine returns early after display without calling `AdvanceStep()`, leaving `_waitingForAction = true`. Advancement only happens when `FireTrigger` later receives the `completionTrigger`.

**Legacy fallback:** `TutorialOverlay.Start()` checks whether a `TutorialManager` is present in the scene. If none is found and the tutorial is not completed, it starts `LegacyTutorialSequence()` — the original four-hint timed coroutine. This preserves backward compatibility for any scene that includes `TutorialOverlay` without the full manager setup.

**Skip:** `TutorialOverlay` exposes a skip button that calls `_manager.SkipTutorial()` if a manager is present, or handles the skip inline (stop coroutines, write save flag) if in legacy mode. This means skip is always functional regardless of which code path is running.

### Architecture Diagram

```
TutorialStep[] (SO assets)
      │ [SerializeField] array on TutorialManager
      ▼
TutorialManager : MonoBehaviour
  _currentStepIndex : int
  _waitingForAction : bool
  _firedTriggers : HashSet<TutorialTrigger>
      │
      │ subscribes OnEnable/unsubscribes OnDisable
      ├──────────────────────────────────────────────────────┐
      │  GameEvents.OnWaveStarted    → FireTrigger(FirstEnemySpawned) [wave==1]
      │  GameEvents.OnEnemyDied      → FireTrigger(FirstKill)
      │  GameEvents.OnXPGemCollected → FireTrigger(FirstXPCollected)
      │  GameEvents.OnLevelUp        → FireTrigger(FirstLevelUp)
      │  GameEvents.OnWaveCleared    → FireTrigger(FirstWaveCleared)
      │  GameEvents.OnBossSpawned    → FireTrigger(BossSpawned)
      │  GameEvents.OnPlayerDashStarted → FireTrigger(PlayerDashed)
      │  GameEvents.OnCalibrationIntervalStarted → FireTrigger(CalibrationStarted)
      │  GameEvents.OnPlayerDied     → CompleteTutorial()
      └──────────────────────────────────────────────────────┘
      │
      │ drives (coroutine yield)
      ▼
TutorialOverlay : MonoBehaviour
  ShowHintExternal(text, duration) : IEnumerator   ← called by TutorialManager
  HideImmediate()                                   ← called by SkipTutorial()
  LegacyTutorialSequence() : IEnumerator            ← standalone fallback

  Skip Button → _manager.SkipTutorial()
             OR inline save (legacy path)

Save Layer:
  SaveManager.Data.tutorialCompleted = true         ← CompleteTutorial() / legacy end
  SaveManager.Save()
```

### Key Interfaces

```csharp
// Synthborn.UI — data atom
[CreateAssetMenu]
public class TutorialStep : ScriptableObject
{
    public string          stepId;
    public TutorialTrigger trigger;
    public string          hintText;
    public float           delayBeforeShow;   // seconds, unscaled
    public float           displayDuration;   // seconds, unscaled
    public bool            requiresAction;
    public TutorialTrigger completionTrigger; // only read when requiresAction == true
}

// Synthborn.UI — sequence controller
public class TutorialManager : MonoBehaviour
{
    public bool IsActive { get; }
    public void SkipTutorial();
}

// Synthborn.UI — display layer (also standalone legacy)
public class TutorialOverlay : MonoBehaviour
{
    public IEnumerator ShowHintExternal(string text, float duration);
    public void        HideImmediate();
}
```

## Alternatives Considered

### Alternative 1: Hardcoded Trigger Mapping in a Single MonoBehaviour (Original Legacy Approach)
- **Description**: Keep the original `LegacyTutorialSequence()` timed coroutine. Replace `WaitForSecondsRealtime` delays with yields on `GameEvents` using anonymous coroutines.
- **Pros**: Minimal new classes; fast to implement.
- **Cons**: Every hint text change requires recompilation. Adding a new trigger requires modifying the same class. The sequence is not independently testable. Cannot scale past ~6 hardcoded steps without the method exceeding the 40-line limit.
- **Rejection Reason**: Not maintainable for a game with a growing first-run experience. The ScriptableObject pattern allows designers to author and reorder steps without programmer involvement.

### Alternative 2: Tutorial Steps as JSON/YAML External Data
- **Description**: Store step definitions in a JSON file loaded via Addressables. `TutorialManager` parses the file at runtime.
- **Pros**: Steps are editable outside the Unity Editor (e.g., in a text editor or CMS).
- **Cons**: Introduces a serialisation layer and Addressables dependency for a system that already has a natural Unity-native data format (ScriptableObjects). The project's tooling is Inspector-centric; a JSON file removes Inspector-side previewing and validation.
- **Rejection Reason**: ScriptableObjects already provide the designer-accessible, inspector-editable, Unity-native data format. External data files are warranted for localisation or live-ops scenarios, neither of which applies to tutorial steps.

### Alternative 3: Finite State Machine with Explicit State Nodes
- **Description**: Model the tutorial as a state machine where each step is a state with entry/exit transitions declared as edges.
- **Pros**: Explicit visual representation of step transitions; supports complex branching (e.g., skip step 3 if player already dashed).
- **Cons**: Significant overengineering for a linear sequence of 6–12 steps. The `requiresAction` bool already handles the only non-linear case (wait-for-player-action). A state machine would triple the code surface for no practical gain at current scope.
- **Rejection Reason**: The `_waitingForAction` flag and `_firedTriggers` HashSet handle all required non-linearity with minimal complexity. An FSM can be introduced if branching tutorial logic is required in a future expansion.

## Consequences

### Positive
- All hint text and trigger configuration lives in ScriptableObject assets — designer-editable without code changes or recompilation.
- The `_firedTriggers` HashSet ensures that if a trigger fires before the step that needs it is reached (e.g., player levels up extremely fast), the step will show correctly when the sequence reaches it.
- `requiresAction` steps pause sequence advancement until the player performs a concrete action, enabling genuine interactive tutorials (e.g., "dash now" with `completionTrigger = PlayerDashed`).
- Legacy fallback means existing scenes continue to work without requiring `TutorialManager` to be added.
- `OnEnable`/`OnDisable` subscription pattern is safe for scenes that enable/disable the tutorial canvas during gameplay.

### Negative
- `TutorialTrigger` enum is the coupling point between step data and engine events. Adding a new trigger (e.g., `FirstSynthesisPerformed`) requires: adding the enum value, adding a `GameEvents` subscription in `TutorialManager`, and adding the handler. Designers cannot author a step for a trigger that does not yet have a corresponding code path.
- The `HashSet<TutorialTrigger>` accumulates all fired triggers for the session and is never cleared (except on tutorial completion). For a 10-step tutorial this is 10 entries at most — negligible. If tutorials grow to 50+ steps with many unique triggers, this could be revisited.
- `TutorialOverlay` retains a `FindAnyObjectByType<TutorialManager>()` call in `Awake()` as a scene-search fallback. This is a startup-only cost but is fragile if `TutorialManager` is disabled at the time of the search.

### Risks
- **Steps fire multiple times if trigger repeats.** `OnKill` fires on every enemy death. `FireTrigger(FirstKill)` is called every time. If the current step has `trigger == FirstKill`, `ShowStep()` will be started as a coroutine each time. Mitigation: `ProcessCurrentStep()` advances `_currentStepIndex` after showing, so subsequent `FirstKill` fires find the next step's trigger and do not match unless the next step also uses `FirstKill`. Functionally correct, but designers must not author two consecutive steps with the same trigger.
- **`tutorialCompleted` write on player death.** If the player dies before completing all steps, `OnPlayerDied` calls `CompleteTutorial()`, permanently marking the tutorial done. This is intentional (death implies the player has engaged with the game enough to skip the tutorial on retry) but should be confirmed with the game designer.
- **Legacy sequence and TutorialManager both active.** If `TutorialOverlay.Start()` runs `LegacyTutorialSequence()` AND a `TutorialManager` is also present, both will attempt to write to `_hintText` via `ShowHint`. Mitigation: `TutorialOverlay.Start()` guards with `if (_manager == null)` before starting the legacy sequence. Correct as long as `TutorialManager` exists in the same scene.

## Performance Implications
- **CPU**: All `FireTrigger` calls are O(1) HashSet operations plus an index comparison. Tutorial hints are shown at most once per trigger type per session; no Update loop usage.
- **Memory**: `_firedTriggers` HashSet holds at most as many entries as there are `TutorialTrigger` enum values (currently 11). Negligible.
- **Load Time**: `TutorialStep` ScriptableObjects are small (~200 bytes each). An array of 20 steps adds ~4 KB to the scene's asset load. Negligible.
- **Network**: Not applicable.

## Validation Criteria
- With `tutorialCompleted = false` and a `TutorialManager` wired with a `FirstKill` step, killing an enemy causes that step's hint to display with the correct text and duration.
- `requiresAction = true` with `completionTrigger = PlayerDashed`: hint shows, sequence does not advance until player dashes, then advances.
- Pressing Skip hides the overlay immediately, sets `tutorialCompleted = true`, and `SaveManager.Data.tutorialCompleted` persists after scene reload.
- With `tutorialCompleted = true`, `TutorialManager.Start()` exits immediately and no hints ever display.
- `TutorialOverlay` in a scene without `TutorialManager` starts `LegacyTutorialSequence()` and the four timed hints display in order.
- Player death mid-tutorial: overlay hides, `tutorialCompleted` is written, no further hints display on restart.

## Related Decisions
- ADR-001: Core Architecture Patterns — EventBus (`GameEvents`) used for all trigger subscriptions; save persistence pattern used for `tutorialCompleted`.
- ADR-008: TextMeshPro Migration Strategy — `TMP_Text` base type used for `_hintText` in `TutorialOverlay`.
