# SYNTHBORN — Visual & Technical Art Assessment

**Date:** 2026-03-29
**Reviewer:** Technical Artist
**Engine:** Unity 6.3 LTS (6000.3.11f1) · URP 2D Renderer
**Scope:** Shaders, sprites/textures, VFX/particles, camera effects, UI visuals, 2D lighting, art pipeline

---

## Summary

The project has a working game-feel foundation: screen shake is well-implemented, the dash trail produces readable motion feedback, wave banners animate correctly, and the static event bus provides clean hooks for all future VFX work. However, three categories of issues need resolution before the game reaches a shippable visual bar:

1. **One blocking runtime bug** — damage numbers permanently stop spawning after 20 hits
2. **One rendering correctness bug** — death burst particles render pink/magenta on URP
3. **Multiple performance and pipeline gaps** — no sprite atlases, GC-heavy VFX allocation, missing 2D lighting, legacy text systems

---

## Part 1: What Works Well

### Screen Shake (CameraController + ScreenShakeListener)
- `CameraController.TriggerShake()` uses `Time.unscaledDeltaTime` — shake correctly fires during pause (level-up screen, mutation selection)
- `ScreenShakeListener` is event-driven, subscribes to `OnDamageDealt`, `OnEnemyDied`, `OnBossSpawned`
- Crit hits apply a 1.5x intensity multiplier — good tactile distinction
- Boss entry shake (0.4s / 0.3 intensity) is meaningfully heavier than standard enemy death
- Arena boundary clamping and SmoothDamp follow are solid

### Dash Trail (DashTrail)
- Ghost SpriteRenderers spawned at 0.03s intervals during dash
- Light blue tint `(0.5, 0.8, 1.0, 0.5)` — visually distinct, communicates player direction
- Reasonable 0.2s fade lifetime — doesn't linger too long

### Wave Banner Animation (WaveBanner)
- Scale-in (2x → 1x) + fade-in, hold, fade-out sequence
- Uses `unscaledDeltaTime` — correct since banner plays at timeScale = 0 context
- Stagger pattern is present and works

### Mutation Card Slide-In (MutationSelectionUI)
- 0.1s per-card stagger on appearance
- `WaitForSecondsRealtime` — correctly unscaled during pause
- Communicates visual priority through sequencing

### Poison Trail Segment (PoisonTrailSegment)
- Last-30% fade-out is smooth — `alpha * 0.6f` caps initial alpha at 60%, which avoids jarring fully-opaque disappearance
- Self-destructs cleanly on duration expiry

### Event Bus VFX Hooks (GameEvents)
- `OnVfxRequested`, `OnSfxRequested`, `OnDamageDealt`, `OnEnemyDied`, `OnBossSpawned`, `OnPlayerDashStarted/Ended`, `OnWaveStarted`, `OnSynergyActivated` all exist
- `Cleanup()` nulls all events on run reset — prevents stale subscriber leaks
- Good foundation for all future VFX wiring

### SRP Batcher
- `m_UseSRPBatcher: 1` in UniversalRP.asset — enabled and correct for URP
- Dynamic batching correctly disabled (`m_SupportsDynamicBatching: 0`)

---

## Part 2: What's Missing for Game Feel

### P0 — Blocking
- **Hit pause absent.** A 1–2 frame `timeScale` micro-pause (0.016–0.033s) on strong impacts (boss hits, player death, elite kills) is the single highest-impact missing feedback. The event bus already has `OnDamageDealt` and `OnEnemyDied` — this is a ~50-line addition.

### P1 — High Impact
- **True white flash missing.** `HitFlash.cs` sets `_renderer.color = Color.white`, which tints the sprite toward white rather than fully replacing pixel color. A proper flash requires a sprite shader with a `_FlashAmount` float property (0–1 lerp between sprite color and white in fragment), read via `MaterialPropertyBlock` to preserve SRP Batcher compatibility.
- **No post-processing volume.** `m_VolumeProfile: {fileID: 0}` in UniversalRP.asset — no bloom, no color grading, no vignette. A single global volume with subtle bloom (threshold 0.8, intensity 0.3) and a slight color grade would transform the 2D pixel art read significantly.
- **No 2D lighting.** The Renderer2D feature set supports URP `Light2D` natively. No `Light2D` components exist anywhere in the project. Dynamic point lights on player attacks, poison pools, and the boss entrance would add substantial atmosphere at low cost (renderer is already configured with `m_LightRenderTextureScale: 0.5` — shadow resolution halved for performance).

### P2 — Medium Impact
- **No hit-stop on boss abilities.** Boss spawned event exists; it triggers camera shake but no audio swell or visual frame-freeze.
- **No synergy activation visual.** `OnSynergyActivated` fires but nothing subscribes — no screen flash, no particle burst, no UI callout for synergy triggers.
- **No combo/kill-streak escalation.** Damage numbers are flat; a scale ramp or color shift for successive hits in short windows would communicate momentum.

### P3 — Polish
- **Exploder pre-detonation charge-up missing** — no visual warning (pulsing glow, growing outline) before explosion radius activates.
- **Level transition screen is static** — gold/XP display has no counter animation.
- **No ambient particle systems** — background parallax or floating debris would reduce the empty-space feeling between waves.

---

## Part 3: Technical Art Issues

### BUG-001 — BLOCKING: DamageNumbers permanently disabled after 20 hits
**File:** `SYNTHBORN/Assets/Scripts/Core/VFX/DamageNumberSpawner.cs`
**Severity:** P0 — damage numbers silently stop appearing mid-run

`_activeCount` increments on every spawn but is **never decremented**. `Destroy(go, 1f)` destroys the GameObject after 1 second but nothing decrements the counter. After `_maxNumbers` (20) damage numbers have appeared across the entire run — even if all are long-destroyed — `SpawnNumber()` returns early forever.

**Fix required:** Decrement `_activeCount` when each damage number object is destroyed. Either subscribe a callback before `Destroy()`, or use a pool pattern where the number self-reports back on deactivation.

---

### BUG-002 — RENDERING: Built-in RP shader renders pink/magenta on URP
**File:** `SYNTHBORN/Assets/Scripts/Combat/VFX/DeathBurstVFX.cs`
**Severity:** P1 — all enemy death bursts render solid pink in URP

`Shader.Find("Particles/Standard Unlit")` returns a Built-in Render Pipeline shader. URP does not support Built-in RP shaders — the particle renderer will display magenta (URP's missing-shader fallback) at runtime.

Additionally, `new Material(shader)` is called on every enemy death, allocating a new material instance each time. This breaks SRP Batcher batching and generates GC pressure proportional to kill rate.

**Fix required:** Replace with a URP-compatible particle shader reference held as a `[SerializeField] Material _particleMaterial` field on the prefab. The `ChargerDeath.prefab` and all other death prefabs must have this field wired in the Inspector.

---

### ISSUE-001 — PERFORMANCE: Zero sprite atlases (highest-impact rendering gap)
**Severity:** P1

No `.spriteatlas` files exist in the project. Every sprite is an independent texture, generating one draw call per `SpriteRenderer` in the worst case (when materials differ). Even with the SRP Batcher reducing CPU overhead, individual texture binds still occur per unique texture.

For a survivor game with 20–50 enemies on screen simultaneously, atlasing enemy sprites, projectiles, and UI icons into 2–3 atlases (enemies atlas, projectiles atlas, UI atlas) will reduce draw calls substantially and improve cache coherence.

**Recommended atlases:**
- `EnemySprites.spriteatlas` — all enemy sprite sheets
- `ProjectileSprites.spriteatlas` — all projectile and pickup sprites
- `UIIcons.spriteatlas` — all HUD, mutation card, and inventory icons

---

### ISSUE-002 — PERFORMANCE: VFX systems bypass ObjectPool (GC spikes during combat)
**Severity:** P1

`ObjectPool.cs` exists but is not used by any VFX system. Three systems allocate per-frame during combat:

| System | Allocation per event | Expected frequency |
|--------|---------------------|-------------------|
| `DeathParticleSpawner` | 6× `new GameObject()` + `AddComponent<SpriteRenderer>()` + `AddComponent<DeathParticle>()` | Every enemy death |
| `DashTrail` | 1× `new GameObject()` + `AddComponent<SpriteRenderer>()` + `AddComponent<FadeAndDestroy>()` | Every 0.03s during dash |
| `DeathBurstVFX` | 1× `new Material()` | Every enemy death |

With 20+ enemy deaths per wave, the GC pressure is meaningful. All three should be converted to pool the GameObjects. `DashTrail` ghost sprites in particular fire at 33Hz during dashes — over a 0.2s dash that's ~7 allocations per dash at minimum.

---

### ISSUE-003 — RENDERING: HitFlash null renderer reference in Enemy.prefab
**File:** `SYNTHBORN/Assets/Prefabs/Enemy.prefab`
**Severity:** P2

`_renderer: {fileID: 0}` on the `HitFlash` component in `Enemy.prefab` — the serialized reference is not wired. `HitFlash.Awake()` falls back to `GetComponent<SpriteRenderer>()`, which works at runtime but is slower than a cached serialized reference and will silently break if the component hierarchy changes.

**Fix:** Wire `_renderer` to the `SpriteRenderer` on the Enemy prefab root in the Inspector.

---

### ISSUE-004 — STANDARDS VIOLATION: Assets in Resources/ folder
**Files:** `SYNTHBORN/Assets/Resources/SkillTreeData.asset`, `SYNTHBORN/Assets/Resources/ItemDatabase.asset`
**Severity:** P2

Both assets sit in `Resources/` and are presumably loaded via `Resources.Load()`. The project coding standards explicitly forbid `Resources.Load()` — Addressables is the approved pattern. These assets should be moved out of `Resources/` and accessed via direct `[SerializeField]` ScriptableObject references or registered as Addressables.

---

### ISSUE-005 — QUALITY: Legacy TextMesh used for damage numbers
**File:** `SYNTHBORN/Assets/Scripts/Core/VFX/DamageNumber.cs`
**Severity:** P2

`DamageNumber` uses `TextMesh` (legacy 3D world-space text mesh). This renders as a 3D mesh with no SRP Batcher support and inferior font rendering vs TextMeshPro. All UI text (`LevelTransitionScreen`, `SceneFader`, `MutationSelectionUI`) also uses `UnityEngine.UI.Text` instead of TextMeshPro.

**Fix:** Migrate damage numbers to `TextMeshPro` (world-space `TextMeshPro` component). Migrate UI text to `TextMeshProUGUI`. TextMeshPro is included in Unity 6 and is the recommended text solution.

---

### ISSUE-006 — PIPELINE: QualitySettings never customized for 2D
**File:** `SYNTHBORN/ProjectSettings/QualitySettings.asset`
**Severity:** P3

Six default quality tiers with 3D-oriented defaults remain unchanged (shadow cascades, skin weights, terrain pixel error, etc.). The game runs at quality tier 0 ("Very Low"). These 3D settings have no effect on a 2D game but consume configuration space and could cause confusion during platform optimization.

**Recommended:** Reduce to 2–3 custom tiers ("Low", "Medium", "High") with 2D-relevant settings only: target frame rate, particle count limits, and texture quality multipliers.

---

### ISSUE-007 — PIPELINE: Missing animations for 4 enemy types + Boss Walk
**Directory:** `SYNTHBORN/Assets/Art/Animations/`
**Severity:** P3

Clips found: Player (Idle, Walk), Chaser (Idle, Walk), Runner (Idle, Walk), Shooter (Idle, Walk), Exploder (Idle, Walk), Boss (Idle only).

Missing entirely:
- **Poisoner** — no Idle, no Walk
- **Summoner** — no Idle, no Walk
- **Charger** — no Idle, no Walk
- **Tank** — no Idle, no Walk
- **Boss Walk** — boss has Idle but no Walk clip

These enemies will display T-pose or first-frame-frozen sprites at runtime.

---

## Part 4: Prioritized Recommendations

| Priority | Issue | Effort | Impact |
|----------|-------|--------|--------|
| P0 | Fix `_activeCount` leak in `DamageNumberSpawner` | 30 min | Damage numbers re-enabled permanently |
| P0 | Fix Built-in RP shader in `DeathBurstVFX` + remove `new Material()` | 1 hour | Death particles render correctly |
| P1 | Create sprite atlases (enemies, projectiles, UI icons) | 2–3 hours | Major draw call reduction |
| P1 | Implement hit pause (1–2 frame timeScale micro-pause) | 1–2 hours | Highest game-feel ROI |
| P1 | Add global post-processing volume (bloom + color grade) | 1 hour | Transforms visual read of pixel art |
| P1 | Wire VFX systems to ObjectPool | 3–4 hours | Eliminates GC spikes in late-wave combat |
| P2 | Implement proper HitFlash shader with `_FlashAmount` + MaterialPropertyBlock | 2–3 hours | True white flash on all enemies |
| P2 | Add 2D lighting (player attack light, poison pool glow, boss ambient) | 3–4 hours | Atmosphere + readability |
| P2 | Migrate damage numbers and UI text to TextMeshPro | 2–3 hours | Text rendering quality |
| P2 | Move Resources/ assets to Addressables or direct SO references | 1–2 hours | Standards compliance |
| P2 | Wire `_renderer` in Enemy.prefab Inspector | 15 min | Defensive correctness |
| P3 | Add synergy activation VFX (subscriber to `OnSynergyActivated`) | 1–2 hours | Missing feedback for key mechanic |
| P3 | Exploder charge-up visual warning | 1–2 hours | Combat readability |
| P3 | Commission missing animations (Poisoner, Summoner, Charger, Tank, Boss Walk) | Art time | Required for shippable state |
| P3 | Simplify QualitySettings to 2D-appropriate tiers | 30 min | Pipeline hygiene |

---

## Appendix: File Reference

| File | Assessment Notes |
|------|-----------------|
| `Assets/Scripts/Core/VFX/DamageNumberSpawner.cs` | BUG-001: `_activeCount` never decremented |
| `Assets/Scripts/Combat/VFX/DeathBurstVFX.cs` | BUG-002: Built-in RP shader; `new Material()` per death |
| `Assets/Scripts/Core/VFX/HitFlash.cs` | Color tint, not true flash; renderer unwired in Enemy.prefab |
| `Assets/Scripts/Core/VFX/DeathParticleSpawner.cs` | Per-death allocation; ObjectPool unused |
| `Assets/Scripts/Core/VFX/DamageNumber.cs` | Legacy TextMesh |
| `Assets/Scripts/Core/CameraController.cs` | Well-implemented; unscaledDeltaTime correct |
| `Assets/Scripts/Core/VFX/ScreenShakeListener.cs` | Event-driven; crit multiplier present |
| `Assets/Scripts/Player/DashTrail.cs` | Per-ghost allocation; not pooled |
| `Assets/Scripts/Player/SpriteCompositor.cs` | Placeholder slot colors; awaiting real sprites |
| `Assets/Scripts/UI/MutationSelectionUI.cs` | Good stagger; uses legacy Text |
| `Assets/Scripts/UI/WaveBanner.cs` | Solid juice; unscaledDeltaTime correct |
| `Assets/Scripts/UI/SceneFader.cs` | Legacy Text for loading text |
| `Assets/Scripts/Core/Events/GameEvents.cs` | All VFX hooks present; Cleanup() correct |
| `Assets/Settings/Renderer2D.asset` | No renderer features; no post-processing |
| `Assets/Settings/UniversalRP.asset` | SRP Batcher on; no volume profile |
| `Assets/Prefabs/Enemy.prefab` | HitFlash renderer unwired |
| `Assets/Prefabs/VFX/ChargerDeath.prefab` | No pre-baked ParticleSystem data |
| `Assets/Resources/SkillTreeData.asset` | Standards violation: Resources/ folder |
| `Assets/Resources/ItemDatabase.asset` | Standards violation: Resources/ folder |
| `ProjectSettings/QualitySettings.asset` | 3D defaults; never customized for 2D |
