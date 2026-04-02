# ADR-007: Biome-Aware Adaptive Music

## Status
Accepted

## Date
2026-04-02

## Context

### Problem Statement
SYNTHBORN's Arena mode spans 100 rooms across 6 biomes. A flat global music system with one clip per intensity tier would fail to communicate biome identity — the Atrium should sound different from the Deep Archive. At the same time, the music must respond dynamically to gameplay state: calm during wave breaks and calibration intervals, escalating through intensity tiers as waves progress, spiking to a dedicated boss theme during boss encounters, and fading to silence on player death.

The architectural challenge is that biome audio data lives in `BiomeConfig : ScriptableObject` (owned by `Synthborn.Waves`), while the music manager lives in `Synthborn.Core.Audio`. A direct reference from `Core.Audio` to `Synthborn.Waves` would create a dependency from a lower layer to a higher one, violating the project's layer order (`Core` must not depend on `Waves`).

### Constraints
- `Synthborn.Core.Audio` must not reference `Synthborn.Waves` types. The dependency direction must remain: `Waves` → `Core`.
- The system must support seamless crossfading between intensity tiers with no audible cut.
- Biome overrides must not require scene reload — changing biome mid-session (entering a new chamber) must swap music immediately if calm, or on next transition if mid-combat.
- All wave thresholds, volume, and crossfade duration must be designer-configurable via a ScriptableObject.
- When no biome stems are provided for an intensity (null clip), the system must fall back to config defaults silently.
- Calibration intervals must always trigger a transition to the calm stem (biome calm if set, config default otherwise).

### Requirements
- Two `AudioSource` components on the same `GameObject` for ping-pong crossfading with no gap.
- A `MusicIntensity` enum (`Calm`, `Low`, `Medium`, `High`, `Boss`) mapping to both config stems and biome override slots.
- An explicit public injection API (`SetBiomeStems`) that accepts three nullable `AudioClip` references — no `BiomeConfig` reference in `AdaptiveMusicManager`.
- `TrialManager` (or any `Synthborn.Waves` caller) calls `SetBiomeStems()` when entering a new chamber, passing clips from the relevant `BiomeConfig`.
- `SFX procedural fallback`: when a biome or config clip is null for a given intensity, stop the active source and proceed silently rather than throwing an error.

## Decision

The system is split into two classes and one enum.

`AdaptiveMusicConfig : ScriptableObject` holds the five default looping `AudioClip` stems (`calmStem`, `lowStem`, `mediumStem`, `highStem`, `bossStem`), `masterVolume`, `crossfadeDuration`, and the two wave-number thresholds that map wave indices to intensity tiers (`lowToMediumWave`, `mediumToHighWave`). It also exposes `GetStem(MusicIntensity)` and `GetIntensityForWave(int)` query helpers. This class has zero runtime state and zero Unity lifecycle overhead.

`AdaptiveMusicManager : MonoBehaviour` owns two `AudioSource` components created in `Awake()` (not serialised) and a set of three nullable `AudioClip` fields for biome overrides (`_biomeCalmOverride`, `_biomeCombatOverride`, `_biomeBossOverride`). The manager subscribes to `GameEvents` in `OnEnable`/`OnDisable`.

**`SetBiomeStems()` injection API.** `TrialManager.StartChamber()` locates the `AdaptiveMusicManager` via `FindAnyObjectByType` and calls `SetBiomeStems(calmStem, combatStem, bossStem)` with the three clips from `CurrentBiomeConfig`. This call site is in `Synthborn.Waves`, so the dependency direction is correct. `AdaptiveMusicManager` receives raw `AudioClip` values — it has no knowledge of `BiomeConfig` or `BiomeLayer`.

**Clip resolution chain.** `ResolveClip(MusicIntensity)` applies the override cascade:
- `Calm`: biome calm override → config calm stem
- `Boss`: biome boss override → biome combat override → config boss stem (boss falls back to combat, combat falls back to config)
- `Low/Medium/High`: biome combat override → config tier stem (all combat intensities share one biome stem)

This cascade means a biome only needs to provide one `biomeCombatStem` to cover all three combat intensity tiers, while still allowing a dedicated `biomeBossStem` for dramatic boss fights.

**Null clip handling (SFX procedural fallback).** When `ResolveClip` returns null for the target intensity, `TransitionTo()` stops the active source rather than starting a crossfade. This silences music gracefully — acceptable for early-content biomes whose audio assets are not yet authored.

**CalibrationInterval → Calm transition.** `OnCalibrationInterval()` calls `TransitionTo(MusicIntensity.Calm)` unconditionally. If a biome calm override is set, the resolved clip will be the biome's ambient, creating a biome-specific calibration atmosphere.

**Player death fade.** `OnPlayerDied()` cancels any running crossfade coroutine and starts `FadeOut()`, which linearly ramps both sources to zero volume using `Time.unscaledDeltaTime` before stopping and clearing them. `ClearBiomeStems()` is also called so a scene reload starts with no stale overrides.

### Architecture Diagram

```
Synthborn.Waves                          Synthborn.Core.Audio
─────────────────                        ────────────────────
BiomeConfig (SO)                         AdaptiveMusicConfig (SO)
  biomeCalmStem    ──┐                     calmStem / lowStem
  biomeCombatStem  ──┤                     mediumStem / highStem / bossStem
  biomeBossStem    ──┘                     masterVolume / crossfadeDuration
                      │                   lowToMediumWave / mediumToHighWave
TrialManager           │
  StartChamber()  ─────┼──► AdaptiveMusicManager.SetBiomeStems(calm, combat, boss)
                        │         │
                        │         │  _biomeCalmOverride    ┐
                        │         │  _biomeCombatOverride  ├─► ResolveClip(intensity)
                        │         │  _biomeBossOverride    ┘        │
                        │         │                                  │ nullable fallback
                        │         │                          _config.GetStem(intensity)
                        │
                      GameEvents (subscriptions in OnEnable/OnDisable)
                        OnWaveStarted    → TransitionTo(config.GetIntensityForWave(n))
                        OnWaveCleared    → TransitionTo(Calm)
                        OnBossSpawned    → TransitionTo(Boss)
                        OnBossDefeated   → TransitionTo(Calm)
                        OnCalibration    → TransitionTo(Calm)
                        OnPlayerDied     → FadeOut() + ClearBiomeStems()

                      Two AudioSources (ping-pong crossfade)
                        _sourceA ↔ _sourceB  (Crossfade coroutine)
                        _aIsActive flag tracks which is foreground
```

### Key Interfaces

```csharp
// Synthborn.Core.Audio — intensity tier enum
public enum MusicIntensity { Calm, Low, Medium, High, Boss }

// Synthborn.Core.Audio — config asset (no runtime state)
[CreateAssetMenu]
public class AdaptiveMusicConfig : ScriptableObject
{
    public AudioClip calmStem, lowStem, mediumStem, highStem, bossStem;
    public AudioClip menuStem;
    public float masterVolume;          // default 0.4
    public float crossfadeDuration;     // default 1.5 s
    public int   lowToMediumWave;       // default 4
    public int   mediumToHighWave;      // default 7

    public AudioClip      GetStem(MusicIntensity intensity);
    public MusicIntensity GetIntensityForWave(int waveNumber);
}

// Synthborn.Core.Audio — runtime manager
public class AdaptiveMusicManager : MonoBehaviour
{
    // Biome injection API (called by Synthborn.Waves callers)
    public void SetBiomeStems(AudioClip calmStem, AudioClip combatStem, AudioClip bossStem);
    public void ClearBiomeStems();
    // All other behaviour is driven by GameEvents subscriptions
}

// Synthborn.Waves — call site (in TrialManager.StartChamber)
var musicManager = FindAnyObjectByType<AdaptiveMusicManager>();
if (musicManager != null && CurrentBiomeConfig != null)
    musicManager.SetBiomeStems(
        CurrentBiomeConfig.biomeCalmStem,
        CurrentBiomeConfig.biomeCombatStem,
        CurrentBiomeConfig.biomeBossStem);
```

## Alternatives Considered

### Alternative 1: Direct BiomeConfig Reference in AdaptiveMusicManager
- **Description**: `AdaptiveMusicManager` holds a `[SerializeField] BiomeConfig[] _biomeConfigs` array and looks up the active biome itself by subscribing to a `GameEvents.OnBiomeChanged` event.
- **Pros**: Biome-to-music wiring is self-contained in the audio manager; no external caller needed.
- **Cons**: Requires adding `Synthborn.Waves` to `Synthborn.Core`'s asmdef references, reversing the intended dependency direction. `Core` would depend on `Waves`, creating a circular path if `Waves` ever needs to reference `Core` audio types.
- **Rejection Reason**: Violates the architectural layer constraint. The injection API (`SetBiomeStems`) preserves the `Waves → Core` dependency direction by having the `Waves` layer push data into `Core`, not the reverse.

### Alternative 2: Unity Audio Mixer with Exposed Parameters
- **Description**: Use Unity's `AudioMixer` with exposed float parameters for volume fading, and author biome music as separate Mixer Groups. `TrialManager` sets mixer parameters when changing biomes.
- **Pros**: Mixing happens in the audio subsystem at native performance; AudioMixer supports complex routing, reverb sends, and ducking that `AudioSource` scripts cannot match.
- **Cons**: Requires authoring and maintaining an AudioMixer asset with one group per biome per intensity tier (6 biomes × 5 tiers = 30 groups). The crossfade logic would still need scripting to drive mixer volume parameters. The `AdaptiveMusicConfig` ScriptableObject approach is simpler and sufficient for the current scope.
- **Rejection Reason**: Deferred to a future audio polish sprint. The two-AudioSource crossfade covers all current requirements with less asset overhead. An AudioMixer migration would be justified if surround mixing, reverb zones, or more than 2 simultaneous stems are needed.

### Alternative 3: Single AudioSource with clip.SetScheduledEndTime (Scheduled Playback)
- **Description**: Pre-schedule the next stem to begin exactly when the current one ends using `AudioSource.PlayScheduled` and `AudioSource.SetScheduledStartTime`.
- **Pros**: Sample-accurate transitions with zero crossfade gap.
- **Cons**: Scheduled transitions cannot be interrupted mid-clip for reactive state changes (e.g., boss spawning mid-wave). SYNTHBORN's music must transition at any moment in response to gameplay events. The scheduled approach is suited to pre-choreographed music, not reactive adaptive music.
- **Rejection Reason**: Reactive crossfading on-demand requires the two-source model. Scheduled playback is incompatible with interrupt-driven state transitions.

## Consequences

### Positive
- `AdaptiveMusicManager` is dependency-free from `Synthborn.Waves` — it can be tested in a minimal scene with only a config asset.
- Adding a new biome requires only populating the three audio clip fields on the biome's `BiomeConfig` asset. No code changes needed.
- The single `_biomeCombatStem` field covers all three combat intensity tiers (Low, Medium, High) for a biome, reducing the number of audio assets required per biome while still allowing escalation via the config default tiers as fallback.
- `null` biome or config clips silence music gracefully, making early-content biomes with no audio assets safe to run.
- All timing is `unscaledDeltaTime`, so crossfades proceed correctly during slow-motion or pause states.

### Negative
- `FindAnyObjectByType<AdaptiveMusicManager>()` in `TrialManager.StartChamber()` is a scene search performed every time a chamber starts. This is a known performance cost, acceptable at chamber-start frequency (at most once per minute). A future sprint could wire the reference via `GameBootstrap` injection to eliminate the search.
- `SaveManager.Data.musicVolume` is read directly inside `Crossfade()` and `TransitionTo()`. If `SaveManager` is unavailable (e.g., in an isolated test scene), this will throw a null reference. The system should guard this access in a future hardening pass.
- The three-slot biome override design (calm/combat/boss) assumes biomes use at most three distinct music moods. Biomes with unique Low/Medium/High variants (e.g., an escalating narrative biome) would need the API extended to accept five separate stems.

### Risks
- **Crossfade interrupted by rapid state transitions.** If `OnWaveCleared` and `OnWaveStarted` fire within `crossfadeDuration` of each other, the coroutine is stopped and restarted. The `StopCoroutine` guard in `TransitionTo()` prevents two coroutines running simultaneously, but the volumes of both sources may be left in an intermediate state. Mitigation: The new crossfade coroutine captures the current volumes of both sources and interpolates from there, not from fixed 0/1 endpoints — handled correctly in the current implementation.
- **`_config == null` at runtime.** If the `[SerializeField] _config` reference is not assigned, most methods guard with `if (_config == null) return`. Music is silenced silently. Mitigation: Add a `Debug.LogError` in `Awake()` when `_config` is null.
- **Biome stems not cleared on scene reload.** If the game scene is reloaded without triggering `OnPlayerDied` (e.g., direct `SceneManager.LoadScene` from another code path), `_biomeCalmOverride` etc. may retain stale clips. Mitigation: `ClearBiomeStems()` is public and should be called from any scene-exit path in `TrialManager` or `GameBootstrap`.

## Performance Implications
- **CPU**: Two `AudioSource.volume` writes per frame during a crossfade (one per source). These are O(1) engine calls. No crossfade is active during steady gameplay, only during transitions.
- **Memory**: Two `AudioSource` components per `AdaptiveMusicManager` instance. Each loaded `AudioClip` is stored once in memory; multiple sources playing the same clip share the compressed audio data. For 6 biomes × 3 stems + 5 config stems = 23 clips at a typical 2 MB each, steady-state audio memory is approximately 46 MB if all clips are resident. Addressable-based streaming per-biome is the recommended future optimisation.
- **Load Time**: `AdaptiveMusicConfig` is a lightweight SO. The `AudioClip` assets themselves load on first `AudioSource.clip` assignment; no synchronous load in `Awake()`.
- **Network**: Not applicable.

## Validation Criteria
- Entering a biome with a non-null `biomeCalmStem`: after `SetBiomeStems()`, the calm music transitions to the biome clip within `crossfadeDuration` seconds.
- Entering a wave starts a crossfade to the config intensity for that wave number (using `GetIntensityForWave`).
- Boss spawn transitions to the boss stem; boss defeat transitions back to calm within `crossfadeDuration`.
- Calibration interval start always plays the calm stem (biome calm if set).
- A biome with all null stems produces silence without errors or exceptions.
- After `OnPlayerDied`, both sources are at volume 0 and stopped within `crossfadeDuration`.
- `ClearBiomeStems()` followed by `SetBiomeStems(null, null, null)`: subsequent calm transition plays the config calm stem, not silence.

## Related Decisions
- ADR-001: Core Architecture Patterns — EventBus pattern (`GameEvents`) used for all state-change subscriptions; ScriptableObject config pattern for `AdaptiveMusicConfig`.
- ADR-002: MVP Combat Systems Architecture — defines the wave and boss events (`OnWaveStarted`, `OnBossSpawned`) that drive music intensity transitions.
- Design document: `design/gdd/arena-biomes.md` — defines the six biome identities that motivate per-biome music stems.
