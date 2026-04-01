# Performance Report -- Static Analysis / 2026-03-29

**Build**: main @ 642c689f
**Scope**: Full codebase static analysis -- all 127 game scripts under `SYNTHBORN/Assets/Scripts/`
**Method**: Source-code review (no runtime profiler data yet)
**Analyst**: performance-analyst agent

---

## Frame Time Budget: 16.6ms (60 fps target)

| Category | Budget | Actual | Status |
|----------|--------|--------|--------|
| Gameplay Logic | 4ms | Unknown -- profile required | UNVERIFIED |
| Rendering | 5ms | Unknown -- profile required | UNVERIFIED |
| Physics | 3ms | Unknown -- profile required | UNVERIFIED |
| AI | 3ms | Unknown -- profile required | UNVERIFIED |
| Audio | 1ms | Unknown -- profile required | UNVERIFIED |

> Note: Runtime budgets are not yet configured (see `technical-preferences.md`). Budgets above are
> recommended defaults. Assign actual values after first profiling session.

---

## Memory Budget

Memory ceiling is not yet configured. The issues below represent confirmed allocation hotspots that
will drive GC spikes. Assign a ceiling after MVP profiling.

---

## CRITICAL Issues

### C1 -- DeathParticleSpawner: 6+ Instantiate calls per enemy death, no pooling

**File**: `SYNTHBORN/Assets/Scripts/Core/VFX/DeathParticleSpawner.cs`
**Method**: `OnEnemyDied` (line 27) and `DeathParticle.Update` (line 73 -- `Destroy(gameObject)`)

**Problem**: Every enemy death calls `new GameObject("DeathParticle")` and `AddComponent<SpriteRenderer>()`
six times, then schedules each `DeathParticle` to `Destroy` itself. In late waves with 30-50 enemies
dying per second this is 180-300 GameObject allocations per second, each generating a GC collection
on destruction. This is the single largest allocation hotspot in the codebase.

**Fix**: Pool `DeathParticle` instances using the existing `ObjectPool<T>` infrastructure. Pre-warm 64
slots at scene start. `DeathParticleSpawner` should own the pool and check out objects in `OnEnemyDied`,
returning them after their fade completes instead of destroying them.

**Estimated impact**: HIGH. Will eliminate the most frequent GC spike source in late-wave gameplay.

---

### C2 -- DamageNumberSpawner: Instantiate per hit AND a correctness bug that permanently stops spawning

**File**: `SYNTHBORN/Assets/Scripts/Core/VFX/DamageNumberSpawner.cs`
**Method**: `SpawnNumber` (lines 25-36)
**Also**: `DamageNumber.cs` line 51 -- `Destroy(gameObject)`

**Problem (performance)**: Every damage event calls `new GameObject("DmgNum")`, `AddComponent<DamageNumber>()`,
and `Destroy(go, 1f)`. With multi-target pierce projectiles this fires dozens of times per second at
high attack speed.

**Problem (correctness bug)**: `_activeCount` is incremented on spawn (line 32) but is NEVER decremented.
`Destroy(go, 1f)` has no callback into the spawner. After exactly 20 damage numbers have ever been
spawned, `_activeCount >= _maxNumbers` is permanently true and the spawner silently stops producing any
further damage feedback. This is a gameplay-visible defect, not just a performance issue.

**Fix**: Pool `DamageNumber` using `ObjectPool<T>`. Have `DamageNumber.OnPoolReturn` clear the text and
color. Replace `_activeCount` tracking with `_pool.ActiveCount` from the pool's own counter. The
`Destroy` call becomes `_pool.Return(this)` when the lifetime timer expires.

**Estimated impact**: HIGH. Fixes both the allocation spike and the broken counter that kills damage
number feedback mid-run.

---

### C3 -- DeathBurstVFX: Runtime Shader.Find and material leak

**File**: `SYNTHBORN/Assets/Scripts/Combat/VFX/DeathBurstVFX.cs`
**Method**: `Start` (lines 38-41)

**Problem**: `Shader.Find("Particles/Standard Unlit")` is called at runtime every time a `DeathBurstVFX`
is instantiated. This string-based shader lookup is slow, fails silently (returns null) in builds where
the shader is not in the Always Included Shaders list or otherwise stripped, and `new Material(shader)`
leaks: Unity's `Destroy` at line 47 destroys the GameObject but not the dynamically created Material,
leaving a leaked asset in memory per VFX invocation.

**Fix**: Move the material reference to a `[SerializeField] Material _burstMaterial` on the component.
Assign it in the Inspector so the shader is always referenced. Remove the `Shader.Find` / `new Material`
calls entirely. If this VFX is intended to be pooled later, use `MaterialPropertyBlock` instead of
setting material per-instance.

**Estimated impact**: HIGH. Prevents silent in-build failures and eliminates per-spawn material leak.

---

## HIGH Issues

### H1 -- EnemyBrain.TickContactDamage: GetComponent every 0.5s per enemy

**File**: `SYNTHBORN/Assets/Scripts/Enemies/Brains/EnemyBrain.cs`
**Method**: `TickContactDamage` (line 230)

```csharp
var playerCol = playerTransform.GetComponent<Collider2D>(); // called 200x/sec at cap
```

**Problem**: `GetComponent<Collider2D>()` is called every time the contact-damage timer fires, which is
every 0.5 seconds per enemy. At the 200-enemy alive cap that is 400 `GetComponent` calls per second --
each one traverses the component list on the player's GameObject.

**Fix**: Cache the player's `Collider2D` in `Initialize()` alongside `playerTransform`. One call on
spawn, zero overhead each tick.

**Estimated impact**: MEDIUM-HIGH. Meaningful at max enemy counts; trivial fix.

---

### H2 -- TargetingSystem.FindBestTarget: GetComponent per overlap hit, every frame

**File**: `SYNTHBORN/Assets/Scripts/Combat/AutoAttack/TargetingSystem.cs`
**Method**: `FindBestTarget` (line 33)

```csharp
var damageable = col.GetComponent<IDamageable>(); // up to 64 calls per targeting frame
```

**Problem**: Called every `Update` frame per active attack slot (one slot minimum, potentially more with
mutations). Each call walks the collider's component list. With up to 64 enemies in the overlap buffer
and multiple attack slots this accumulates quickly.

**Fix**: Because every enemy already has an `EntityHealth` that implements `IDamageable`, cache the
`IDamageable` reference on the enemy's own component at `Awake` / `Initialize` time and expose it via a
property. Then callers can use `col.GetComponent<EnemyBrain>().Damageable` which accesses a cached
field, or use `TryGetComponent` which is faster than `GetComponent` when the component may be absent.

**Estimated impact**: MEDIUM. Noticeable when the buffer fills with enemies at mid-to-late waves.

---

### H3 -- LootDropper.GetRandomItemOfMinRarity: List allocation on every enemy kill

**File**: `SYNTHBORN/Assets/Scripts/LootDropper.cs`
**Method**: `GetRandomItemOfMinRarity` (lines 87-97)

```csharp
var candidates = new System.Collections.Generic.List<ItemData>(); // allocated per kill
```

**Problem**: A new `List<ItemData>` is allocated every time any enemy dies (the method is called from
`OnEnemyDied`). This creates continuous GC pressure proportional to kill rate.

**Fix**: Promote `candidates` to a private member field. Call `candidates.Clear()` at the start of
`GetRandomItemOfMinRarity` instead of allocating a new list. Zero-allocation hotpath.

**Estimated impact**: MEDIUM. Easy fix; proportional to kill rate.

---

### H4 -- DashTrail.SpawnGhost: Instantiate every 0.03s during dash, no pooling

**File**: `SYNTHBORN/Assets/Scripts/Player/DashTrail.cs`
**Method**: `SpawnGhost` (line 55) and `FadeAndDestroy.Update` (line 86 -- `Destroy(gameObject)`)

**Problem**: Each dash spawns a new `GameObject` every 30ms and destroys it 200ms later. A 200ms dash
creates ~6 new GameObjects. Dashes are frequent. The `FadeAndDestroy` component also allocates a new
`MonoBehaviour` per ghost.

**Fix**: Pre-pool 8-10 `DashGhost` objects (simple GameObject with SpriteRenderer and FadeAndDestroy).
Return them to the pool from `FadeAndDestroy.Update` rather than calling `Destroy`.

**Estimated impact**: LOW-MEDIUM on its own. Follows the same pattern as C1/C2; eliminating all three
unifies the codebase on the existing pool infrastructure.

---

### H5 -- PrototypeHUD.OnMutationApplied: Resources.FindObjectsOfTypeAll during gameplay

**File**: `SYNTHBORN/Assets/Scripts/UI/PrototypeHUD.cs`
**Method**: `OnMutationApplied` (line 126)

```csharp
var databases = Resources.FindObjectsOfTypeAll<MutationDatabase>(); // full asset scan
```

**Problem**: `Resources.FindObjectsOfTypeAll` scans all currently loaded assets. This is intended for
editor tooling, not runtime gameplay. Called on every mutation pickup during a run.

**Fix**: Inject `MutationDatabase` via `[SerializeField]` in the Inspector (or via `Initialize()` from
`GameBootstrap`). The reference should be set once and reused. Remove the `FindObjectsOfTypeAll` call
entirely.

**Estimated impact**: LOW in terms of frame time (mutations are infrequent), HIGH in terms of code
quality -- this pattern will cause issues if `MutationDatabase` assets multiply.

---

### H6 -- WaveSpawner.Start: FindFirstObjectByType as fallback

**File**: `SYNTHBORN/Assets/Scripts/Waves/WaveSpawner.cs`
**Method**: `Start` (line 65)

```csharp
if (FindFirstObjectByType<LevelManager>() == null)
```

**Problem**: `FindFirstObjectByType` performs a full scene search. While this is a one-time call in
`Start`, the fallback pattern means this runs on every scene load. It should be replaced with a direct
serialized reference or a null check against an injected reference from `GameBootstrap`.

**Estimated impact**: LOW at runtime. Code quality and reliability issue.

---

### H7 -- SummonerBrain.Tick: RemoveAll with lambda, every FixedUpdate frame

**File**: `SYNTHBORN/Assets/Scripts/Enemies/Brains/SummonerBrain.cs`
**Method**: `Tick` (line 53)

```csharp
_activeMinions.RemoveAll(m => m == null || !m.gameObject.activeInHierarchy);
```

**Problem**: `RemoveAll` with a lambda allocates a delegate on each call. `activeInHierarchy` traverses
the full transform hierarchy. This runs every `FixedUpdate` frame for every live Summoner enemy.

**Fix**: Iterate manually with a reverse `for` loop checking `m == null || !m.gameObject.activeSelf`
(`activeSelf` is O(1), `activeInHierarchy` is O(depth)). Cache the delegate as a static member if
`RemoveAll` is preferred.

**Estimated impact**: MEDIUM. Depends on Summoner count. The delegate allocation is the primary issue.

---

### H8 -- PoisonerBrain.DropTrailSegment: Instantiate without pooling

**File**: `SYNTHBORN/Assets/Scripts/Enemies/Brains/PoisonerBrain.cs`
**Method**: `DropTrailSegment` (line 52)

```csharp
var segment = Instantiate(_poisonerData.TrailPrefab, transform.position, Quaternion.identity);
```

**Problem**: Trail segments are instantiated at a rate of `1 / TrailInterval` per Poisoner enemy and
destroyed after `TrailDuration`. With multiple Poisoners active this creates sustained allocation churn.

**Fix**: Pool `PoisonTrailSegment` objects. Pre-warm 20-30 slots. The segment's `Update` should call
`pool.Return(this)` instead of `Destroy(gameObject)` when the lifetime expires.

**Estimated impact**: MEDIUM. Becomes HIGH if multiple Poisoners are in the same wave.

---

## MEDIUM Issues

### M1 -- HitFlash.FlashRoutine: new WaitForSeconds allocation per hit

**File**: `SYNTHBORN/Assets/Scripts/Core/VFX/HitFlash.cs`
**Method**: `FlashRoutine` (line 38)

```csharp
yield return new WaitForSeconds(_flashDuration);
```

**Problem**: Allocates a new `WaitForSeconds` object on every enemy hit. With hundreds of hits per
second across many enemies this is constant GC pressure.

**Fix**: Cache the `WaitForSeconds` instance as a private field, created once in `Awake`:
`_waitForFlash = new WaitForSeconds(_flashDuration);`. Reuse it in every `FlashRoutine` call.

**Estimated impact**: LOW-MEDIUM. Easy win with one line of change.

---

### M2 -- EnemyBrain.ReturnToPoolNextFrame: Coroutine allocation per enemy death

**File**: `SYNTHBORN/Assets/Scripts/Enemies/Brains/EnemyBrain.cs`
**Method**: `HandleDeath` (line 288) / `ReturnToPoolNextFrame` (line 291)

**Problem**: Starting a coroutine allocates a `Coroutine` object and boxes the `IEnumerator`. At high
kill rates (late waves, AoE pierce projectiles) this generates measurable GC.

**Fix**: Use a frame-deferred return via a static `WaitForEndOfFrame` cache, or have `GameBootstrap`
own a `DelayedReturnQueue` that processes returns at the start of each frame. Alternatively: if the
only reason for the one-frame delay is to avoid destroying during a physics step, consider calling
`pool.Return` directly in `HandleDeath` since `OnPoolReturn` only disables the object (it does not
destroy it), which is safe mid-physics.

**Estimated impact**: MEDIUM at high kill rates. Worth investigating once runtime profiling is enabled.

---

### M3 -- XPGem.Init: FindFirstObjectByType as runtime fallback

**File**: `SYNTHBORN/Assets/Scripts/Progression/XPGem.cs`
**Method**: `Init` (line 38)

```csharp
if (_xpManager == null)
    _xpManager = Object.FindFirstObjectByType<XPManager>();
```

**Problem**: Fallback scene search runs every time a gem is initialized without a pre-set reference.
`XPGemSpawner` already calls `SetXPManager` on each gem, so this fallback should never fire -- but it
will if initialization order is wrong, and the search is expensive if it does.

**Fix**: Remove the fallback. Make the `null` check an assertion-style `Debug.LogError` in
non-development builds so misconfiguration is caught at authoring time rather than silently patched at
runtime.

---

### M4 -- ShooterBrain / SummonerBrain / ExploderBrain: Vector2.Distance in hot loops

**Files**:
- `SYNTHBORN/Assets/Scripts/Enemies/Brains/ShooterBrain.cs` lines 89, 103
- `SYNTHBORN/Assets/Scripts/Enemies/Brains/SummonerBrain.cs` lines 69, 82, 129
- `SYNTHBORN/Assets/Scripts/Enemies/Brains/ExploderBrain.cs` lines 102, 141, 176

**Problem**: `Vector2.Distance` computes a square root. For range-check comparisons (is distance less
than threshold?), `sqrMagnitude` vs `threshold * threshold` is equivalent and avoids the sqrt. These
run every `FixedUpdate` frame per enemy.

**Fix**: Replace `Vector2.Distance(a, b) < range` with `((Vector2)a - b).sqrMagnitude < range * range`.
Cache `range * range` as a precomputed field in `Initialize`.

**Estimated impact**: LOW per instance, scales with enemy count.

---

### M5 -- HPOrb / XPGem: Vector2.Distance in Update loop

**Files**:
- `SYNTHBORN/Assets/Scripts/Progression/HPOrb.cs` lines 43, 53
- `SYNTHBORN/Assets/Scripts/Progression/XPGem.cs` lines 52, 65

Same pattern as M4 -- `Vector2.Distance` used for range comparisons that should use `sqrMagnitude`.

---

### M6 -- ExploderBrain.SpawnWarning: Instantiate without pooling

**File**: `SYNTHBORN/Assets/Scripts/Enemies/Brains/ExploderBrain.cs`
**Method**: `SpawnWarning` (line 159) / `DestroyWarning` (line 163)

**Problem**: The warning circle is instantiated on every prime and destroyed after detonation or death.
Lower frequency than trail segments but follows the same unpool pattern.

**Fix**: Single cached reference per Exploder instance -- instantiate once during Awake if the prefab
is assigned, deactivate/activate rather than destroy/instantiate.

---

## LOW Issues (Code Quality / Data-Driven Compliance)

### L1 -- EnemyBrain: Hardcoded ContactDamageInterval

**File**: `SYNTHBORN/Assets/Scripts/Enemies/Brains/EnemyBrain.cs` line 75

```csharp
private const float ContactDamageInterval = 0.5f;
```

This value is not exposed in `EnemyData` or `EnemyScalingConfig`. Per coding standards, tuning
values must be data-driven. Move to `EnemyScalingConfig` as a serialized field.

---

### L2 -- EnemyBrain.ComputeEffectiveSpeed: Hardcoded player base speed

**File**: `SYNTHBORN/Assets/Scripts/Enemies/Brains/EnemyBrain.cs` line 252

```csharp
float cap = scalingConfig.SpeedCapFraction * 5f; // Player base speed = 5.0 (PlayerConfig default)
```

Player base speed is read from `PlayerConfig` elsewhere. This comment acknowledges the coupling but
leaves a hardcoded `5f`. If `PlayerConfig.baseMoveSpeed` ever changes, enemy speed caps silently
diverge. Inject or reference `PlayerConfig` for the cap computation.

---

### L3 -- PoisonTrailSegment: Hardcoded damage tick interval

**File**: `SYNTHBORN/Assets/Scripts/Enemies/PoisonTrailSegment.cs` line 67

```csharp
if (_damageTimer >= 0.5f)
```

Tick rate is a tuning knob. Move to `PoisonerData` or a `PoisonTrailConfig` ScriptableObject so it can
be balanced without code changes.

---

### L4 -- DamageNumber: Hardcoded lifetime and physics values

**File**: `SYNTHBORN/Assets/Scripts/Core/VFX/DamageNumber.cs` lines 37-42

```
_lifetime = 0.8f;
_velocity = new Vector3(Random.Range(-0.5f, 0.5f), 1.5f, 0f);
```

Font sizes, drift speed, and lifetime should be configurable in a `DamageNumberConfig` ScriptableObject.
This is currently unreachable by designers without code changes.

---

### L5 -- HPOrb: Tuning values serialized on component, not ScriptableObject

**File**: `SYNTHBORN/Assets/Scripts/Progression/HPOrb.cs` lines 17-21

Fields `_pickupRadius`, `_magnetSpeed`, `_collectDistance`, `_lifetime`, `_healAmount` are per-prefab
`[SerializeField]` values with no ScriptableObject backing. If there are multiple HP orb variants (or
if balance changes are needed), each prefab must be edited individually. A shared `HPOrbConfig` SO
would centralize tuning.

---

### L6 -- PrototypeHUD: Uses legacy UnityEngine.UI.Text

**File**: `SYNTHBORN/Assets/Scripts/UI/PrototypeHUD.cs` (multiple Text references)
**Also**: `KillCounter.cs`, `LevelHUD.cs`, `GoldHUD.cs`

`UnityEngine.UI.Text` is the legacy UGUI text component. In Unity 6.x the recommended component is
`TextMeshProUGUI`. Legacy `Text` does not support font atlasing or signed distance field rendering,
producing blurry text at non-native resolution and generating separate draw calls per text element.
Replace with `TextMeshProUGUI` to improve visual quality and reduce draw calls.

---

## Top 5 Bottlenecks (Ranked by Expected Runtime Impact)

1. **DeathParticleSpawner unpool pattern** (C1) -- Up to 300 `new GameObject` allocations/sec in late
   waves. Guaranteed GC stutter at survivor-game scales.

2. **DamageNumberSpawner unpool + broken counter** (C2) -- Both a GC source and a mid-run gameplay
   defect. The counter never decrements, so damage numbers disappear after 20 total spawns.

3. **DeathBurstVFX shader/material leak** (C3) -- Shader.Find can return null in builds; material leak
   accumulates indefinitely if VFX fires frequently.

4. **LootDropper List allocation on every kill** (H3) -- Constant GC pressure; trivial one-line fix.

5. **EnemyBrain TickContactDamage GetComponent** (H1) -- 400 GetComponent calls/sec at max alive count;
   fixable by caching one reference in Initialize.

---

## Regressions Since Last Report

None detected -- this is the first performance analysis report for this project.

---

## Recommended Next Steps (for technical-director review)

1. **Assign engine-programmer** to fix C1, C2, C3 and H1 before next playtest build.
2. **Assign technical-artist** to wire `DeathBurstVFX` to a serialized material reference (C3).
3. **Profile with Unity Profiler** after fixes to establish runtime budget baselines for the frame-time
   table. Record against the 60fps / 16.6ms target.
4. **Configure memory ceiling** in `technical-preferences.md` after profiling.
5. **Track draw calls** using the Sprite Atlas Analyzer (Unity 6.3 built-in tool) once baseline
   profiling is complete.

---

*Report generated by performance-analyst agent. Escalate budget changes to technical-director.
Coordinate fixes with engine-programmer (H1, H2, H3, H7, M1-M6) and technical-artist (C3, L6).*
