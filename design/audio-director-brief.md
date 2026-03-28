# Audio Director Brief: SFX Specification Complete

**Date**: 2026-03-27
**For**: Audio Director
**Status**: Ready for review and sign-off

---

## Executive Summary

A complete SFX specification document has been created for SYNTHBORN, covering all 40 target MVP sounds with mixing, variation, and audio aging strategies. The spec is production-ready and provides everything an audio engineer needs to begin creating assets.

**Document Location**: `/design/gdd/sfx-specification.md`

---

## Key Features of the Specification

### 1. Complete Event Table (40 sounds)

All gameplay moments covered:

- **Player actions**: Hit, dash, death, level-up, mutations, damage taken (8 sounds)
- **Combat/projectiles**: Fire, impact (weak/strong), explosions, enemy attacks, crits, synergy (10 sounds)
- **Enemy actions**: Spawn, death (normal/elite), boss, alerts, wave mechanics (8 sounds)
- **Progression**: XP pickups, game over, achievements, pause/resume (6 sounds)
- **UI**: Button interactions, card reveals, notifications (7 sounds)
- **Ambience**: Arena hum loop (1 sound)

Each sound includes:
- Detailed sonic description (waveform character, emotional intent)
- Duration range (in milliseconds)
- Volume level (0.0–1.0)
- Pitch variation range (semitones, for randomization)
- Max concurrent instances (concurrency limits)
- Target mix bus
- Implementation notes (audio aging directives, clip naming)

### 2. Audio Aging System (Core Feature)

The palette evolves across the player's run, reflecting the game's core fantasy of biological mutation:

| Mutation Count | Phase | SFX Character | Master EQ / Effects |
|---|---|---|---|
| **0–4** | **Clinical** | Pure synthesis, sharp attacks, minimal reverb | Flat EQ, no compression |
| **5–9** | **Early Decay** | Subtle harmonics, slight reverb tail, crisp envelope | +2 dB @ 2 kHz, soft limiter |
| **10–14** | **Mid-Mutation** | Complex tones, noisy textures, 0.4–0.8 sec reverb | +3 dB @ 250 Hz, –2 dB @ 5 kHz, compression 3:1 |
| **15+** | **Biological** | Distorted, organic, wet, long reverb (1+ sec) | +4 dB @ 150 Hz, –3 dB @ 4 kHz, compression 5:1, chorus |

**Design moment sounds** (level-up jingle, synergy activation, boss intros) are always "clinical" — no aging — emphasizing precision at narrative peaks.

### 3. Mixing Structure (4-Bus Hierarchy)

```
Master (output, limiter)
├── Music (–6 dB, ducks to –14 dB during combat)
├── SFX (–3 dB, primary payload, no ducking)
│   └── Ambient (–12 dB sub-bus)
└── UI (–8 dB, always audible)
```

**Snapshots** handle intensity transitions:
- **Default**: Normal gameplay mix
- **Music_Intense**: Music ducks when 5+ SFX active
- **UI_Focused**: Music/ambient mute during mutation selection
- **Aging_5/10/15Mut**: EQ + compression update at aging thresholds

### 4. Variation Strategy

**High-repetition sounds** (projectile fire, enemy death) get **3 aging variants** each (clean/mid/organic):
- **Clinical variant**: Pure synthesis (dry, sharp, minimal reverb)
- **Mid variant**: Bridging (slight texture, brief reverb, soft envelope)
- **Organic variant**: Biological (wet reverb, distortion, rich harmonics)

**Runtime behavior**:
- At mutation count 5, 10, 15: Snapshot transition swaps variant set
- Within phase (0–4 mut): Pitch randomization (±1–2 semitones) applied per instance for variety
- High-instance sounds (6+ concurrent) use round-robin to avoid exact repetition

### 5. Asset Naming & Organization

All clips follow: `[category]_[event]_[variant].wav`

**Example**:
- `player_hit_melee_clean.wav`
- `player_hit_melee_mid.wav`
- `player_hit_melee_organic.wav`
- `projectile_fire_clean.wav`
- `enemy_death_normal_organic.wav`

**Folder structure** in `assets/audio/`:
```
sfx/
├── player/
├── combat/
├── enemy/
├── progression/
├── ui/
└── ambient/
```

### 6. Integration with GameEvents

All SFX are tied to existing event system — no new infrastructure needed:

```
GameEvents.OnPlayerDamaged → PlayPlayerDamageSFX()
GameEvents.OnEnemyDied → PlayEnemyDeathSFX()
GameEvents.OnLevelUp → PlayLevelUpJingle()
GameEvents.OnMutationSelected → PlayMutationSFX()
GameEvents.OnWaveStarted → PlayWaveStartSFX()
GameEvents.OnWaveCleared → PlayWaveClearSFX()
GameEvents.OnPlayerDied → PlayGameOverSFX()
```

An `AudioManager.cs` pseudocode skeleton is provided in the spec.

### 7. MVP Priority Tiers

Sounds organized by ship criticality:

| Tier | Count | Sounds | MVP? |
|---|---|---|---|
| **Critical** | 8 | Player hit, level-up, mutations, projectile fire, enemy death, player death, game over | YES |
| **High** | 10 | Dash, strong impact, synergy, spawn, wave start/clear, XP pickup, button click, ambience | TARGET |
| **Medium** | 12 | Damage feedback, explosions, enemy attacks, pause/resume, card reveal | POLISH |
| **Low** | 10 | Crits, shield, alerts, portals, achievements, phases | FUTURE |

**Estimated assets**: 45–54 clips total (Tiers 1–2 only = ~24–32 clips for hard MVP).

---

## Waveform & Synthesis Guidelines (In Appendix)

For audio engineers, the spec includes:

- **Waveform reference table**: Sine, square, triangle, sawtooth, white/pink/brown noise use cases
- **Envelope templates**: Attack/decay/sustain/release (ADSR) presets for each sound type
- **Reverb IR suggestions**: Small room (0.3–0.5 sec) for mid phase, hall (0.8–1.5 sec) for organic
- **Inspiration sources**: Tron (minimalist), Synthwave artists, The Thing, Akira, game references

---

## Mixing Checklist (Sign-Off Criteria)

Before marking complete, verify:

- [ ] SFX audible above music/ambience (levels balanced)
- [ ] All variants (clean/mid/organic) have consistent EQ character
- [ ] Reverb uses unified impulse response (single convolver IR)
- [ ] Music ducking smooth, no pumping artifacts
- [ ] Organic variants don't clip; –3 dB headroom minimum
- [ ] Stereo width consistent (mono SFX, full-width ambience)
- [ ] Aging transitions seamless (<100 ms crossfade)
- [ ] Ambience loop has zero clicks at loop point
- [ ] 8 simultaneous SFX play without artifacts
- [ ] Master loudness in spec (–23 LUFS broadcast standard)

---

## Open Questions for Audio Director

1. **Settings**: Single "SFX Master" volume, or per-category sliders?
   - *Recommendation*: Single master + preset EQ (Normal, Quiet, Bass-Light)

2. **Synergy Sounds**: One generic "Synergy Activation" jingle, or unique per synergy?
   - *Recommendation*: MVP = generic dramatic sound; Vertical Slice+ = per-synergy custom

3. **Mono vs Stereo**: Should SFX be mono (clarity) or stereo (space)?
   - *Recommendation*: Mono SFX (smaller file size, cleaner mix); stereo music/ambience

4. **Reverb IR**: Pre-recorded impulse response, algorithmic, or off-the-shelf?
   - *Recommendation*: Single convolver IR (e.g., small hall ~1.0 sec decay) for tonal unity

5. **Damage Masking**: Should player-damage SFX duck other SFX, or stay sharp always?
   - *Recommendation*: No ducking; hazard warnings must be crisp

6. **Audio Format**: WAV for all SFX, or OGG for music streaming?
   - *Recommendation*: WAV (short SFX), OGG (long music tracks)

---

## Production Timeline Estimate

- **Critical Tier (8 sounds × 3 variants)**: ~2 weeks
  - 1 week: Clean variant synthesis
  - 1 week: Mid + organic variants (aging, reverb, distortion)

- **High Tier (10 sounds × 2–3 variants)**: ~1.5 weeks
  - Parallel to critical if resources available

- **Integration Testing**: ~1 week
  - AudioManager.cs wiring, GameEvents subscription, snapshot transitions, concurrency testing

- **Total**: 3–4 weeks (critical + high, sequential) or 2–3 weeks (parallel)

**Deliverable**: 45–54 audio clips + AudioManager.cs integration + mixing sign-off

---

## Next Steps

1. **Review & Approve**: Audio director signs off on spec (event table, mixing, aging strategy, variants)
2. **Produce Critical Tier**: Audio engineer creates 8 sounds × 3 variants (24 clips)
3. **Integrate**: Programmer wires AudioManager.cs to GameEvents, tests concurrency
4. **Produce High Tier**: Audio engineer creates 10 sounds × 2–3 variants (20–30 clips) in parallel
5. **Mix Balance**: Audio director balances levels, verifies aging transitions
6. **Sign-Off**: QA verifies mixing checklist; audio director approves loudness + tonal character

---

**Status**: ✅ Specification complete, production-ready.
**Document**: See `/design/gdd/sfx-specification.md` for full details.

