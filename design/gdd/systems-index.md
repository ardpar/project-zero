# Systems Index: SYNTHBORN

> **Status**: Draft
> **Created**: 2026-03-27
> **Last Updated**: 2026-03-27
> **Source Concept**: design/gdd/game-concept.md

---

## Overview

SYNTHBORN is a roguelite survivor with a modular body-mutation system as its unique hook. The game requires systems spanning auto-combat, modular character building, visual sprite compositing, wave-based enemy spawning, synergy detection, and multi-layered meta-progression. The core loop is: **move → auto-attack → collect XP → level-up → choose mutation → discover synergies → survive waves → defeat boss**. All systems serve four pillars: Visual Evolution Satisfaction, Fast Chaotic Runs, Synergy Discovery, and Easy Entry/Deep Depth.

---

## Systems Enumeration

| # | System Name | Category | Priority | Status | Design Doc | Depends On |
|---|-------------|----------|----------|--------|------------|------------|
| 1 | Player Controller | Core | MVP | Designed | design/gdd/player-controller.md | — |
| 2 | Entity Health System | Core | MVP | Designed | design/gdd/entity-health-system.md | — |
| 3 | Camera System | Core | MVP | Designed | design/gdd/camera-system.md | — |
| 4 | Auto-Attack System | Gameplay | MVP | Designed | design/gdd/auto-attack-system.md | Player Controller, Entity Health |
| 5 | Projectile/Damage System | Gameplay | MVP | Designed | design/gdd/projectile-damage-system.md | Auto-Attack, Entity Health |
| 6 | XP & Level-Up System | Gameplay | MVP | Designed | design/gdd/xp-levelup-system.md | Entity Health |
| 7 | Düşman AI | Gameplay | MVP | Designed | design/gdd/enemy-ai-system.md | Player Controller, Entity Health |
| 8 | Modüler Mutasyon Sistemi | Gameplay | MVP | Designed | design/gdd/mutation-system.md | Auto-Attack, Projectile/Damage |
| 9 | Sprite Compositing (inferred) | Visual | MVP | Designed | design/gdd/sprite-compositing.md | Mutasyon Sistemi |
| 10 | Mutasyon Havuzu / Loot Table (inferred) | Economy | MVP | Designed | design/gdd/mutation-pool.md | Mutasyon Sistemi |
| 11 | Synergy Matrisi | Gameplay | MVP | Designed | design/gdd/synergy-matrix.md | Mutasyon Sistemi |
| 12 | Dalga/Spawning Sistemi | Gameplay | MVP | Designed | design/gdd/wave-spawning-system.md | Düşman AI, XP & Level-Up |
| 13 | Gameplay HUD (inferred) | UI | MVP | Designed | design/gdd/gameplay-hud.md | Entity Health, XP, Dalga Sistemi, Mutasyon Sistemi |
| 14 | Mutasyon Seçim UI | UI | MVP | Designed | design/gdd/mutation-selection-ui.md | Mutasyon Sistemi, Mutasyon Havuzu, Synergy Matrisi |
| 15 | VFX / Juice System (inferred) | Visual | MVP | Designed | design/gdd/vfx-juice-system.md | Projectile/Damage, XP, Entity Health |
| 16 | Run Manager (inferred) | Gameplay | Vertical Slice | Not Started | — | Dalga Sistemi, XP, Entity Health |
| 17 | Meta-İlerleme | Progression | Vertical Slice | Not Started | — | Run Manager, Unlock Tracker |
| 18 | Meta Currency (inferred) | Economy | Vertical Slice | Not Started | — | Run Manager, Meta-İlerleme |
| 19 | Unlock/Collection Tracker (inferred) | Progression | Vertical Slice | Not Started | — | Mutasyon Sistemi, Synergy Matrisi |
| 20 | Meta/Hub UI (inferred) | UI | Vertical Slice | Not Started | — | Meta-İlerleme, Meta Currency, Save/Load |
| 21 | Audio Manager (inferred) | Audio | Vertical Slice | Not Started | — | (event-driven, all gameplay systems) |
| 22 | Save/Load (inferred) | Persistence | Alpha | Not Started | — | Meta-İlerleme, Unlock Tracker |
| 23 | Leaderboard | Meta | Alpha | Not Started | — | Run Manager |
| 24 | Lore/Hikaye | Narrative | Alpha | Not Started | — | Run Manager, Dalga Sistemi |
| 25 | Onboarding/Tutorial (inferred) | Meta | Full Vision | Not Started | — | All MVP systems |

---

## Categories

| Category | Description |
|----------|-------------|
| **Core** | Foundation systems everything depends on — player movement, health, camera |
| **Gameplay** | Systems that make the game fun — combat, mutations, waves, synergies |
| **Visual** | Rendering and feedback — sprite compositing, VFX, juice |
| **Economy** | Resource creation and consumption — mutation pools, meta currency |
| **Progression** | How the player grows — unlocks, meta-stats, collection tracking |
| **UI** | Player-facing displays — HUD, mutation selection, meta menus |
| **Persistence** | Save state — meta-progression, settings |
| **Audio** | Sound and music — SFX, music, adaptive audio |
| **Narrative** | Story delivery — lore unlocks, boss stories |
| **Meta** | Outside core loop — tutorials, leaderboards, analytics |

---

## Priority Tiers

| Tier | Definition | Target Milestone | Systems Count |
|------|------------|------------------|---------------|
| **MVP** | Core loop functional: move → attack → XP → level-up → mutate → survive waves | First playable (4-6 weeks) | 15 |
| **Vertical Slice** | Complete run cycle: death → meta-progression → new run. One polished area. | Vertical slice (8-10 weeks) | 6 |
| **Alpha** | All features present in rough form. Save/load, leaderboard, story. | Alpha (14-18 weeks) | 3 |
| **Full Vision** | Polish, onboarding, full content. | Beta/Release (20-26 weeks) | 1 |

---

## Dependency Map

### Foundation Layer (no dependencies)

1. **Player Controller** — Top-down movement is the absolute base; everything targets or follows the player
2. **Entity Health System** — HP management for all entities; death triggers are the primary game event
3. **Camera System** — Viewport management; needed for spatial reasoning in all other systems

### Core Layer (depends on Foundation)

4. **Auto-Attack System** — depends on: Player Controller, Entity Health
5. **Projectile/Damage System** — depends on: Auto-Attack, Entity Health
6. **XP & Level-Up System** — depends on: Entity Health (enemy death → XP drop)
7. **Düşman AI** — depends on: Player Controller (target), Entity Health (damageable)

### Feature Layer (depends on Core)

8. **Modüler Mutasyon Sistemi** — depends on: Auto-Attack, Projectile/Damage (mutations modify attack behavior)
9. **Sprite Compositing** — depends on: Mutasyon Sistemi (visual representation of equipped mutations)
10. **Mutasyon Havuzu / Loot Table** — depends on: Mutasyon Sistemi (pool feeds the selection)
11. **Synergy Matrisi** — depends on: Mutasyon Sistemi (detects combinations on equipped mutations)
12. **Dalga/Spawning Sistemi** — depends on: Düşman AI, XP & Level-Up (wave composition + reward pacing)
13. **Run Manager** — depends on: Dalga Sistemi, XP, Entity Health (orchestrates run lifecycle)

### Presentation Layer (depends on Features)

14. **Gameplay HUD** — depends on: Entity Health, XP, Dalga Sistemi, Mutasyon Sistemi
15. **Mutasyon Seçim UI** — depends on: Mutasyon Sistemi, Mutasyon Havuzu, Synergy Matrisi
16. **VFX / Juice System** — depends on: Projectile/Damage, XP, Entity Health
17. **Audio Manager** — depends on: all gameplay systems (event-driven listeners)

### Polish Layer (depends on everything)

18. **Meta-İlerleme** — depends on: Run Manager, Unlock Tracker
19. **Meta Currency** — depends on: Run Manager, Meta-İlerleme
20. **Unlock/Collection Tracker** — depends on: Mutasyon Sistemi, Synergy Matrisi
21. **Save/Load** — depends on: Meta-İlerleme, Unlock Tracker
22. **Meta/Hub UI** — depends on: Meta-İlerleme, Meta Currency, Save/Load
23. **Leaderboard** — depends on: Run Manager
24. **Lore/Hikaye** — depends on: Run Manager, Dalga Sistemi
25. **Onboarding/Tutorial** — depends on: all MVP systems

---

## Recommended Design Order

| Order | System | Priority | Layer | Lead Agent | Est. Effort |
|-------|--------|----------|-------|------------|-------------|
| 1 | Player Controller | MVP | Foundation | gameplay-programmer | S |
| 2 | Entity Health System | MVP | Foundation | systems-designer | S |
| 3 | Camera System | MVP | Foundation | gameplay-programmer | S |
| 4 | Auto-Attack System | MVP | Core | systems-designer | M |
| 5 | Projectile/Damage System | MVP | Core | systems-designer | M |
| 6 | XP & Level-Up System | MVP | Core | economy-designer | S |
| 7 | Düşman AI | MVP | Core | ai-programmer | M |
| 8 | Modüler Mutasyon Sistemi | MVP | Feature | game-designer | L |
| 9 | Sprite Compositing | MVP | Feature | technical-artist | M |
| 10 | Mutasyon Havuzu / Loot Table | MVP | Feature | economy-designer | M |
| 11 | Synergy Matrisi | MVP | Feature | systems-designer | L |
| 12 | Dalga/Spawning Sistemi | MVP | Feature | systems-designer | M |
| 13 | Gameplay HUD | MVP | Presentation | ui-programmer | S |
| 14 | Mutasyon Seçim UI | MVP | Presentation | ui-programmer | M |
| 15 | VFX / Juice System | MVP | Presentation | technical-artist | M |
| 16 | Run Manager | V. Slice | Feature | gameplay-programmer | M |
| 17 | Unlock/Collection Tracker | V. Slice | Polish | systems-designer | S |
| 18 | Meta-İlerleme | V. Slice | Polish | economy-designer | M |
| 19 | Meta Currency | V. Slice | Polish | economy-designer | S |
| 20 | Meta/Hub UI | V. Slice | Polish | ui-programmer | M |
| 21 | Audio Manager | V. Slice | Presentation | sound-designer | M |
| 22 | Save/Load | Alpha | Polish | gameplay-programmer | M |
| 23 | Leaderboard | Alpha | Polish | gameplay-programmer | S |
| 24 | Lore/Hikaye | Alpha | Polish | narrative-director | M |
| 25 | Onboarding/Tutorial | Full Vision | Polish | ux-designer | M |

**Effort:** S = 1 session, M = 2-3 sessions, L = 4+ sessions

---

## Circular Dependencies

- **None found.** The dependency graph is a clean DAG. The Mutation System ↔ Auto-Attack relationship is one-directional: mutations modify attack parameters via a data interface, the attack system reads mutation stats but does not know about the mutation system directly.

---

## High-Risk Systems

| System | Risk Type | Risk Description | Mitigation |
|--------|-----------|-----------------|------------|
| Sprite Compositing | Technical | Modular sprite layering with animation sync across 4+ body slots is unproven in Unity 2D. Performance with many enemies on screen unknown. | Prototype early with 3-4 mutations. Test with 200+ enemies. Evaluate SpriteRenderer layering vs custom shader approach. |
| Synergy Matrisi | Design | Combinatorial explosion: 40 mutations = 780 possible pairs. Defining, balancing, and testing all synergies is massive scope. | Start with 5-10 synergies for MVP. Use data-driven ScriptableObject approach. Design a "synergy template" system that generates variations. |
| Mutasyon Havuzu / Loot Table | Design / Balance | Bad pool weighting = "dead runs" where no useful mutations are offered. Too generous = every run feels the same. | Define mathematical model for pool weights. Implement pity timer. Playtest with 50+ runs during prototype phase. |
| Dalga/Spawning Sistemi | Design | Difficulty curve for 10-15 minute runs with auto-scaling is hard to tune. Too easy = boring. Too hard = frustrating (anti-pillar). | Prototype with 3 difficulty presets. Use data-driven wave tables. Playtest extensively. |
| Düşman AI | Technical | 200+ enemies with individual AI = performance risk. Sürü davranışı (flocking) is CPU-intensive. | Use simple state machines, not complex BTs. Burst Jobs for movement calculations. Object pooling mandatory. |

---

## Progress Tracker

| Metric | Count |
|--------|-------|
| Total systems identified | 25 |
| Design docs started | 15 |
| Design docs reviewed | 0 |
| Design docs approved | 0 |
| MVP systems designed | 15/15 |
| Vertical Slice systems designed | 0/6 |

---

## Next Steps

- [ ] Review and approve this systems index
- [ ] Design MVP-tier systems first (use `/design-system [system-name]`)
- [ ] Start with: Player Controller → Entity Health → Auto-Attack
- [ ] Prototype highest-risk system early: Sprite Compositing (`/prototype sprite-compositing`)
- [ ] Run `/design-review` on each completed GDD
- [ ] Run `/gate-check pre-production` when MVP systems are designed
- [ ] Plan first implementation sprint with `/sprint-plan new`
