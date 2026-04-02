# ADR-005: Lore Delivery System (Signal Archive)

## Status
Accepted

## Date
2026-04-02

## Context

### Problem Statement
SYNTHBORN's Arena mode requires a narrative layer that does not interrupt gameplay. Lore must surface organically through enemy drops, persist globally across all runs (unlike the run-scoped Adaptation Points system), and be reviewable by the player at any time between waves. The system must accommodate a growing library of fragments (100+ rooms across 6 biomes produce many drop opportunities) while ensuring biome thematic coherence — players should discover fragments that match the environment they are currently fighting through. No external narrative UI framework existed; the system needed to be built from scratch.

### Constraints
- Lore discovery must persist in `CharacterSaveData` (meta-layer), not in run-scoped save data. Once a fragment is found, it is found forever.
- Drop logic must fire from the existing `GameEvents.OnEnemyDied` event — no modifications to enemy or combat code.
- The Signal Archive UI must be fully runtime-generated; no prefab-per-entry approach can scale to 100+ fragments.
- Boss drops must always yield a new fragment when undiscovered entries remain for the current biome.
- The system must compile within `Synthborn.Lore` and `Synthborn.UI` namespaces without circular dependencies back into `Synthborn.Waves` (BiomeLayer is an enum that both assemblies can reference directly).

### Requirements
- Must support fragment authoring through ScriptableObjects with no code change per new fragment.
- Must prioritise biome-matched fragments for drops; fall back to any undiscovered fragment for guaranteed boss drops.
- Elite enemies drop with a designer-tunable probability (default 10%); standard enemies never drop.
- Discovered fragments must be saved immediately on acquisition (call `SaveManager.SaveSlot()`).
- Archive UI groups fragments by `LoreCategory` enum with undiscovered entries shown as "???" placeholders.
- A non-blocking discovery popup must display during gameplay via `GameEvents.OnLoreFragmentDiscovered`.

## Decision

Three runtime classes and two ScriptableObject types implement the system.

`LoreFragment : ScriptableObject` is the atom of the system — one asset per lore entry. It carries `fragmentId` (string, save key), `title`, `content`, `LoreCategory`, `BiomeLayer`, and `sortOrder`. No runtime logic lives in this class; it is pure data.

`LoreDatabase : ScriptableObject` is an unordered flat array of all `LoreFragment` references with three query helpers (`GetById`, `GetByBiome`, `GetByCategory`). Both `LoreDropper` and `SignalArchiveScreen` receive a reference to the same database asset via `[SerializeField]`. There is no singleton — each consumer holds its own inspector reference to the shared asset.

`LoreDropper : MonoBehaviour` subscribes to `GameEvents.OnEnemyDied`, reads `EnemyBrain.Data.Tier` to classify the kill as Boss, Elite, or standard, and runs the biome-priority drop algorithm. The current biome is queried from `TrialManager.CurrentChamber.biomeLayer` at drop time (not cached at startup) so that biome changes mid-session are always reflected. Persistence is immediate: `SaveManager.Character.signalArchiveEntries.Add(id)` followed by `SaveManager.SaveSlot()`.

`SignalArchiveScreen : MonoBehaviour` generates list UI at `Show()` time rather than at `Awake()`. For each `LoreCategory` value it creates a styled header row, then a button-entry row per fragment in that category. Undiscovered fragments render as non-interactive "???" entries. This runtime-generation strategy avoids the need to maintain a prefab library and ensures the layout always reflects the current save state at the moment the screen opens.

`LoreDiscoveryPopup : MonoBehaviour` listens to `GameEvents.OnLoreFragmentDiscovered` and runs a CanvasGroup fade-in/hold/fade-out coroutine using `Time.unscaledDeltaTime` so the popup is visible even during paused wave transitions.

**Biome-priority drop algorithm:**
1. Collect all fragments for `currentBiome` that are not in `signalArchiveEntries`.
2. If candidates exist, pick one at random.
3. If candidates are empty AND the drop is guaranteed (boss), fall back to any undiscovered fragment in the full database.
4. If no undiscovered fragments remain globally, silently skip.

### Architecture Diagram

```
GameEvents.OnEnemyDied
        │
        ▼
  LoreDropper
  (Synthborn.Lore)
        │ reads
        ├──────────────► LoreDatabase (SO asset) ──► LoreFragment[] (SO assets)
        │                 GetByBiome()
        │                 GetById()
        │
        │ queries
        ├──────────────► TrialManager.CurrentChamber.biomeLayer
        │
        │ persists
        ├──────────────► SaveManager.Character.signalArchiveEntries
        │
        │ raises
        └──────────────► GameEvents.OnLoreFragmentDiscovered
                                │
                ┌───────────────┴───────────────┐
                ▼                               ▼
      LoreDiscoveryPopup              (any future subscriber)
      (fade popup, unscaled time)

SignalArchiveScreen.Show()
        │
        ├──────────────► LoreDatabase.GetByCategory() — iterate LoreCategory enum
        │
        ├──────────────► SaveManager.Character.signalArchiveEntries (discovered check)
        │
        └──────────────► Runtime UI generation (CreateListItem)
                         Headers + Button entries with TextMeshProUGUI
```

### Key Interfaces

```csharp
// Synthborn.Lore — pure data atom
[CreateAssetMenu]
public class LoreFragment : ScriptableObject
{
    public string fragmentId;
    public string title;
    public LoreCategory category;
    public BiomeLayer biome;
    public string content;
    public int sortOrder;
}

// Synthborn.Lore — query facade over the fragment array
[CreateAssetMenu]
public class LoreDatabase : ScriptableObject
{
    public LoreFragment[] fragments;
    public LoreFragment   GetById(string id);
    public LoreFragment[] GetByBiome(BiomeLayer biome);
    public LoreFragment[] GetByCategory(LoreCategory category);
}

// Synthborn.Lore — drop controller (scene MonoBehaviour)
public class LoreDropper : MonoBehaviour
{
    // Configured via [SerializeField] _database, _eliteDropChance
    // Driven by GameEvents.OnEnemyDied
}

// Synthborn.Lore — archive UI (scene MonoBehaviour)
public class SignalArchiveScreen : MonoBehaviour
{
    public void Show();
    public void Hide();
}

// GameEvents additions
public static event Action<string, string> OnLoreFragmentDiscovered; // (fragmentId, title)
public static void RaiseLoreFragmentDiscovered(string id, string title);
```

## Alternatives Considered

### Alternative 1: Run-Scoped Lore Discovery (Reset on Death)
- **Description**: Store discovered IDs in run-scoped save data rather than `CharacterSaveData`. Players would rediscover fragments across runs.
- **Pros**: Simpler save-data schema; aligns with roguelite "fresh start" philosophy.
- **Cons**: Contradicts the narrative purpose — the Signal Archive is meant to be a persistent codex that grows over multiple runs, mirroring the player-character's accumulating knowledge of the Arena.
- **Rejection Reason**: Game design decision. Lore persistence is an explicit design requirement for the Signal Archive's narrative function.

### Alternative 2: LoreDropper Subscribes Directly to EnemyBrain (Observer on Entity)
- **Description**: Each `EnemyBrain` holds a reference to `LoreDropper` and calls it on death, rather than having `LoreDropper` listen to the global event bus.
- **Pros**: More direct coupling; easier to trace call sites.
- **Cons**: Requires modifying `EnemyBrain` or introducing a registration step for every spawned enemy. Violates the layer rule that `Synthborn.Enemies` must not depend on `Synthborn.Lore`. Coupling direction is wrong.
- **Rejection Reason**: `GameEvents.OnEnemyDied` already carries all required data (`GameObject enemy`, `Vector2 pos`, `int xp`). The event bus preserves the correct dependency direction.

### Alternative 3: Prefab-Per-Fragment UI (ScrollRect with Instantiated Prefab Rows)
- **Description**: Author a `LoreEntryRow.prefab` and instantiate one per fragment in the archive screen.
- **Pros**: Easier to style entries without code; supports animator components.
- **Cons**: With 100+ fragments and 6 category headers, the prefab approach would instantiate 100+ GameObjects every time the screen opens. Runtime generation using `new GameObject(...)` with TextMeshProUGUI achieves the same visual result with direct code control. Maintaining a prefab for a 24-pixel text row adds asset overhead without meaningful benefit.
- **Rejection Reason**: Runtime generation accepted as the simpler, more maintainable approach at current scope. A prefab-based approach would be reconsidered if entries required rich layout (icons, progress bars) that code-generated UI could not cleanly support.

## Consequences

### Positive
- New lore fragments are authored entirely in the Unity Inspector with zero code changes — create a `LoreFragment` asset, assign it to `LoreDatabase.fragments`, done.
- Biome-priority logic means players consistently encounter contextually appropriate narrative during a run, reinforcing biome identity.
- Global persistence ensures the archive grows across sessions, giving long-term players a complete record.
- `LoreDiscoveryPopup` uses `Time.unscaledDeltaTime` and `WaitForSecondsRealtime`, so it remains visible during slow-motion or pause-state wave transitions.
- `LoreDropper` has no direct dependency on any UI system — it raises an event and is done. UI can be replaced independently.

### Negative
- `LoreDatabase.GetByBiome()` and `GetByCategory()` allocate a new `List<LoreFragment>` and convert it to an array on every call. For a database of 100+ fragments, these calls during `Show()` produce GC pressure. This is acceptable since the screen opens at most once per calibration interval (not per frame).
- `SignalArchiveScreen.PopulateList()` destroys and recreates all child GameObjects on every `Show()` call. This is intentional to ensure state freshness but produces a frame spike on opening. A dirty-flag approach (only rebuild if save data changed) could reduce this cost in a future polish pass.
- `LoreDropper` uses `FindAnyObjectByType<TrialManager>()` in `Start()`. If `TrialManager` is not yet in the scene at that moment, `_trialManager` will be null and the biome fallback (Atrium) will be used silently.

### Risks
- **`fragmentId` collision.** Two fragments with the same `fragmentId` would cause both to be considered "discovered" when either is found. Mitigation: Document that `fragmentId` must be unique across all assets; consider a validation editor tool in a future sprint.
- **Save-data list grows unbounded.** `signalArchiveEntries` is a `List<string>`. At 100 fragments, each ID averages ~20 characters; the list reaches ~2 KB — negligible. If the fragment count reaches 1,000+, a `HashSet` serialised as an array would be more efficient. Not required at current scope.
- **`TrialManager` null at drop time.** If `_trialManager` is null (e.g., during a non-Arena scene), `currentBiome` defaults to `BiomeLayer.Atrium`. Boss drops will still fall through to the global fallback, so guaranteed drops remain guaranteed. The default is safe.

## Performance Implications
- **CPU**: `LoreDropper.OnEnemyDied` fires on every enemy death. The fast-path (not boss, not elite) returns in two tier comparisons with no allocation. The slow-path builds a candidate list once per drop event — O(N) over database size, called at most a handful of times per minute during typical play.
- **Memory**: Each `LoreFragment` ScriptableObject is loaded into memory when `LoreDatabase` is referenced. At ~100 fragments with predominantly string content (~500 bytes each), steady-state memory cost is approximately 50 KB. Acceptable.
- **Load Time**: No impact; `LoreDatabase` is loaded on first reference via standard Unity asset loading, not at startup.
- **Network**: Not applicable.

## Validation Criteria
- Killing a boss in Biome X with undiscovered fragments yields a fragment whose `BiomeLayer` matches Biome X.
- Killing a boss when all biome-X fragments are discovered yields a fragment from any other biome (global fallback active).
- Killing a boss when all 100+ fragments are discovered produces no drop and no error.
- Elite kills yield a fragment in approximately 10% of trials over a statistically significant sample (e.g., 100 kills, ~10 ± tolerance).
- The `signalArchiveEntries` list persists across scene reloads; a discovered fragment remains discovered after scene restart.
- `SignalArchiveScreen.Show()` displays the correct "Keşfedilen: N/M" counter matching `signalArchiveEntries.Count`.

## Related Decisions
- ADR-001: Core Architecture Patterns — EventBus pattern this system uses for both receiving (`OnEnemyDied`) and emitting (`OnLoreFragmentDiscovered`) events.
- ADR-004: Adaptation Points System — establishes the `CharacterSaveData` persistence model that lore discovery IDs join.
- ADR-008: TextMeshPro Migration Strategy — governs the `TMP_Text` / `TextMeshProUGUI` usage in `SignalArchiveScreen`'s runtime UI generation.
