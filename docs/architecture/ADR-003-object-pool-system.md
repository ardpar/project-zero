# ADR-003: Generic ObjectPool&lt;T&gt; System

## Status
Accepted

## Date
2026-04-02

## Context

### Problem Statement
SYNTHBORN targets 60 fps with 200+ simultaneous enemies and 300+ projectiles on screen. Without object reuse, each projectile fired, XP gem dropped, damage number displayed, or enemy spawned triggers a `GameObject` instantiation and eventual `Destroy()` call. In Unity, these operations force GC allocations that cause frame-time spikes — particularly damaging in a survivor game where the spawn rate is continuous and high. A pooling solution was required from the first combat sprint and is referenced as a dependency in ADR-001 and ADR-002.

### Constraints
- Must work with any `MonoBehaviour` subclass without requiring type-specific pool classes.
- Must integrate cleanly with Unity's GameObject activation model (`SetActive`).
- Must not introduce a global singleton — pools must be owned by the system that uses them (injected at runtime).
- Pool instances must be pre-warmable to avoid spikes on first spawn.

### Requirements
- Must support: `ProjectileController`, `EnemyBrain`, `XPGem`, `HPOrb`, `DamageNumber`, `DashTrail`, `DeathParticle`, `PoisonTrailSegment`, `MaterialPickupDisplay`.
- Must allow each consumer to specify its own factory delegate (prefab may differ per spawner).
- Must provide diagnostic counters (`AvailableCount`, `TotalCreated`, `ActiveCount`) for profiling.
- Must be safe against double-return (return called on an already-inactive object).

## Decision

A single generic class `ObjectPool<T> where T : MonoBehaviour, IPoolable` is implemented in `Synthborn.Core.Pool`. It stores inactive instances in a `Stack<T>` and is paired with the `IPoolable` interface that every pooled type must implement.

**Pool ownership follows constructor injection.** Each spawner system receives its pool from `GameBootstrap` during `Awake()`. No pool is held in a static field or a global manager.

**Factory delegate pattern.** The pool accepts a `Func<T>` at construction so the caller controls prefab instantiation. This means the pool itself has zero knowledge of prefabs or asset addresses.

**IPoolable lifecycle contract:**
- `OnPoolGet()` — called after the object is activated; implementations reset all runtime state here.
- `OnPoolReturn()` — called before the object is deactivated; implementations stop coroutines and cancel effects here.

**Double-return guard.** `Return()` checks `instance.gameObject.activeSelf` before pushing; already-inactive objects are silently skipped. This prevents stack corruption from redundant returns (e.g., a projectile returned by both collision and lifetime expiry paths).

### Architecture Diagram

```
GameBootstrap (Awake, Order -100)
    │
    ├─ new ObjectPool<ProjectileController>(factory, preWarm)  ──► AutoAttackController.SetPool()
    ├─ new ObjectPool<EnemyBrain>(factory, preWarm)            ──► WaveSpawner.SetPool()
    ├─ new ObjectPool<XPGem>(factory, 0)                       ──► XPGemSpawner.SetPool()
    ├─ new ObjectPool<HPOrb>(factory, hpOrbPoolSize)           ──► (HPOrb spawning system)
    ├─ new ObjectPool<DamageNumber>(factory, 0)                ──► DamageNumberSpawner.SetPool()
    └─ new ObjectPool<DeathParticle>(factory, 0)               ──► DeathParticleSpawner.SetPool()

ObjectPool<T>                          IPoolable
  _pool : Stack<T>           ◄──────   OnPoolGet()
  _factory : Func<T>                   OnPoolReturn()
  Get() : T                    │
  Return(T) : void             │
  AvailableCount : int    Implemented by:
  TotalCreated : int        ProjectileController, EnemyBrain, XPGem, HPOrb,
  ActiveCount : int         DamageNumber, DashTrail, DeathParticle,
                            PoisonTrailSegment, MaterialPickupDisplay
```

### Key Interfaces

```csharp
// Synthborn.Core.Pool
public interface IPoolable
{
    void OnPoolGet();
    void OnPoolReturn();
}

public class ObjectPool<T> where T : MonoBehaviour, IPoolable
{
    public ObjectPool(Func<T> factory, int initialCapacity = 0);
    public T Get();
    public void Return(T instance);
    public int AvailableCount { get; }
    public int TotalCreated { get; }
    public int ActiveCount { get; }
}
```

## Alternatives Considered

### Alternative 1: Unity's Built-in ObjectPool&lt;T&gt; (UnityEngine.Pool)
- **Description**: Use `UnityEngine.Pool.ObjectPool<T>` introduced in Unity 2021.
- **Pros**: No custom code to maintain; supports `maxSize` cap and `collectionCheck` for debug double-return detection.
- **Cons**: Not constrained to `MonoBehaviour + IPoolable`; would require wrapping or adapting for the `SetActive` pattern; adds a Unity-version dependency on a namespace that may shift across LTS versions.
- **Rejection Reason**: The project's Tier 0 core must have zero external dependencies. The custom implementation is 82 lines and covers all required behavior; the overhead of learning a third-party API surface is not justified.

### Alternative 2: Type-Specific Pool Classes (ProjectilePool, EnemyPool, etc.)
- **Description**: One pool class per pooled type.
- **Pros**: Simple, no generics complexity.
- **Cons**: Large code duplication; adding a new poolable type requires a new pool class; no shared diagnostic interface.
- **Rejection Reason**: The `IPoolable` interface provides the common contract that makes the generic approach both type-safe and zero-duplication.

### Alternative 3: Global Pool Registry / PoolManager Singleton
- **Description**: A static `PoolManager` that owns all pools and provides `Get<T>()` globally.
- **Pros**: Convenient — any system can retrieve a pool without injection.
- **Cons**: Static singletons for game state are explicitly forbidden by project coding standards. Creates hidden coupling and makes systems harder to test.
- **Rejection Reason**: Violates the dependency injection rule established in ADR-001. GameBootstrap's constructor-injection model is the correct coordination point.

## Consequences

### Positive
- Eliminates per-frame GC allocations from the highest-frequency game objects (projectiles, enemies, gems).
- Adding a new poolable type requires only: implement `IPoolable`, create the pool in `GameBootstrap`, inject via a `SetPool()` method.
- `TotalCreated` and `ActiveCount` counters allow the Unity Profiler to verify pool sizing without custom tooling.
- Double-return guard prevents a class of subtle runtime bugs with no performance cost.

### Negative
- All pooled `MonoBehaviour` types carry an `IPoolable` implementation requirement. Implementors must be disciplined about resetting state in `OnPoolGet()` — a missed field reset causes ghost-state bugs that are hard to reproduce.
- The pool grows without bound. There is no maximum size, so an atypical spike (e.g., 400 simultaneous projectiles) permanently allocates those instances even if the normal steady state is 60. Memory footprint is bounded only by the worst-case peak, not the average load.
- Pre-warming runs `_factory()` synchronously in `Awake()`. For prefabs with complex `Awake()` chains, this can extend load time.

### Risks
- **Ghost-state bugs from incomplete `OnPoolGet()` resets.** Mitigation: Code review checklist item for every new `IPoolable` implementation; integration tests that borrow, return, and re-borrow instances to verify field resets.
- **Unbounded pool growth in edge cases.** Mitigation: Monitor `TotalCreated` in the profiler during QA playtests; add a configurable `maxSize` cap in a future revision if memory pressure is observed.
- **Pool lifetime tied to `GameBootstrap`.** If the scene is destroyed and rebuilt mid-session, pools must be re-created. Mitigation: `GameBootstrap` is scene-level; pool re-creation is automatic on scene reload.

## Performance Implications
- **CPU**: `Get()` and `Return()` are O(1) stack operations with no allocation. `Stack<T>` does not resize on pop/push within capacity. Pre-warmed pools have zero allocation cost during gameplay.
- **Memory**: Each pool instance holds `initialCapacity` inactive GameObjects in memory. For 100-enemy pools with typical enemy prefab overhead (~2 KB each), steady-state cost is approximately 200 KB — acceptable within project memory targets.
- **Load Time**: Pre-warming 100 enemies + 60 projectiles adds ~50 ms to `Awake()`. This occurs before the first frame and is not player-visible during scene load.
- **Network**: Not applicable.

## Migration Plan
The pattern was established at Sprint 2 and is fully implemented. No migration is required. Future poolable types must follow the pattern: implement `IPoolable`, register in `GameBootstrap`, receive the pool via injection.

## Validation Criteria
- Unity Profiler GC Alloc column shows zero allocations in the projectile and enemy spawn hot paths during a 5-minute run.
- `ObjectPool.TotalCreated` for the enemy pool does not grow beyond `initialCapacity + 20` during a standard wave progression run (indicating the pool is sized correctly).
- All `IPoolable` implementations pass a re-borrow test: get instance, configure state, return, get again — verify all fields are in initial state.

## Related Decisions
- ADR-001: Core Architecture Patterns — establishes no-singleton rule and dependency injection as project-wide constraints.
- ADR-002: MVP Combat Systems Architecture — lists ObjectPool as a dependency for ProjectileController and EnemyBrain.
