# SFX Specification: SYNTHBORN

> **Status**: Designed
> **Author**: Claude Code Game Studios (Sound Designer)
> **Last Updated**: 2026-03-27
> **Audio Middleware**: Native Unity Audio (AudioSource + AudioMixer)
> **Music Style**: Dark Synthwave / Ambient Hybrid (NOT chiptune)
> **SFX Palette**: Clinical Synthesis → Biological Disruption (aging with mutations)
> **Target MVP**: 35-40 critical SFX
> **Spatial**: 2D (no 3D spatialization for MVP)

---

## 1. Mixing Group Definitions

Audio bus structure for Unity AudioMixer. All groups route through Master.

| Group Name | Parent | Base Level (dB) | Ducking Rules | Snapshot Integration |
|------------|--------|-----------------|----------------|----------------------|
| **Master** | — | 0 dB | — | Main output |
| **Music** | Master | -6 dB | Ducks -8 dB during combat/boss phases | Music_Intense snapshot |
| **SFX** | Master | -3 dB | No ducking (SFX is primary) | SFX_HeavyAction snapshot |
| **UI** | Master | -8 dB | No ducking (UI always audible) | UI_Focused snapshot |
| **Ambient** | SFX | -12 dB | Fades in/out with environment | Ambient_Decay snapshot |

**Snapshot Details:**
- **Music_Intense**: Music → -6 dB (default) OR -14 dB (during boss combat / wave intensity >8)
- **SFX_HeavyAction**: Activates when 5+ simultaneous SFX play; compressor on Master
- **UI_Focused**: Used during mutation selection screen; Music → -12 dB, Ambient → -18 dB
- **Ambient_Decay**: Ambient fades out over 2 sec during player death or game over

---

## 2. Complete SFX Event Table

**Column Explanations:**
- **ID**: Reference identifier for implementation
- **Event Name**: Display name (used in code comments and documentation)
- **Category**: Gameplay context (Player, Combat, Enemy, Progression, UI, Ambient)
- **Priority**: Critical (MVP launch blocker) | High (target MVP) | Medium (polish/secondary) | Low (nice-to-have)
- **Sound Description**: Detailed sonic character and emotional intent
- **Duration (ms)**: Expected length; [min–max] if variable
- **Volume**: Relative loudness (0.0–1.0, where 0.5 = -6 dB from nominal)
- **Pitch Var**: Range for random pitch shifts (in semitones); 0 = no variation
- **Max Instances**: Concurrency limit (prevent audio stack overflow)
- **Mix Group**: Target AudioMixer group
- **Notes**: Implementation hints, waveform suggestions, audio aging rules

### PLAYER ACTIONS (8 SFX)

| ID | Event Name | Category | Priority | Sound Description | Duration | Volume | Pitch Var | Max Inst | Mix Group | Notes |
|----|------------|----------|----------|-------------------|----------|--------|-----------|----------|-----------|-------|
| P001 | Player Hit (Melee) | Player | Critical | Percussive metallic thud. Synthesized: short attack (<50ms) sine sweep 150→80Hz, heavy envelope. Feeling: weighty blade strike. Aging: +5mut → add organic squish tail (10ms). +15mut → wet biological slap. | 120–180 | 0.75 | ±2 | 3 | SFX | Triggered by Auto-Attack melee. Stack with projectiles only. Clip: `player_hit_melee_[clean/mid/organic]` |
| P002 | Player Dash | Player | Critical | Whoosh + electrical pop. Synth: sweep 2000→500Hz (40ms) + click (sine 8000Hz, 15ms). Feeling: quick aggressive burst, digital. Aging: +5mut → add growl undertone. +15mut → squishy swoosh. | 160–220 | 0.65 | ±1 | 2 | SFX | Triggered by Player dash action. No interruption on re-cast. |
| P003 | Player Death | Player | Critical | Descending pitch with dissonance. Synth: major chord (300Hz base) → minor (add 50 cent detuned notes), frequency sweep 600→200Hz (300ms), reverb tail. Feeling: catastrophic failure. Aging: +5mut → longer reverb tail. +15mut → wet organic decay. | 450–600 | 0.85 | 0 | 1 | SFX | Triggered by GameEvents.PlayerDied. Pause on playback. Volume override: always audible. |
| P004 | Player Level-Up Jingle | Progression | Critical | Ascending arpeggiated synth. 4-note sequence: E4 → G4 → B4 → E5 (each 100ms, staggered attack). Feeling: triumphant, rewarding. Aging: constant (no mutation aging). | 400–500 | 0.80 | 0 | 1 | UI | Triggered by GameEvents.LevelUp. Often paired with mutation selection screen music fade. |
| P005 | Player Damage Taken (Light) | Player | High | Quick beep + brief whoosh. Synth: sine pulse 600Hz (80ms) + noise sweep down. Feeling: alert, minor threat. Aging: +5mut → add reverb. +15mut → organic fleshy sound. | 100–150 | 0.60 | 0 | 2 | SFX | Triggered by GameEvents.PlayerDamaged (dmg < 10). Stack up to 2. |
| P006 | Player Damage Taken (Heavy) | Player | High | Lower-pitched thud + pitch drop. Synth: sine 300Hz attack, sweep down to 150Hz (200ms), brief reverb. Feeling: serious threat, pain. Aging: +10mut → add wet distortion. +15mut → biological groan undertone. | 250–350 | 0.75 | 0 | 2 | SFX | Triggered by GameEvents.PlayerDamaged (dmg >= 10). Interrupts "Light" damage SFX. |
| P007 | Mutation Slot Applied | Progression | Critical | Transformation shimmer + burst. Synth: upward sweep 300→1200Hz (150ms) + filtered noise burst (white noise, high-pass filtered, 100ms envelope). Feeling: evolution, power gain. Aging: constant (evolution is always "clean/clinical" in this design). | 250–350 | 0.70 | 0 | 1 | UI | Triggered by GameEvents.MutationSelected when slot mutation chosen. Pair with VFX morph. |
| P008 | Mutation Passive Applied | Progression | Medium | Short pickup sound. Synth: sine pulse 800Hz (50ms) + decay. Feeling: bonus, stat boost. Aging: constant. | 80–120 | 0.55 | 0 | 2 | UI | Triggered by GameEvents.MutationSelected when passive mutation chosen. Gentler than slot mutation. |

### COMBAT / PROJECTILES (10 SFX)

| ID | Event Name | Category | Priority | Sound Description | Duration | Volume | Pitch Var | Max Inst | Mix Group | Notes |
|----|------------|----------|----------|-------------------|----------|--------|-----------|----------|-----------|-------|
| C001 | Projectile Fire (Basic) | Combat | Critical | Zippy pop + brief resonance. Synth: sharp attack sine 1200Hz (30ms) + filtered tail (band-pass peak 800Hz, 50ms). Feeling: quick, light, digital. Aging: +5mut → add organic breath. +15mut → wet squelchy launch. | 100–140 | 0.65 | ±1 | 6 | SFX | Triggered per projectile spawn. Max 6 concurrent (saturates at heavy fire). Clip: `projectile_fire_[clean/mid/organic]` |
| C002 | Projectile Impact (Weak) | Combat | High | Click + fizz. Synth: sine click 2000Hz (20ms) + white noise decay (filtered high-pass, 60ms). Feeling: light hit, no critical mass. Aging: +5mut → add tone to fizz. +15mut → splat sound. | 80–120 | 0.60 | ±2 | 4 | SFX | Triggered per projectile hit on enemy. Avoid double-stacking with enemy death. |
| C003 | Projectile Impact (Strong/Crit) | Combat | High | Deep thud + resonant decay. Synth: sine 400Hz attack, harmonic ring (add 800Hz + 1200Hz sine overtones), decay 150ms. Feeling: satisfying hit, momentum. Aging: +5mut → add wet reverb. +15mut → organic wet thud. | 150–220 | 0.75 | 0 | 3 | SFX | Triggered when crit or high-damage projectile hits. Louder/longer than C002. |
| C004 | Explosive Impact (Area Damage) | Combat | High | Boom + sub rumble. Synth: low sine sweep 200→80Hz (150ms) + filtered noise burst (brown noise, 200ms). Feeling: devastation, area effect. Aging: +5mut → add mid-range distortion. +15mut → wet squelchy boom. | 300–400 | 0.80 | 0 | 2 | SFX | Triggered by area-of-effect projectiles (if implemented in MVP). Max 2 concurrent. Pair with screen shake. |
| C005 | Enemy Melee Attack (Swipe) | Combat | Medium | Airy swish + brief tone. Synth: pink noise sweep 1000→2000Hz (80ms) + sine 400Hz undertone (20ms). Feeling: motion, threat. Aging: +5mut → add reverb/wet. +15mut → organic growl undertone. | 100–150 | 0.50 | ±1 | 2 | SFX | Triggered when enemy performs melee attack animation. Can stack (multiple enemies). |
| C006 | Enemy Ranged Attack (Shoot) | Combat | Medium | Zippy launch + energy pop. Synth: sine sweep 1500→600Hz (40ms) + filtered noise pop (band-pass 1200Hz, 50ms). Feeling: energy projectile, hostile. Aging: +5mut → add texture. +15mut → organic crackle. | 110–160 | 0.55 | ±1 | 3 | SFX | Triggered when ranged enemy fires. Stack with player projectiles, not each other. |
| C007 | Crit Hit Indicator | Combat | High | Bright ding + shimmer. Synth: sine 1600Hz (60ms) + high-frequency noise shimmer (5000Hz, 80ms, filtered). Feeling: bonus damage, status boost. Aging: constant (critical hits are always "clean" — precision). | 120–180 | 0.70 | 0 | 2 | SFX | Triggered when projectile crit proc. Pair with damage number VFX. Can overlap with impact SFX. |
| C008 | Hit Absorption / Shield | Combat | Medium | Deflection tone + reverb. Synth: sine 800Hz pulse (50ms) + reverb tail (convolver IR: metallic resonance). Feeling: protection, bounce. Aging: +5mut → dampen/muddy reverb. +15mut → organic thud. | 140–200 | 0.65 | 0 | 2 | SFX | Triggered if armor/shield system implemented. Not critical for MVP but reserved. |
| C009 | Explosion Pop (from dying enemy) | Combat | Critical | Percussive burst + decay. Synth: sine sweep 600→200Hz (100ms) + filtered noise burst (white noise, high-pass filtered, 120ms). Feeling: finality, satisfying. Aging: +5mut → add wet tail. +15mut → squishy organic pop. | 150–200 | 0.70 | ±1 | 8 | SFX | Triggered by enemy death (linked to VFX particle burst). Heavy stacking OK (satisfying). Clip: `explosion_pop_[clean/mid/organic]` |
| C010 | Synergy Activation | Combat | Critical | Ascending shimmer + sustained tone + boom. Synth: swept sine 800→2000Hz (300ms) + held tone 1200Hz (200ms) + low sine 150Hz boom (100ms). Feeling: momentous, gamechanging, transcendent. Aging: constant (synergy is always "clean" — design moment). | 600–800 | 0.85 | 0 | 1 | SFX | Triggered by GameEvents when synergy detected. Pair with slow-mo + VFX flash. Unique, memorable sound. |

### ENEMY ACTIONS (8 SFX)

| ID | Event Name | Category | Priority | Sound Description | Duration | Volume | Pitch Var | Max Inst | Mix Group | Notes |
|----|------------|----------|----------|-------------------|----------|--------|-----------|----------|-----------|-------|
| E001 | Enemy Spawn | Enemy | Medium | Quick rise + presence. Synth: sine sweep 200→600Hz (100ms) + white noise texture (100ms). Feeling: appearance, threat entry. Aging: +5mut → add wetness. +15mut → organic emergence sound. | 120–180 | 0.60 | ±1 | 4 | SFX | Triggered when enemy spawned. Stack OK (wave arrivals). Clip: `enemy_spawn_[clean/mid/organic]` |
| E002 | Enemy Death (Normal) | Enemy | Critical | Pitched decay + fizz. Synth: sine sweep 400→100Hz (200ms) + filtered white noise decay (high-pass, 150ms). Feeling: defeat, demise. Aging: +5mut → add reverb. +15mut → wet squishy death rattle. | 250–350 | 0.70 | ±2 | 6 | SFX | Triggered by GameEvents.EnemyDied. Often stacks (multiple kills). Heavy instances OK. Clip: `enemy_death_[clean/mid/organic]` |
| E003 | Elite Enemy Spawn | Enemy | High | Deeper, more menacing spawn. Synth: sine sweep 100→400Hz (150ms) + low harmonic rumble (80Hz sine, 100ms). Feeling: danger, power presence. Aging: +5mut → add reverb. +15mut → wet growl. | 180–250 | 0.70 | 0 | 2 | SFX | Triggered when elite/mini-boss enemy enters arena. Distinct from normal spawn. |
| E004 | Elite / Boss Death | Enemy | Critical | Extended resonant decay + low rumble. Synth: complex sine chord (150Hz base + 225Hz + 300Hz) swept down to 75Hz over 400ms, reverb tail 600ms. Feeling: momentous victory, power release. Aging: +5mut → add wet distortion. +15mut → biological death wail. | 1000–1200 | 0.85 | 0 | 1 | SFX | Triggered on boss/elite death. Pair with slow-mo + VFX cinematic. Long, memorable. Clip: `elite_death_[clean/mid/organic]` |
| E005 | Wave Start / Alarm | Enemy | High | Ascending alarm tone + siren. Synth: sine sweep 600→1200Hz (300ms, repeating pulse 2x) + harmonic rise. Feeling: urgency, new threat phase. Aging: constant (design moment, always "clinical"). | 600–800 | 0.75 | 0 | 1 | SFX | Triggered by GameEvents.WaveStarted. Pair with music transition to intense. |
| E006 | Wave Clear / Victory | Progression | High | Descending resolved chord + bell. Synth: major chord (C4 300Hz + E4 330Hz + G4 400Hz) swept down 300Hz over 300ms, decaying bell tone (sine 600Hz, 400ms reverb). Feeling: triumph, challenge passed. Aging: constant. | 700–900 | 0.80 | 0 | 1 | SFX | Triggered by GameEvents.WaveCleared. Pair with music resolution. |
| E007 | Enemy Alert / Aggro | Enemy | Medium | Quick ascending beep + warning. Synth: sine 900Hz pulse (100ms) + rising swept sine 600→1000Hz (100ms). Feeling: enemy noticing player, threat. Aging: +5mut → add texture. +15mut → organic snarl. | 120–180 | 0.55 | 0 | 3 | SFX | Reserved for future AI aggro system. Not critical MVP. |
| E008 | Spawner / Portal Active | Enemy | Low | Looping hum + pulse. Synth: sustained sine 300Hz (looping, variable duty cycle) + filtered white noise pulse (2 hz pulse rate, 200ms window). Feeling: ongoing threat, dimensional rift. Aging: +5mut → add layer. +15mut → wet organic hum. | [looping] | 0.45 | 0 | 1 | Ambient | Continuous sound during wave spawning. Not critical MVP. |

### PROGRESSION & FEEDBACK (6 SFX)

| ID | Event Name | Category | Priority | Sound Description | Duration | Volume | Pitch Var | Max Inst | Mix Group | Notes |
|----|------------|----------|----------|-------------------|----------|--------|-----------|----------|-----------|-------|
| G001 | XP Gem Pickup | Progression | High | Sparkly chime + pop. Synth: sine 1200Hz + 1600Hz (2 note chord, 80ms attack) + decaying shimmer (white noise, high-pass filtered, 100ms). Feeling: collection, reward. Aging: +5mut → add wetness. +15mut → organic slurp. | 140–200 | 0.65 | ±1 | 3 | UI | Triggered per XP gem collection. Stack OK (multiple pickups). |
| G002 | Game Over / Run End | Progression | Critical | Descending sad chord + reverb tail. Synth: minor chord (A3 220Hz + C4 262Hz + E4 330Hz) swept down 2 octaves over 400ms, long reverb (1200ms). Feeling: failure, run end. Aging: +5mut → add distortion. +15mut → wet organic wail. | 1600–2000 | 0.85 | 0 | 1 | SFX | Triggered by player death (end of run). Distinct from player death SFX (slower, sadder). Clip: `gameover_[clean/mid/organic]` |
| G003 | Milestone Achievement | Progression | Medium | Ascending fanfare + sustained note. Synth: 3-note sequence (C5 523Hz → E5 659Hz → G5 784Hz, each 150ms) + sustained G5 with harmonics, reverb tail 400ms. Feeling: major accomplishment. Aging: constant (milestones are "clinical" moments). | 700–900 | 0.75 | 0 | 1 | UI | Reserved for meta-progression unlocks (future). Not critical MVP. |
| G004 | Pause / Menu Open | Progression | High | Soft click + gentle tone. Synth: sine 400Hz pulse (50ms) + filtered noise (low-pass, 100ms). Feeling: interface, non-intrusive. Aging: constant. | 100–150 | 0.50 | 0 | 1 | UI | Triggered by GameEvents.GamePaused. Non-jarring. |
| G005 | Pause / Menu Close | Progression | High | Soft rising chirp + click. Synth: sine sweep 400→600Hz (100ms) + click (sine 800Hz, 30ms). Feeling: resumption, ready. Aging: constant. | 100–160 | 0.50 | 0 | 1 | UI | Triggered by GameEvents.GameResumed. Pair with pause open. |
| G006 | Boss Intro / Phase Change | Progression | High | Dramatic descending siren + low rumble. Synth: sine sweep 1200→300Hz (200ms) + sub rumble (80Hz sine, 300ms). Feeling: danger, climactic moment. Aging: constant (boss moments are design moments, always "clinical"). | 500–700 | 0.80 | 0 | 1 | SFX | Triggered at boss spawn or phase transition. Pair with music shift + VFX effect. |

### UI INTERACTIONS (7 SFX)

| ID | Event Name | Category | Priority | Sound Description | Duration | Volume | Pitch Var | Max Inst | Mix Group | Notes |
|----|------------|----------|----------|-------------------|----------|--------|-----------|----------|-----------|-------|
| U001 | UI Button Hover | UI | Medium | Light click + tone. Synth: sine 700Hz pulse (40ms) + decay (100ms). Feeling: interactive, responsive. Aging: constant. | 100–140 | 0.45 | 0 | 2 | UI | Triggered on button focus. Non-intrusive. |
| U002 | UI Button Click / Confirm | UI | High | Satisfying click + pop. Synth: sine pulse 900Hz (50ms) + filtered noise pop (1500Hz, 80ms). Feeling: action, confirmation. Aging: constant. | 120–180 | 0.55 | 0 | 2 | UI | Triggered on button press (mutation selection, etc.). |
| U003 | UI Negative / Cancel | UI | Medium | Low beep + brief tone. Synth: sine 300Hz pulse (80ms) + decay. Feeling: rejection, error. Aging: constant. | 120–160 | 0.45 | 0 | 1 | UI | Triggered on invalid action or cancel. |
| U004 | UI Selection Change | UI | Low | Gentle slide + click. Synth: sine sweep 600→700Hz (100ms) + click (sine 800Hz, 30ms). Feeling: navigation. Aging: constant. | 100–150 | 0.40 | 0 | 1 | UI | Reserved for menu navigation (future). |
| U005 | Mutation Card Reveal | UI | High | Reveal shimmer + sustained tone. Synth: upward sweep 400→1000Hz (150ms) + shimmer (white noise, high-pass, 200ms) + sustain (sine 600Hz, 200ms). Feeling: discovery, potential. Aging: constant (card reveal is always "clean" — new information). | 400–550 | 0.65 | 0 | 1 | UI | Triggered when mutation selection screen shows 3 cards. Anticipatory sound. |
| U006 | Notification / Alert | UI | Medium | Ascending double-beep + tone. Synth: sine 800Hz (60ms) + sine 1000Hz (60ms) + held tone (sine 900Hz, 200ms). Feeling: attention-grabbing, informative. Aging: constant. | 300–400 | 0.60 | 0 | 1 | UI | Reserved for future system notifications. |
| U007 | Success / Affirmation | UI | Low | Cheerful chime + decay. Synth: major third chord (C5 523Hz + E5 659Hz, 100ms each) + decaying shimmer (1200 Hz tone, 300ms). Feeling: success, positive feedback. Aging: constant. | 300–450 | 0.60 | 0 | 1 | UI | Reserved for additional confirmation sounds. |

### AMBIENCE (1 Looping Base Layer)

| ID | Event Name | Category | Priority | Sound Description | Duration | Volume | Pitch Var | Max Inst | Mix Group | Notes |
|----|------------|----------|----------|-------------------|----------|--------|-----------|----------|-----------|-------|
| A001 | Arena Ambience Loop | Ambient | High | Low hum + periodic pulses. Synth: sustained sub sine 80Hz (looping) + filtered pink noise (300–500Hz, 2 sec cycle pulse). Feeling: digital arena, presence, low-level tension. Aging: +5mut → add layer harmonic. +15mut → organic wet hum + breath texture. | [looping] | 0.30 | 0 | 1 | Ambient | Continuous background during gameplay. Fades in at game start, out at pause/death. Clip: `ambience_arena_[clean/mid/organic]` |

---

## 3. Variation Strategy

Each sound effect with **Pitch Var > 0** needs multiple variations to avoid repetition. For sounds with **Pitch Var = 0**, variations are primarily audio-aging variants.

| SFX ID | Event Name | Variants | Pitch Range (Semitones) | Round-Robin? | Notes |
|--------|------------|----------|------------------------|--------------|-------|
| P001 | Player Hit (Melee) | 2 (clean, mid, organic) | ±2 st | Yes | Same recorded hit, aged variants. Randomize pitch within ±2st per hit. |
| P002 | Player Dash | 2 (clean, organic) | ±1 st | Yes | Whoosh + pop structure constant; pitch varies on sweep. |
| C001 | Projectile Fire | 3 (clean, mid, organic) | ±1 st | Yes | Light pop sound; plenty of variation tolerated. High instance count justifies. |
| C002 | Projectile Impact (Weak) | 2 (clean, organic) | ±2 st | Yes | Quick fizzy sound; variation per impact to reduce repetition. |
| C003 | Projectile Impact (Strong) | 2 (clean, organic) | 0 st | No | Thud is satisfying; sampled variation or slight EQ filtering. |
| C005 | Enemy Melee Attack | 2 (clean, organic) | ±1 st | Yes | Swish motion; pitch variation feels natural. |
| C006 | Enemy Ranged Attack | 2 (clean, organic) | ±1 st | Yes | Energy pop; variations per enemy type if future-proofed. |
| C009 | Explosion Pop | 3 (clean, mid, organic) | ±1 st | Yes | Burst sound; high instances (8 concurrent) demand variation. |
| E001 | Enemy Spawn | 3 (clean, mid, organic) | ±1 st | Yes | Rising sweep; pitch varies across spawning wave. |
| E002 | Enemy Death | 3 (clean, mid, organic) | ±2 st | Yes | Decaying pitch-fall; randomized per death for uniqueness. Max 6 concurrent OK. |
| G001 | XP Gem Pickup | 2 (clean, organic) | ±1 st | Yes | Chime pop; pitch varies per gem (ascending pitch trend possible). |
| A001 | Arena Ambience | 3 (clean, mid, organic) | 0 st | No | Looping base layer; no per-instance variation (single instance). |

**Variation Philosophy:**
- **Clinical Phase (clean variant)**: Synthesized, pure sine/noise, minimal reverb, sharp envelope.
- **Mid-Life (mid variant)**: Slight filtering, added harmonics, brief reverb tail, gentler envelope.
- **Biological Phase (organic variant)**: Wet reverb/distortion, harmonically complex, texture overlays, longer decay.

---

## 4. Audio Aging Rules

As the player accumulates mutations during a run, the audio palette gradually shifts from clean clinical synthesis toward organic biological disruption. This reinforces the game's core fantasy: *"You are evolving into something alien."*

### Mutation Count Thresholds

| Mutation Count | Audio Phase | Master Changes | SFX Character | Mix Adjustments |
|---|---|---|---|---|
| **0–4 mut** | **Clinical (Pure)** | Baseline. No aging applied. | Pure synthesis: sharp sine waves, minimal reverb, dry envelope. Clinical precision. | Master EQ: flat. No compression. |
| **5–9 mut** | **Early Decay** | Add subtle texture to SFX. Reverb appears. | Harmonically enriched: add 2nd/3rd harmonics to synths. Slight reverb tail (0.2–0.4 sec). Envelope stays crisp but adds release tail. | Master EQ: gentle +2 dB around 2k Hz (brightness retention). Soft limiter on Master (threshold –6 dB, ratio 4:1). |
| **10–14 mut** | **Mid-Mutation** | Reverb deepens. Distortion enters. | Complex tones: multi-harmonic synths, noisy texture overlays. Reverb 0.4–0.8 sec. Envelope releases slower. | Master EQ: +3 dB around 150–300 Hz (warmth), –2 dB around 5k Hz (edge softening). Compression ratio 3:1 (more noticeable). |
| **15+ mut** | **Biological (Disrupted)** | Heavy distortion. Organic character dominates. | Harmonically chaotic: rich distortion, biological textures (squelch, wet, fleshy), long reverb (1.0+ sec). Envelopes soft and flowing. | Master EQ: +4 dB @ 150 Hz (bass resonance), –3 dB @ 4k Hz (harshness removal). Compression ratio 5:1. Add subtle chorus (0.02 sec mod, 0.5 Hz rate) for "morphing" feel. |

### Per-SFX Aging Directives

The **Notes** column in the SFX Event Table includes aging directives like:
- `+5mut → add reverb tail`
- `+15mut → organic wet distortion + biological texture`

**Implementation approach:**
- Store 3 audio clips per sound ID: `[eventname]_clean`, `[eventname]_mid`, `[eventname]_organic`
- At runtime, audio mixer snapshot routes based on `PlayerMutationCount`:
  - 0–4: Use `_clean` clips; no reverb group active
  - 5–9: Blend `_clean` + `_mid` clips (crossfade or layer); apply light reverb snapshot
  - 10–14: Use `_mid` or blend `_mid` + `_organic`; apply medium reverb + EQ snapshot
  - 15+: Use `_organic` clips; apply heavy reverb + distortion snapshot

**Edge Case:** Sounds marked "constant (no aging)" (e.g., Level-Up Jingle, Synergy Activation, Boss Intro) are ALWAYS played from `_clean` variant regardless of mutation count. These are **design moments** where clinical precision is the intent.

---

## 5. Asset Naming Convention

All SFX assets follow this naming scheme for Unity project organization.

### Master Naming Pattern

```
[category]_[event]_[variant].wav
```

### Examples

| Clip Name | Event ID | Category | Description |
|-----------|----------|----------|-------------|
| `player_hit_melee_clean.wav` | P001 | Player | Melee hit, clinical variant |
| `player_hit_melee_mid.wav` | P001 | Player | Melee hit, mid-evolution variant |
| `player_hit_melee_organic.wav` | P001 | Player | Melee hit, biological variant |
| `projectile_fire_clean.wav` | C001 | Combat | Projectile launch, clean |
| `projectile_fire_organic.wav` | C001 | Combat | Projectile launch, organic |
| `enemy_death_clean.wav` | E002 | Enemy | Enemy death, clinical |
| `enemy_death_mid.wav` | E002 | Enemy | Enemy death, mid |
| `enemy_death_organic.wav` | E002 | Enemy | Enemy death, biological |
| `player_levelup_jingle.wav` | P004 | Progression | Level-up sound (no aging) |
| `synergy_activation.wav` | C010 | Combat | Synergy jingle (no aging) |
| `mutation_slot_applied.wav` | P007 | Progression | Slot mutation SFX (no aging) |
| `ambience_arena_clean.wav` | A001 | Ambient | Arena hum, clinical |
| `ambience_arena_organic.wav` | A001 | Ambient | Arena hum, biological |

### Directory Structure in `assets/audio/`

```
assets/audio/
├── sfx/
│   ├── player/
│   │   ├── hit_melee_clean.wav
│   │   ├── hit_melee_mid.wav
│   │   ├── hit_melee_organic.wav
│   │   ├── dash_clean.wav
│   │   ├── dash_organic.wav
│   │   ├── death.wav
│   │   ├── levelup_jingle.wav
│   │   └── damage_light.wav, damage_heavy.wav
│   ├── combat/
│   │   ├── projectile_fire_clean.wav
│   │   ├── projectile_fire_organic.wav
│   │   ├── projectile_impact_weak_clean.wav
│   │   ├── projectile_impact_strong_clean.wav
│   │   ├── explosion_pop_clean.wav, ...organic.wav
│   │   └── synergy_activation.wav
│   ├── enemy/
│   │   ├── spawn_clean.wav, ...organic.wav
│   │   ├── death_normal_clean.wav, ...organic.wav
│   │   ├── death_elite_clean.wav, ...organic.wav
│   │   └── melee_swipe_clean.wav, ...organic.wav
│   ├── progression/
│   │   ├── mutation_slot_applied.wav
│   │   ├── mutation_passive_applied.wav
│   │   ├── xp_gem_pickup_clean.wav, ...organic.wav
│   │   ├── wave_clear.wav
│   │   ├── wave_start.wav
│   │   └── gameover.wav
│   ├── ui/
│   │   ├── button_hover.wav
│   │   ├── button_click.wav
│   │   ├── cancel.wav
│   │   ├── mutation_card_reveal.wav
│   │   └── notification.wav
│   └── ambient/
│       ├── arena_hum_clean.wav
│       └── arena_hum_organic.wav
├── music/
│   ├── menu.ogg
│   ├── arena_base.ogg
│   ├── arena_intense.ogg
│   └── boss_theme.ogg
└── config/
    └── mixer_snapshot_config.json
```

---

## 6. Implementation Notes for Unity

### 6.1 AudioMixer Setup (Hierarchical Bus Structure)

1. **Create AudioMixer asset** at `assets/audio/config/SynthbornMixer.mixer`

2. **Create mixer groups** (in order):
   ```
   Master (output)
   ├── Music (send: none, level: -6 dB)
   ├── SFX (send: none, level: -3 dB)
   │   └── Ambient (parent: SFX, level: -12 dB)
   └── UI (send: none, level: -8 dB)
   ```

3. **Add DSP effects**:
   - **Master**: Limiter (threshold: –6 dB, release: 100 ms)
   - **SFX**: (optional) Compressor (only at snapshot "SFX_HeavyAction")
   - **Ambient**: Lowpass filter (cutoff: 5 kHz, resonance: 1.0)

4. **Create AudioMixer Snapshots**:
   - **Default**: Music: –6 dB, SFX: –3 dB, UI: –8 dB, Ambient: –12 dB
   - **Music_Intense**: Music: –14 dB (ducking during combat)
   - **SFX_HeavyAction**: SFX: –5 dB, Compressor ratio increased to 4:1
   - **UI_Focused**: Music: –12 dB, Ambient: –18 dB (mutation selection screen)
   - **Ambient_Decay**: Ambient: fades from –12 dB → –30 dB (over 2 sec)
   - **Aging_5Mut**: Master: Soft limiter engaged; EQ +2 dB @ 2 kHz
   - **Aging_10Mut**: Master: EQ +3 dB @ 250 Hz, –2 dB @ 5 kHz; Compression ratio 3:1
   - **Aging_15Mut**: Master: EQ +4 dB @ 150 Hz, –3 dB @ 4 kHz; Compression 5:1; Chorus added

### 6.2 Wiring Events to GameEvents.cs

**File**: `src/core/AudioManager.cs` (new)

```csharp
using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioClip[] playerHitMeleeClips; // [clean, mid, organic]
    [SerializeField] private AudioClip projectileFireClips_clean;
    // ... (all SFX clips, organized by category)

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource uiSource;
    private AudioSource ambientSource;

    private int currentMutationCount = 0;

    void Start()
    {
        // Subscribe to GameEvents
        GameEvents.OnPlayerDamaged += PlayPlayerDamageSFX;
        GameEvents.OnEnemyDied += PlayEnemyDeathSFX;
        GameEvents.OnLevelUp += PlayLevelUpJingle;
        GameEvents.OnMutationSelected += PlayMutationSFX;
        GameEvents.OnWaveStarted += PlayWaveStartSFX;
        GameEvents.OnWaveCleared += PlayWaveClearSFX;
        GameEvents.OnPlayerDied += PlayGameOverSFX;
        // ... other event subscriptions
    }

    public void PlayPlayerDamageSFX(int damageAmount)
    {
        // Choose between Light (P005) or Heavy (P006) based on damageAmount
        if (damageAmount < 10)
            PlaySFX(GetAgingVariant("player_damage_light"), sfxSource, 0.6f);
        else
            PlaySFX(GetAgingVariant("player_damage_heavy"), sfxSource, 0.75f);
    }

    public void PlayEnemyDeathSFX(Vector2 position, int xpValue)
    {
        PlaySFX(GetAgingVariant("enemy_death_normal"), sfxSource, 0.7f, pitchVar: 2f);
    }

    public void PlayLevelUpJingle()
    {
        // P004: Always "clean" variant (design moment)
        PlaySFX(Resources.Load<AudioClip>("audio/sfx/progression/player_levelup_jingle"), uiSource, 0.8f);
    }

    public void PlayMutationSFX(MutationData mutationData)
    {
        // Increment mutation counter for audio aging
        currentMutationCount++;
        UpdateAudioAging();

        if (mutationData.category == MutationCategory.Slot)
            PlaySFX(Resources.Load<AudioClip>("audio/sfx/progression/mutation_slot_applied"), uiSource, 0.7f);
        else
            PlaySFX(Resources.Load<AudioClip>("audio/sfx/progression/mutation_passive_applied"), uiSource, 0.55f);
    }

    /// <summary>
    /// Select audio aging variant based on currentMutationCount
    /// </summary>
    private AudioClip GetAgingVariant(string baseName)
    {
        string suffix = currentMutationCount switch
        {
            >= 15 => "_organic",
            >= 10 => "_mid",
            >= 5 => "_mid",
            _ => "_clean"
        };

        return Resources.Load<AudioClip>($"audio/sfx/{baseName}{suffix}");
    }

    /// <summary>
    /// Update AudioMixer snapshots based on mutation count
    /// </summary>
    private void UpdateAudioAging()
    {
        AudioMixerSnapshot snapshot = currentMutationCount switch
        {
            >= 15 => audioMixer.FindSnapshot("Aging_15Mut"),
            >= 10 => audioMixer.FindSnapshot("Aging_10Mut"),
            >= 5 => audioMixer.FindSnapshot("Aging_5Mut"),
            _ => audioMixer.FindSnapshot("Default")
        };

        snapshot.TransitionToAtTime(0.5f, AudioSettings.dspTime);
    }

    private void PlaySFX(AudioClip clip, AudioSource source, float volume, float pitchVar = 0f)
    {
        if (clip == null) return;

        source.clip = clip;
        source.volume = volume;
        if (pitchVar > 0)
            source.pitch = 1f + UnityEngine.Random.Range(-pitchVar, pitchVar) / 12f; // semitones to pitch
        else
            source.pitch = 1f;

        source.Play();
    }

    void OnDestroy()
    {
        // Unsubscribe
        GameEvents.OnPlayerDamaged -= PlayPlayerDamageSFX;
        GameEvents.OnEnemyDied -= PlayEnemyDeathSFX;
        // ... etc.
    }
}
```

### 6.3 Ducking Behavior (Music during Combat)

Add this to AudioManager to trigger music ducking when combat is heavy:

```csharp
private float combatIntensity = 0f;

public void UpdateCombatIntensity(int activeSFXCount)
{
    // If more than 5 SFX playing, duck music + activate compression
    if (activeSFXCount > 5)
    {
        audioMixer.FindSnapshot("SFX_HeavyAction").TransitionToAtTime(0.2f, AudioSettings.dspTime);
    }
    else
    {
        audioMixer.FindSnapshot("Default").TransitionToAtTime(0.5f, AudioSettings.dspTime);
    }
}
```

Connect this to the Auto-Attack or WaveSpawner system to track active SFX count.

### 6.4 Audio Timing and Synchronization

**Key Constraints:**
- **Level-Up Jingle (P004)**: Play BEFORE mutation selection screen UI fade-in (~100ms before UI appears)
- **Mutation Applied (P007/P008)**: Play IN SYNC with VFX morph animation (0ms offset)
- **Enemy Death + Explosion Pop**: C009 should trigger simultaneously with VFX particle burst
- **Synergy Activation (C010)**: Play at same time as slow-mo snapshot transition + VFX flash

**Implementation:**
```csharp
// Example: Synchronize mutation SFX with VFX
public void OnMutationApplied(MutationData data)
{
    if (data.category == MutationCategory.Slot)
    {
        // Play SFX + VFX at same time
        float syncTime = AudioSettings.dspTime;
        PlaySFXAtTime("mutation_slot_applied", syncTime);
        VFXManager.Instance.PlayMorphEffectAtTime(data.visual_prefab, syncTime);
    }
}
```

### 6.5 Concurrency & Pooling

**Use Unity's AudioSource pool** to manage SFX playback without frame stutters:

```csharp
public class AudioSourcePool : MonoBehaviour
{
    private Queue<AudioSource> availableSources = new Queue<AudioSource>();
    private HashSet<AudioSource> activeSources = new HashSet<AudioSource>();

    void PlaySFXFromPool(AudioClip clip, AudioMixerGroup mixGroup, float volume)
    {
        AudioSource source = availableSources.Count > 0
            ? availableSources.Dequeue()
            : CreateNewSource();

        source.clip = clip;
        source.outputAudioMixerGroup = mixGroup;
        source.volume = volume;
        source.Play();

        activeSources.Add(source);
        StartCoroutine(ReturnToPoolWhenDone(source, clip.length));
    }

    IEnumerator ReturnToPoolWhenDone(AudioSource source, float clipLength)
    {
        yield return new WaitForSeconds(clipLength + 0.1f);
        activeSources.Remove(source);
        availableSources.Enqueue(source);
    }
}
```

### 6.6 Volume Calibration Table

Reference volumes for AudioManager configuration:

| Bus | Default Level | Min | Max | Notes |
|-----|---|---|---|---|
| Master | 0 dB | –80 dB | 0 dB | Main output, always monitored |
| Music | –6 dB | –20 dB | 0 dB | Ducks to –14 dB during heavy action |
| SFX | –3 dB | –20 dB | 0 dB | Primary payload; no ducking |
| UI | –8 dB | –15 dB | 0 dB | Always audible, softer than SFX |
| Ambient | –12 dB | –30 dB | –6 dB | Background layer; fades in/out |

**Test Procedure:**
- Play game with all buses at reference levels
- Listen for balance: SFX should dominate, but music shouldn't disappear
- Adjust individually if tonal balance feels off (e.g., if SFX muddy the mix, lower Ambient bass or reduce Master bass EQ)

---

## 7. MVP Priority Tiers

Organize implementation in phases based on criticality. **All Critical tier SFX must ship for MVP launch.**

### Tier 1: Critical (MVP Launch Blocker)

These sounds are **non-negotiable** for the core game loop:

| Priority | SFX ID | Event Name | Reason |
|----------|--------|-----------|--------|
| **CRITICAL** | P001 | Player Hit (Melee) | Core feedback loop; without it, combat feels dead |
| **CRITICAL** | P004 | Player Level-Up Jingle | Celebration moment; essential for player motivation |
| **CRITICAL** | P007 | Mutation Slot Applied | Core fantasy: visual evolution + audio transformation |
| **CRITICAL** | C001 | Projectile Fire | Gunfire feedback; needed if projectiles ship in MVP |
| **CRITICAL** | C009 | Explosion Pop | Enemy death feedback; satisfying core loop |
| **CRITICAL** | E002 | Enemy Death (Normal) | Positive reinforcement for kills |
| **CRITICAL** | P003 | Player Death | Run end; distinct finality sound |
| **CRITICAL** | G002 | Game Over / Run End | Post-run conclusion; emotional anchor |

**Estimate**: 8 sounds, 3–4 distinct variants each = ~24–32 audio clips to produce.

### Tier 2: High (Target MVP, ship if time permits)

These greatly enhance the experience but game ships without them if needed:

| Priority | SFX ID | Event Name | Reason |
|----------|--------|-----------|--------|
| **HIGH** | P002 | Player Dash | Mobility feedback; if dash mechanic in MVP |
| **HIGH** | C003 | Projectile Impact (Strong/Crit) | Satisfying crit feedback |
| **HIGH** | C010 | Synergy Activation | Design moment; pivotal to game fantasy |
| **HIGH** | E001 | Enemy Spawn | Wave arrival cue; readability |
| **HIGH** | E003 | Elite Enemy Spawn | Boss intro; danger signal |
| **HIGH** | E005 | Wave Start / Alarm | Urgency cue; gameplay clarity |
| **HIGH** | E006 | Wave Clear / Victory | Positive reinforcement |
| **HIGH** | G001 | XP Gem Pickup | Reward feedback |
| **HIGH** | U002 | UI Button Click | Menu navigation |
| **HIGH** | A001 | Arena Ambience Loop | Background presence |

**Estimate**: 10 sounds, 2–3 variants each = ~20–30 clips.

### Tier 3: Medium (Polish, Vertical Slice+)

Nice-to-have, ship in Vertical Slice or later:

| Priority | SFX ID | Event Name | Reason |
|----------|--------|-----------|--------|
| **MEDIUM** | P005 | Player Damage Taken (Light) | UI feedback, low criticality |
| **MEDIUM** | P006 | Player Damage Taken (Heavy) | Hazard warning |
| **MEDIUM** | P008 | Mutation Passive Applied | Passive pickup feedback |
| **MEDIUM** | C002 | Projectile Impact (Weak) | Hit feedback variation |
| **MEDIUM** | C004 | Explosive Impact | If AoE explosives added |
| **MEDIUM** | C005 | Enemy Melee Attack (Swipe) | Enemy action feedback |
| **MEDIUM** | C006 | Enemy Ranged Attack (Shoot) | Enemy action variety |
| **MEDIUM** | E004 | Elite / Boss Death | Boss victory moment |
| **MEDIUM** | G004 | Pause / Menu Open | Interface feedback |
| **MEDIUM** | G005 | Pause / Menu Close | Resume cue |
| **MEDIUM** | U001 | UI Button Hover | Menu subtlety |
| **MEDIUM** | U005 | Mutation Card Reveal | Selection anticipation |

**Estimate**: 12 sounds, 1–2 variants each = ~12–24 clips.

### Tier 4: Low (Future Expansion)

Future-proofed but not needed for MVP:

| Priority | SFX ID | Event Name | Reason |
|----------|--------|-----------|--------|
| **LOW** | C007 | Crit Hit Indicator | Crit system if added later |
| **LOW** | C008 | Hit Absorption / Shield | Armor system if added |
| **LOW** | E007 | Enemy Alert / Aggro | AI alerting if added |
| **LOW** | E008 | Spawner / Portal Active | If environmental spawners added |
| **LOW** | G003 | Milestone Achievement | Meta-progression if added |
| **LOW** | G006 | Boss Intro / Phase Change | Multi-phase boss if added |
| **LOW** | U003 | UI Negative / Cancel | Cancel button feedback |
| **LOW** | U004 | UI Selection Change | Menu navigation detail |
| **LOW** | U006 | Notification / Alert | System notifications if added |
| **LOW** | U007 | Success / Affirmation | Extra confirmation if added |

**Total MVP Estimate (Critical + High tiers)**: ~18 sounds × 2–3 variants = **~45–54 audio clips to produce**.

---

## 8. Mixing Checklist (Before Submission)

Before considering SFX complete, verify:

- [ ] **Levels balanced**: SFX audible above music and ambience
- [ ] **EQ consistency**: All "clinical" variants are bright/crisp; all "organic" variants are warm/wet
- [ ] **Reverb coherence**: All reverb-heavy sounds use same impulse response (convolver IR) for tonal unity
- [ ] **Ducking works**: Music drops cleanly when SFX dense (no pumping artifacts)
- [ ] **Distortion clean**: Organic variants don't clip or sound harsh (–3 dB headroom minimum)
- [ ] **Stereo width**: UI sounds are mono; SFX can be stereo; ambience is full stereo
- [ ] **Aging smooth**: No jarring transition between "clean" → "mid" → "organic" snapshots
- [ ] **Crossfade silent**: At mutation count 5, 10, 15 transitions are <100ms, inaudible
- [ ] **Looping seamless**: Ambience loop has zero clicks at loop point (fade tail to fade head)
- [ ] **Concurrency tested**: 8 simultaneous SFX plays without artifacts or stuttering
- [ ] **Loudness normalized**: Peak metering on Master shows –6 dB to –3 dB (broadcast standard: –23 LUFS)

---

## 9. Open Questions for Audio Director / Developer

| Question | Owner | Deadline | Notes |
|----------|-------|----------|-------|
| Should SFX have individual volume sliders in Settings, or single "SFX Master" control? | Audio Director + Accessibility | MVP | Recommend: single master + EQ preset selection (Normal, Quiet, Bass-Light) |
| Should synergy SFX be unique per synergy, or is single dramatic sound OK? | Game Designer | MVP prototype | MVP: one generic "Synergy Activation" sound; future: per-synergy custom jingles |
| Is mono OK for combat SFX, or require stereo? | Audio Director | Asset production | Recommend: mono for SFX (clearer, smaller file size); stereo for music & ambience |
| What reverb IR should we use? (Recording, algorithmic, pre-built?) | Audio Director | Asset production | Recommend: one convolver IR (e.g., small hall ~1.0 sec decay) for all reverb-heavy sounds (unified tone) |
| Should player-damage SFX duck other SFX for clarity? | Audio Director | MVP prototype | Recommend: no ducking; hazard warnings should be sharp, audible, never masked |
| Audio format: WAV vs OGG for streaming? | Technical Director | Asset pipeline | Recommend: WAV for SFX (short, loaded on-demand); OGG for music (streaming, compressed) |

---

## 10. References & Inspiration

**Clinical Synthesis Inspirations:**
- *Tron* (1982) — minimalist digital sounds, pure sine waves, sharp attack
- *Synthwave / Darksynth* artists: Perturbator, Carpenter Brut, Gost (sparse, menacing synth palettes)

**Biological Disruption Inspirations:**
- *The Thing* (1982, John Carpenter score) — organic textures, distorted harmonies
- *Akira* OST — mutation/transformation moments (orchestral meets electronic)
- *Videodrome* sound design — unsettling organic-electronic hybrids

**Game Sound References:**
- *Vampire Survivors* — minimal SFX, high-frequency clarity, high stacking tolerance
- *Enter the Gungeon* — satisfying hit feedback, varied projectile sounds, crunchy impact
- *Risk of Rain 2* — rich ambient layers, aging/mutation metaphor (progression echoing in sound)

---

## Appendix: Waveform & Synthesis Guidelines

### Waveform Choices

| Waveform | Use Cases | Character |
|----------|-----------|-----------|
| **Sine** | Bass, pure tones, resonance | Clean, pure, minimal harmonic content |
| **Square** | Retro synth, synth leads | Bright, nasal, 2nd/4th harmonics prominent |
| **Triangle** | Soft synth, bass + treble mix | Mellower square, less harsh |
| **Sawtooth** | Rich synthesis, analog textures | Very bright, full harmonic series |
| **White Noise** | Impact, texture, fizz | Random, full spectrum |
| **Pink Noise** | Ambient, texture, rumble | Deeper than white, 1/f spectrum |
| **Brown Noise** | Sub bass, rumble, explosion | Very deep, 1/f² spectrum |

### Envelope Recommendations

| SFX Type | Attack | Decay | Sustain | Release | Rationale |
|----------|--------|-------|---------|---------|-----------|
| Hit/Impact | 5–30 ms | 50–150 ms | 0% | 50 ms | Quick punch, immediate tail |
| Whoosh/Swish | 20–80 ms | 100–200 ms | 0% | 100 ms | Gradual fade, motion feel |
| Synth Pad / Ambient | 100–300 ms | 200 ms | 50% | 300 ms | Lush, warm entry |
| Jingle / Uplifting | 10–30 ms | 100 ms | 50% | 200 ms | Bright attack, sustained sustain |
| Death / Sad Chord | 50–100 ms | 200 ms | 0% | 1000+ ms | Extended decay, finality |

### Reverb IR Suggestions (For Aging Phase)

| Phase | Reverb Type | Decay Time | Notes |
|-------|------------|-----------|-------|
| **Clinical** | None (dry) | — | Pure synthesis, no space |
| **Mid-Mutation** | Small room / plate | 0.3–0.5 sec | Slight air, beginning of space |
| **Biological** | Hall / Cathedral | 0.8–1.5 sec | Rich, wet, organic presence |

**Implementation**: Store 3 convolver IR WAV files (room impulses) and switch between them at mutation thresholds.

---

## Acceptance Criteria (Final Sign-Off)

- [ ] All 35–40 Critical + High priority SFX fully implemented and integrated into GameEvents
- [ ] Audio aging snapshots working: 0–4 mutations = clean, 5–9 = mid, 10–14 = mid, 15+ = organic
- [ ] AudioMixer routed correctly; all buses outputting to Master with proper ducking
- [ ] SFX volume levels calibrated and balanced against music/UI (reference: SFX –3 dB, Music –6 dB, UI –8 dB)
- [ ] All variant clips (clean/mid/organic) produced and named per convention
- [ ] Ambience loop (A001) plays continuously during gameplay and fades cleanly on pause/death
- [ ] At least 3 concurrent SFX can play without artifacts; max instance counts enforced
- [ ] Player feedback loop sounds satisfying: hit SFX + enemy death SFX + XP pickup create positive reinforcement
- [ ] Audio aging is noticeable but not jarring; smooth snapshot transitions < 100 ms
- [ ] All SFX clips tested at 2x speed and 0.5x speed (for time scale effects, if implemented later)
- [ ] Loudness measured and within broadcast spec (–23 LUFS Master)
- [ ] No unintended SFX stacking issues (e.g., 10 enemy spawns at once don't cause audio overload)

---

**Status**: Ready for Asset Production
**Next Steps**: Audio Engineer produces clips (MVP Critical tier first), then Integration Engineer wires to AudioManager
**Timeline Estimate**: 3–4 weeks for Critical + High tier SFX production + integration testing

