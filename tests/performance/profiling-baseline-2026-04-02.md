# Profiling Baseline — 2026-04-02

**Build**: main @ 87d61c1b (Sprint 17 commit)
**Engine**: Unity 6.3 LTS (6000.3.11f1)
**Scope**: Editor memory baseline (SampleScene loaded, no Play Mode)
**Method**: Unity Profiler API via script-execute

---

## Editor Memory Baseline (Non-Gameplay)

| Metric | Value | Notes |
|--------|-------|-------|
| Total Allocated | 1401 MB | Editor inflated — includes IDE, Profiler, MCP plugin |
| Total Reserved | 1890 MB | Unity editor memory pool |
| Mono Heap | 1294 MB | Managed heap (editor + scripts) |
| Mono Used | 1072 MB | Active managed allocations |
| GFX Driver | 205 MB | GPU-side allocations (textures, render targets) |

> **Important**: Editor memory is NOT representative of build memory. A standalone
> build will use ~200-400 MB total for a 2D pixel art game of this scope.
> Build profiling required for accurate memory ceiling.

## Asset Counts

| Asset Type | Count | Notes |
|------------|-------|-------|
| Textures | 956 | Includes editor UI textures |
| Sprites | 33 | Game sprites only |
| Meshes | 11 | Minimal for 2D game |
| Materials | 142 | Includes editor materials |
| AnimClips | 2 | Boss_Idle, Chaser_Idle/Walk |

## Performance Fixes Applied (Sprint 17)

All 13 issues from perf-report-2026-03-29.md addressed:

| Fix | Category | Status |
|-----|----------|--------|
| C1 DeathParticleSpawner pooling | Critical | Fixed (pre-Sprint 17) |
| C2 DamageNumberSpawner pooling + counter | Critical | Fixed (pre-Sprint 17) |
| C3 DeathBurstVFX material serialized | Critical | Fixed (pre-Sprint 17) |
| H1 EnemyBrain GetComponent cache | High | Fixed (pre-Sprint 17) |
| H2 TargetingSystem TryGetComponent | High | Fixed (Sprint 17) |
| H3 LootDropper list reuse | High | Fixed (pre-Sprint 17) |
| H4 DashTrail pooling | High | Fixed (Sprint 17) |
| H5 PrototypeHUD cached TrialManager | High | Fixed (Sprint 17) |
| H6 WaveSpawner serialized LevelManager | High | Fixed (pre-Sprint 17) |
| H7 SummonerBrain reverse loop | High | Fixed (pre-Sprint 17) |
| H8 PoisonerBrain trail pooling | High | Fixed (Sprint 17) |
| M1-M6 Medium fixes | Medium | Fixed (Sprint 17) |
| L1-L5 Data-driven compliance | Low | Fixed (Sprint 17) |

## Frame Budget Target

| Category | Budget | Measured | Status |
|----------|--------|----------|--------|
| Total Frame | 16.6 ms | — | REQUIRES BUILD PROFILING |
| Gameplay Logic | 4 ms | — | REQUIRES BUILD PROFILING |
| Rendering | 5 ms | — | REQUIRES BUILD PROFILING |
| Physics | 3 ms | — | REQUIRES BUILD PROFILING |
| AI | 3 ms | — | REQUIRES BUILD PROFILING |
| Audio | 1 ms | — | REQUIRES BUILD PROFILING |

## Recommended Next Steps

1. **Build profiling**: Create a Development Build with Profiler connection
2. **Test scenarios**: Wave 1 (few enemies), Wave 5 (many enemies), Boss fight, 200-enemy stress test
3. **Measure draw calls**: Use Unity 6.3 Sprite Atlas Analyzer + Frame Debugger
4. **Set memory ceiling**: After build profiling, update technical-preferences.md

---

*Baseline captured by script-execute via Unity MCP. Build profiling deferred to runtime session.*
