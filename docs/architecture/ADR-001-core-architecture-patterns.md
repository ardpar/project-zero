# ADR-001: Core Architecture Patterns

> **Status**: Accepted
> **Date**: 2026-03-27
> **Decision Makers**: User + Claude Code Game Studios
> **Relates to**: All 15 MVP systems

## Context

SYNTHBORN is a roguelite survivor with 15 MVP systems that need to communicate, share data, and perform well with 200+ enemies and 300+ projectiles on screen. We need to decide on three foundational patterns before any implementation begins:

1. **Data Architecture** — How game data (mutations, enemies, waves, tuning knobs) is stored and accessed
2. **System Communication** — How systems talk to each other (tight coupling vs loose coupling)
3. **Object Lifecycle** — How frequently created/destroyed objects (enemies, projectiles, XP gems) are managed

These decisions affect every system and are expensive to change later.

## Decision 1: ScriptableObject-Driven Data

### Choice: ScriptableObjects for all game data

All gameplay values, configurations, and definitions are stored in ScriptableObject assets, not hardcoded in MonoBehaviours.

**What this means:**
- `MutationData` (SO) — mutation stats, tags, visual references
- `EnemyData` (SO) — enemy type stats, behaviors, sprites
- `WaveTableData` (SO) — wave composition, timing, difficulty
- `ProjectileData` (SO) — projectile speed, damage, on-hit behavior
- `PlayerConfig` (SO) — base stats, dash parameters, collider size
- `SynergyDefinition` (SO) — required tags, bonus effects

**Alternatives Considered:**
- **JSON/XML files**: More portable but lose Unity inspector editing, no asset references, requires parsing layer
- **Hardcoded values**: Fastest to implement but violates coding standards ("gameplay values must be data-driven")
- **Database (SQLite)**: Overkill for single-player, adds dependency

**Why ScriptableObject:**
- Native Unity asset — inspector editing, drag-drop references, no serialization code
- Hot-reloadable in editor (change values during play mode)
- Every GDD specifies tuning knobs — ScriptableObjects make these directly editable by designers
- Zero runtime parsing cost (loaded as Unity assets)
- Supports asset references (sprite, prefab, audio clip) natively

**Consequences:**
- (+) All tuning knobs from GDDs become editable fields
- (+) No hardcoded values in MonoBehaviour scripts
- (-) SO assets must be organized (folder structure, naming convention)
- (-) Runtime SO values are shared — need to copy at runtime if mutated per-instance

## Decision 2: Event-Driven System Communication

### Choice: C# Events + lightweight Event Bus

Systems communicate through C# events and a central EventBus for cross-cutting concerns. No direct method calls between unrelated systems.

**Pattern:**
```
// Direct C# events for 1-to-1 relationships
public class EntityHealth : MonoBehaviour {
    public event Action<int, DamageSource> OnDamageTaken;
    public event Action OnDeath;
}

// EventBus for cross-cutting / 1-to-many
public static class GameEvents {
    public static event Action<int> OnWaveStarted;
    public static event Action<MutationData> OnMutationEquipped;
    public static event Action<SynergyDefinition> OnSynergyActivated;
    public static event Action OnBossDefeated;
}
```

**Alternatives Considered:**
- **Direct method calls**: Simplest but creates tight coupling (Player Controller calling Camera directly)
- **Unity Events (Inspector)**: Visual but fragile, hard to debug, serialization issues
- **Scriptable Object Events (Ryan Hipple pattern)**: Good but adds SO asset overhead for every event
- **Message Bus (Pub/Sub with string keys)**: Flexible but loses type safety

**Why C# Events + EventBus:**
- Type-safe, zero allocation, compile-time checking
- Every GDD specifies interactions as "X publishes event, Y listens" — maps directly
- Direct events for tight relationships (Health → PlayerController.OnDeath)
- Static EventBus for broadcast events (WaveStarted, MutationEquipped, BossDefeated)
- Easy to debug (breakpoints on event invocation)

**Consequences:**
- (+) Loose coupling — systems don't reference each other directly
- (+) VFX/Juice/Audio can listen to any event without being a dependency
- (+) New systems can subscribe without modifying existing code
- (-) Must unsubscribe on destroy (memory leak risk if not)
- (-) Static EventBus requires careful cleanup between runs

## Decision 3: Object Pool for High-Frequency Entities

### Choice: Generic ObjectPool<T> for enemies, projectiles, XP gems, VFX

All frequently instantiated/destroyed objects use pre-allocated object pools instead of Instantiate/Destroy.

**Pooled entities (from GDDs):**
- Enemies: max 200 alive (Düşman AI GDD)
- Projectiles: max 300 active (Projectile/Damage GDD)
- XP Gems: max 500 active (XP GDD)
- Damage Numbers: max 30 active (VFX GDD)
- Particle Effects: max 50 active (VFX GDD)

**Pattern:**
```
public class ObjectPool<T> where T : MonoBehaviour {
    private Queue<T> _pool;
    public T Get();      // Activate from pool
    public void Return(T obj);  // Deactivate, return to pool
}
```

**Alternatives Considered:**
- **Instantiate/Destroy**: Simplest but causes GC spikes with 200+ enemies dying per minute
- **Unity's built-in ObjectPool (2021+)**: Available but collection-based, less control
- **DOTS Entity pooling**: Best performance but requires ECS rewrite — overkill for 2D pixel art game

**Why Generic ObjectPool:**
- GDDs specify max counts — pool sizes are known at design time
- ~750 enemies killed per run (Wave GDD) = ~750 Instantiate/Destroy avoided
- XP gems, projectiles, damage numbers are even more frequent
- Simple to implement, well-understood pattern
- Pre-warming eliminates first-encounter stutter

**Consequences:**
- (+) Zero GC allocation during gameplay
- (+) Consistent frame times (no GC spikes)
- (-) Higher initial memory (pre-allocated pools)
- (-) Objects must properly reset state on Get/Return
- (-) Pool sizes need tuning (too small = fallback to Instantiate, too large = wasted memory)

## Implementation Notes

### Folder Structure
```
src/
├── Core/
│   ├── EventBus/GameEvents.cs
│   ├── Pool/ObjectPool.cs
│   └── Data/                    # Base SO types
├── Gameplay/
│   ├── Player/
│   ├── Enemies/
│   ├── Mutations/
│   ├── Combat/
│   └── Waves/
└── UI/
```

### Conventions
- Every MonoBehaviour that holds tuning data takes a ScriptableObject reference via `[SerializeField]`
- Every system that publishes events documents them in a `// Events` region
- Every pooled object implements `IPoolable { void OnPoolGet(); void OnPoolReturn(); }`
- EventBus subscriptions happen in `OnEnable`, unsubscriptions in `OnDisable`

## Review

This ADR covers the three patterns referenced across all 15 MVP GDDs:
- "Tüm değerler ScriptableObject'ten okunur" (every GDD acceptance criteria)
- "Event yayınlar" / "event dinler" (every interaction table)
- "Object pool kullanılır" (Projectile, Düşman AI, XP, VFX GDDs)
