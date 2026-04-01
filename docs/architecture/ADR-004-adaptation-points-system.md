# ADR-004: Adaptation Points System (Run-içi Stat Allocation)

## Status
Accepted

## Date
2026-04-02

## Context

### Problem Statement
SYNTHBORN's Arena mode requires a within-run stat progression layer distinct from the meta-progression (Skill Tree, Equipment) and the passive upgrade layer (Mutations). Players must feel moment-to-moment agency over their character's direction as they level up inside a run — leaning into damage, survivability, speed, critical hits, or XP acceleration depending on the enemies they face. This system is called "Adaptation Points" internally and exposed as a five-parameter allocation panel in the Calibration UI.

The decision required establishing: how parameters map to gameplay stats, where the values live (hardcoded vs. data-driven), how allocation interacts with the existing `CombatStatBlock`, how state persists mid-run across save intervals, and how the system resets cleanly on run end.

### Constraints
- Must integrate with the existing `CombatStatBlock` / `ApplyMutation()` API without requiring its modification.
- All per-point gain values must be designer-tunable without code changes (ScriptableObject).
- Must reset completely on player death — adaptation points are run-scoped, not persistent.
- Must participate in the mid-run save/load cycle used by the Calibration interval save.
- Must fire the project's EventBus (`GameEvents`) for UI decoupling; the UI must never poll the manager directly.
- Five parameters are fixed for the Arena expansion vision (Sistem 6). Extending to more parameters requires a code change — this is an accepted constraint.

### Requirements
- Five parameters: MASS (damage), RESILIENCE (HP), VELOCITY (speed), VARIANCE (crit chance), YIELD (XP gain multiplier).
- Points awarded per level-up, quantity configurable via `AdaptationConfig`.
- Allocation is one point at a time, applied immediately to `CombatStatBlock`.
- YIELD is the exception: it does not go through `CombatStatBlock`; it is read directly by `XPGemSpawner` via `XPGainMultiplier`.
- Save to `CharacterSaveData.adaptationUnspentPoints` and `CharacterSaveData.adaptationAllocated[]` on calibration interval.
- Full reset on `GameEvents.OnPlayerDied`.

## Decision

A single `AdaptationPointManager : MonoBehaviour` in `Synthborn.Progression` owns all adaptation state for the run. It is wired by `GameBootstrap` via `Initialize(CombatStatBlock)`. Per-point gain values are stored in `AdaptationConfig : ScriptableObject` — a `[SerializeField]` reference on the manager.

**Parameter indexing.** The five parameters are addressed by `public const int` indices (MASS=0 … YIELD=4) and a parallel `string[]` display name array. This avoids a `public enum` that would require explicit casting throughout the codebase; an enum can be introduced in a future refactor without API breakage.

**Immediate stat application.** `AllocatePoint(int)` decrements `_unspentPoints`, increments the relevant `_allocatedPoints[i]`, and immediately calls `_statBlock.ApplyMutation()` with the per-point delta. This keeps `CombatStatBlock` as the single authoritative runtime stat value — the adaptation manager never computes a player's final stat; it only contributes a delta.

**YIELD exception.** XP gain is not modelled in `CombatStatBlock`. `AdaptationPointManager.XPGainMultiplier` (a computed property) is read by `XPGemSpawner` when it creates each gem. This is the simplest approach given that `CombatStatBlock` has no XP field and adding one would be premature generalization.

**Reset-by-delta, not reset-by-zeroing.** `ResetAll()` calls `ApplyStatDelta(i, -_allocatedPoints[i])` for each parameter before zeroing the array. This ensures `CombatStatBlock` is left in the same state as if adaptation points were never spent — no accumulated float drift from zero-then-reapply.

**EventBus integration.** The manager subscribes to `GameEvents.OnLevelUp` and `GameEvents.OnPlayerDied` in `OnEnable`/`OnDisable`. It raises `GameEvents.RaiseAdaptationPointsAwarded` and `GameEvents.RaiseAdaptationPointAllocated` after state changes. The UI listens to these events; it never reads the manager directly.

### Architecture Diagram

```
GameBootstrap
    └─ Initialize(CombatStatBlock) ──────────────► AdaptationPointManager
                                                         │
                                        ┌────────────────┼────────────────────┐
                                        │                │                    │
                                  [SerializeField]   EventBus           SaveManager
                                  AdaptationConfig   (subscribe)        (read/write
                                  (SO asset)         OnLevelUp           CharacterSave
                                                     OnPlayerDied)       Data)
                                        │
                                  per-point gains:
                                  massPerPoint
                                  resiliencePerPoint
                                  velocityPerPoint
                                  variancePerPoint
                                  yieldPerPoint
                                  pointsPerLevel

AdaptationPointManager
    AllocatePoint(paramIndex) ──────► ApplyStatDelta() ──► CombatStatBlock.ApplyMutation()
    ResetAll()                ──────► ApplyStatDelta(-n) ── (undo all contributions)
    XPGainMultiplier          ──────► read by XPGemSpawner (YIELD bypass)
    SaveToCharacter()         ──────► CharacterSaveData.adaptationUnspentPoints / adaptationAllocated[]
    LoadFromCharacter()       ──────► ResetAll() + re-apply from save data
```

### Key Interfaces

```csharp
// Public API surface of AdaptationPointManager
public class AdaptationPointManager : MonoBehaviour
{
    public const int MASS = 0, RESILIENCE = 1, VELOCITY = 2, VARIANCE = 3, YIELD = 4;
    public static readonly string[] ParameterNames;

    public void Initialize(CombatStatBlock statBlock);
    public bool AllocatePoint(int paramIndex);   // returns false if no unspent points
    public void ResetAll();
    public void SaveToCharacter();
    public void LoadFromCharacter();

    public int UnspentPoints { get; }
    public int GetAllocated(int param);
    public float GetStatValue(int param);
    public float XPGainMultiplier { get; }       // 1f + YIELD contribution
}

// Data contract (ScriptableObject)
// Assets/Resources/Synthborn/Progression/AdaptationConfig.asset
public class AdaptationConfig : ScriptableObject
{
    public float massPerPoint;        // default 0.05  (+5% damage per point)
    public float resiliencePerPoint;  // default 0.06  (+6% HP per point)
    public float velocityPerPoint;    // default 0.04  (+4% speed per point)
    public float variancePerPoint;    // default 0.02  (+2% crit per point)
    public float yieldPerPoint;       // default 0.08  (+8% XP multiplier per point)
    public int   pointsPerLevel;      // default 1
}
```

## Alternatives Considered

### Alternative 1: Flat Stat Bonuses Stored in CombatStatBlock Directly
- **Description**: Expose adaptation fields directly on `CombatStatBlock` (e.g., `AdaptationDamageBonus`) and have the Calibration UI write to them.
- **Pros**: No extra manager class; single stat source.
- **Cons**: `CombatStatBlock` accumulates responsibilities (it becomes aware of the progression system). UI directly writing to a stats struct violates the separation established in ADR-002. Reset logic becomes entangled with the stat block.
- **Rejection Reason**: Breaks the single-responsibility principle for `CombatStatBlock`. The manager pattern keeps adaptation logic isolated and testable.

### Alternative 2: Enum-Based Parameter Addressing
- **Description**: Use a `public enum AdaptationParam { MASS, RESILIENCE, VELOCITY, VARIANCE, YIELD }` instead of `const int` indices.
- **Pros**: Type-safe; impossible to pass an out-of-range index.
- **Cons**: All callers must cast to `int` when indexing `_allocatedPoints[]` or use `switch` on the enum, which is equivalent verbosity. The `const int` approach with a bounds check (`paramIndex < 0 || paramIndex > 4`) achieves the same safety.
- **Rejection Reason**: Not rejected outright — this is a valid future refactor. Deferred because the `const int` pattern is already consistent with the existing codebase and the UI is already implemented against it. Listed as a Suggestion.

### Alternative 3: Persist Adaptation Points Across Runs (Semi-Permanent)
- **Description**: Partially or fully preserve allocated points between runs, treating them as lightweight meta-progression.
- **Pros**: Reduces early-run friction; makes a longer unlocking arc.
- **Cons**: Contradicts the core roguelite design principle that each run starts from scratch. Conflicts with the game design document for Arena mode.
- **Rejection Reason**: Game design decision — not a technical trade-off. Adaptation points are explicitly run-scoped. The reset-on-death behavior is the intended design.

### Alternative 4: Event-Driven Allocation with Command Objects
- **Description**: Wrap each allocation in a command object (Command pattern) to support undo/reallocation.
- **Pros**: Enables a proper "reallocate" feature (move a point from MASS to VELOCITY).
- **Cons**: Significantly more complex; the Arena calibration UI does not require point reallocation, only a full reset via SIFIRLA.
- **Rejection Reason**: Over-engineered for current requirements. If a partial-reallocation feature is added in a future sprint, the command pattern can be introduced then.

## Consequences

### Positive
- All balance values (per-point gains, points per level) are in one ScriptableObject, editable in the Unity Inspector without recompile.
- The EventBus contract means the Calibration UI can be developed, replaced, or tested independently of the manager.
- `ResetAll()` using negative deltas guarantees `CombatStatBlock` consistency regardless of how many points were allocated — no need for a separate "undo stack."
- Save/load integration is self-contained: `SaveToCharacter()` and `LoadFromCharacter()` touch only their own fields in `CharacterSaveData`.

### Negative
- The five-parameter array (`_allocatedPoints[5]`) is a fixed-size structure. Adding a sixth parameter requires: a new `const int`, a new field in `AdaptationConfig`, a new case in `GetStatValue()` and `ApplyStatDelta()`, and a save-data migration. This is a deliberate trade-off against the complexity of a fully generic parameter system.
- YIELD's bypass of `CombatStatBlock` is an architectural exception. If a future system needs to query total XP gain modifier from the stat block, this exception will need to be resolved (add an XP field to `CombatStatBlock` or introduce a unified multiplier query).
- `SaveManager.Character` is accessed as a static property inside `SaveToCharacter()` and `LoadFromCharacter()`. This is a coupling to the persistence layer that cannot be mocked in unit tests without a seam.

### Risks
- **Float accumulation drift on repeated resets.** `ApplyStatDelta` uses `float` arithmetic; over many runs in one session, accumulated floating-point error in `CombatStatBlock` is theoretically possible. Mitigation: `ResetAll()` applies a precise negative delta equal to the integer point count times the config float — the same multiplication used to apply. Drift is sub-epsilon over any realistic session length.
- **`_config == null` silent fallback.** `GetStatValue()` and `AllocatePoint()` return 0/false silently if `_config` is unassigned. This can mask a missing asset reference in a misconfigured scene. Mitigation: Add a `Debug.LogError` warning in `Initialize()` if `_config` is null.
- **Save-data array size mismatch.** `CharacterSaveData.adaptationAllocated` is assumed to be length 5. If save data from a version with fewer parameters is loaded, `System.Array.Copy` will copy only the available count without error, but the rest will be zero-initialized — safe but silent. Mitigation: Document the fixed length contract in `CharacterSaveData`.

## Performance Implications
- **CPU**: `AllocatePoint()` is player-triggered (UI button press), not in an Update loop. `ApplyMutation()` on `CombatStatBlock` is O(1). No performance concern.
- **Memory**: `_allocatedPoints` is a 5-element `int[]` allocated at class instantiation. Negligible.
- **Load Time**: `LoadFromCharacter()` runs once at run start; it loops 5 times and calls `ApplyStatDelta` up to 5 times. Negligible.
- **Network**: Not applicable.

## Migration Plan
The system is fully implemented. No migration is required. If a sixth parameter is added in a future expansion:
1. Extend `AdaptationConfig` with the new per-point field.
2. Add the `const int` index and `ParameterNames` entry.
3. Add a case to `GetStatValue()` and `ApplyStatDelta()`.
4. Extend `CharacterSaveData.adaptationAllocated` to length 6 with a save-migration guard.
5. Update `AdaptationPointUI` to display the new parameter.

## Validation Criteria
- `AllocatePoint()` returns `false` when `_unspentPoints == 0`; `CombatStatBlock` is unchanged.
- After allocating N points to MASS, `CombatStatBlock.DamageModifier` increases by `N * massPerPoint`.
- After `ResetAll()`, all `_allocatedPoints[]` entries are 0 and `CombatStatBlock` stats equal their pre-allocation baseline.
- `LoadFromCharacter()` after `SaveToCharacter()` produces identical `_unspentPoints` and `_allocatedPoints[]` values.
- `XPGainMultiplier` returns `1.08f` after allocating one YIELD point with default config.

## Related Decisions
- ADR-001: Core Architecture Patterns — EventBus and ScriptableObject patterns that this system conforms to.
- ADR-002: MVP Combat Systems Architecture — defines `CombatStatBlock.ApplyMutation()` which this system calls.
- Design document: `design/gdd/adaptation-points.md` (if exists) — authoritative source for the five-parameter design and Arena terminology.
