# ADR-002: MVP Combat Systems Architecture

> **Status**: Accepted
> **Date**: 2026-03-28
> **Decision Makers**: User + Gameplay Programmer + AI Programmer
> **Implements**: player-controller, entity-health, auto-attack, projectile-damage, xp-levelup, enemy-ai, wave-spawning GDDs
> **Conforms to**: ADR-001 (ScriptableObject, EventBus, ObjectPool)

---

## Architecture Decisions

| Decision | Choice | Alternatives Rejected |
|----------|--------|----------------------|
| EntityHealth | Single component + EntityHealthConfig SO | Player/Enemy subclasses (duplicated logic) |
| Attack slots | `List<AttackSlotState>` structs in single controller | Component-per-slot (complex hierarchy) |
| Stat modification | Hybrid: CombatStatBlock (pull) + IAttackModifier (push) | Pure push (fragile), pure pull (can't do structural mods) |
| Projectile collision | Physics2D triggers for MVP | Raycasts (swap if >2ms budget) |
| XP gem spawning | XPGemSpawner separate from XPManager | Combined (mixed concerns) |
| Enemy AI | Plain C# loop, profile before Burst Jobs | Burst from day one (premature optimization) |
| Enemy data | Derived ScriptableObjects (ShooterData, ExploderData) | Single flat SO (confusing unused fields) |
| Boss class | Separate BossBrain stub from day one | ChaserBrain+flag (refactor debt) |
| Shooter projectile | GameEvents.OnProjectileRequested (loose coupling) | Direct pool reference injection |
| Assembly Definitions | 5 asmdef: Core, Player, Combat, Enemies, Progression | Monolithic (slow compile) |

---

## Folder Structure

```
src/
├── Core/                          [Core.asmdef]
│   ├── Pool/
│   │   ├── ObjectPool.cs          Generic ObjectPool<T>
│   │   └── IPoolable.cs           Interface: OnPoolGet(), OnPoolReturn()
│   ├── Events/
│   │   └── GameEvents.cs          Static EventBus
│   ├── Stats/
│   │   └── CombatStatBlock.cs     Runtime mutation stat aggregate
│   └── Data/
│       └── DamageInfo.cs          Struct + DamageSource enum
│
├── Player/                        [Player.asmdef → Core]
│   ├── PlayerController.cs        Movement FSM + Input System
│   ├── PlayerInputHandler.cs      InputAction wrapper
│   └── Data/
│       └── PlayerConfig.cs        SO: movement + dash params
│
├── Combat/                        [Combat.asmdef → Core]
│   ├── Health/
│   │   ├── EntityHealth.cs        Shared HP/armor, IDamageable
│   │   ├── EntityHealthConfig.cs  SO: hp, armor, invuln config
│   │   └── IDamageable.cs         Interface
│   ├── AutoAttack/
│   │   ├── AutoAttackController.cs  Slot management + targeting
│   │   ├── AttackSlotData.cs      SO: interval, projectile ref
│   │   ├── AttackSlotState.cs     Struct: runtime cooldown
│   │   ├── AttackConfig.cs        SO: range, cone, max slots
│   │   ├── IAttackModifier.cs     Interface: structural mods
│   │   └── TargetingSystem.cs     Cone query + 360° fallback
│   └── Projectile/
│       ├── ProjectileController.cs  Movement, collision, pool
│       ├── ProjectileData.cs      SO: speed, damage, on-hit type
│       └── HitBehavior/
│           ├── IHitBehavior.cs    Strategy interface
│           ├── DestroyOnHit.cs
│           ├── PierceOnHit.cs
│           ├── AoEOnHit.cs
│           └── ChainOnHit.cs
│
├── Enemies/                       [Enemies.asmdef → Core, Combat]
│   ├── Brains/
│   │   ├── EnemyBrain.cs          Abstract base (chase, contact dmg, states)
│   │   ├── ChaserBrain.cs         Simple chase (also Runner/Tank via data)
│   │   ├── ShooterBrain.cs        Chase → stop at range → fire
│   │   ├── ExploderBrain.cs       Chase → prime → explode
│   │   └── BossBrain.cs           MVP stub, VS phase system
│   ├── EnemyInitializer.cs        Sets health from EnemyData + wave
│   ├── EnemyState.cs              Enum
│   └── Data/
│       ├── EnemyData.cs           SO base: hp, speed, xp, tier
│       ├── ShooterData.cs         SO: shoot_range, interval, projectile
│       ├── ExploderData.cs        SO: explode_range, prime, radius
│       ├── BossData.cs            SO: phases (VS), intro VFX
│       └── EnemyScalingConfig.cs  SO: wave scaling params
│
├── Waves/                         [part of Enemies.asmdef]
│   ├── WaveSpawner.cs             Wave state machine, spawn logic
│   └── Data/
│       ├── WaveTableData.cs       SO: wave definitions array
│       ├── WaveDefinition.cs      Struct: duration, interval, pool
│       └── SpawnEntry.cs          Struct: enemyData + weight
│
├── Progression/                   [Progression.asmdef → Core]
│   ├── XPManager.cs               XP tracking, level-up chain
│   ├── XPGemSpawner.cs            Subscribes OnEnemyDied, spawns gems
│   ├── XPGem.cs                   Pooled: magnet + pickup
│   └── Data/
│       ├── XPConfig.cs            SO: pickup radius, magnet, lifetime
│       └── XPLevelTable.cs        SO: stepped XP curve
│
└── Tests/
    ├── EditMode/
    │   ├── DamageCalculationTests.cs
    │   ├── XPLevelCurveTests.cs
    │   ├── EffectiveSpeedTests.cs
    │   └── EnemyHPScalingTests.cs
    └── PlayMode/
        ├── ProjectileLifecycleTests.cs
        ├── TargetingConeTests.cs
        └── XPGemPickupTests.cs
```

---

## Key Interfaces

```csharp
// Synthborn.Core
public interface IPoolable {
    void OnPoolGet();
    void OnPoolReturn();
}

public interface IDamageable {
    void TakeDamage(DamageInfo info);
    bool IsDead { get; }
}

// Synthborn.Combat
public interface IAttackModifier {
    string ModifierId { get; }
    void Apply(AutoAttackController controller);
    void Remove(AutoAttackController controller);
}

public interface IHitBehavior {
    bool OnHit(ProjectileController projectile, IDamageable target, DamageInfo baseDamage);
    // Returns true → return projectile to pool
}
```

---

## Damage Flow Pipeline

```
AutoAttack cooldown expires + target found
  → ProjectilePool.Get(origin, direction, projectileData)
    → Projectile travels (FixedUpdate)
      → OnTriggerEnter2D(enemy collider)
        → Resolve crit: Random.value < (baseCrit + StatBlock.CritChance)
        → Calculate: max(round(baseDmg * (1+dmgMod) * critMult), 1)
        → Fire GameEvents.OnDamageDealt(pos, finalDmg, isCrit)
        → target.TakeDamage(damageInfo)
          → EntityHealth: actualDmg = max(raw - armor, 1)
          → HP -= actualDmg
          → if HP ≤ 0: Die()
            → Fire GameEvents.OnEnemyDied(pos, data, xp)
              → XPGemSpawner spawns gem
              → WaveSpawner decrements alive count
              → VFX plays death effect
        → IHitBehavior.OnHit() → Destroy/Pierce/AoE/Chain
```

---

## Enemy AI State Machines

**Chaser/Runner/Tank:** Chase → Dead (2 states, data-driven speed/HP difference)

**Shooter:** Chase → Shooting → Dead (stop at range, fire on interval)

**Exploder:** Chase → Priming → Exploding → Dead (1s warning, AoE on explode, killed-before-prime = no explosion)

**Boss MVP:** ChaserBrain + BossData (big HP, slow, high damage). BossBrain stub.

**Performance:** Plain C# Update loop. 200 enemies × ~10 ops = negligible. Profile → Burst Jobs only if >1ms.

---

## Implementation Order (8 Tiers)

| Tier | Systems | Dependencies |
|------|---------|-------------|
| 0 | ObjectPool, GameEvents, CombatStatBlock, DamageInfo | None |
| 1 | All ScriptableObject data types | None |
| 2 | EntityHealth + IDamageable | Tier 0-1 |
| 3 | PlayerController + PlayerInputHandler | Tier 0-1 |
| 4 | ProjectileController + DestroyOnHit | Tier 0-2 |
| 5 | AutoAttackController + TargetingSystem | Tier 3-4 |
| 6 | EnemyBrain variants + EnemyInitializer + WaveSpawner | Tier 0-2 |
| 7 | XPManager + XPGem + XPGemSpawner + PauseManager | Tier 0-2-6 |

Each tier = one PR, testable independently.

---

## Prototype → Production Changes

| Prototype | Production | Reason |
|-----------|-----------|--------|
| Singletons (Instance) | [SerializeField] injection | ADR-001 |
| Hardcoded values | ScriptableObject | ADR-001 |
| PlayerHealth + EnemyHealth | Single EntityHealth + config SO | DRY |
| Time.timeScale in GameEvents | PauseManager subscriber | Events = pure notification |
| Legacy Input | Input System package | Unity 6.3 default |
| Plain MutationData class | MutationData : ScriptableObject | Asset refs, inspector |
