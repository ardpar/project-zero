# SYNTHBORN — Sound Bible

> **Status**: Approved
> **Created**: 2026-03-28
> **Authority**: audio-director + sound-designer
> **Middleware**: Native Unity Audio (AudioSource + AudioMixer)
> **Music Style**: Dark Synthwave / Ambient Hybrid

---

## Audio Pillars

1. **Arena Nefes Alır** — Ortam canlı hissetmeli, sessizlik bir araç. Ambient bed'ler Arena'nın gözlemlediğini hissettirmeli.
2. **Mutasyon Duyulmalı** — Slot mutasyonları ağır, geri dönüşsüz SFX. Pasifler hafif. Synergy'ler "yerine oturma" hissi.
3. **Bilgi Atmosferden Önce** — Kritik sesler (elite uyarısı, boss girişi, düşük HP) her zaman mix'i keser. Müzik ve ambient duck eder.
4. **Kısıtlı Palet, Maksimum Kontrast** — Varsayılan soğuk/dijital. Kontrast anları (synergy, boss ölümü, zafer) bu yüzden güçlü hisseder.

---

## Music Direction

### Genre: Modular Synthwave / Dark Ambient Hybrid
Retro chiptune değil, orkestral değil. Klinik sentez, pulsing low-frequency foundation, sparse melodic elementler. Referans: Hotline Miami (enerji) × SOMA (atmosfer). Chiptune sadece aksan olarak (arpeggio, high-freq pulse).

### Adaptive Music: Layer-Based Vertical Remixing
Müzik stem'lerden oluşur. Gameplay state'e göre stem eklenir/çıkarılır (2-4 sn crossfade).

**Base (her zaman aktif):**
- Drone/Pad — sub-frequency foundation, biome-specific
- Click/Pulse — metronomic synthetic percussion, BPM anchor

**Intensity layers (progressive):**

| Tetikleyici | Eklenen Layer |
|-------------|---------------|
| Wave 2+ / 31+ düşman | Bass line (ritmik sürüş) |
| Wave 3+ / 81+ düşman | Synth lead melody (sparse, 1-bar loop) |
| Wave 4-5 / 150+ düşman | Distorted percussion, ritim ikiye katlanır |
| Wave 6 / max pressure | Full arrangement + tempo nudge (+2-4 BPM) |

**Special states:**
- **Mutasyon seçimi:** Drone + faint pad only, suspended chord
- **Synergy:** One-shot melodic stinger, music ducked
- **Wave clear:** Bass + percussion drop, 3 sn rest
- **Low HP (<20%):** Distortion-pulse layer, heartbeat sub hit / 2 sn
- **Boss fight:** Full override to boss track
- **Boss death:** 1.5 sn silence → victory stinger

### Per-Biome Music

| Biome | Konsept |
|-------|---------|
| The Atrium | Pristine sine-wave pads, clean BPM grid — kontrollü bir deney |
| Assay Chambers | Atrium stems pitch-shifted -2 semitone, noise artifacts in drone |
| Deep Archive | Pure data-pulse rhythm, no melody — otoriter olmayan bir protokol |
| Collapse Stratum | Atrium stems wrong speed, phase drift — tanıdık ama yerinden |
| Corruption Layer | Organic textures: breathing frequencies, wet low-end, irregular rhythm |
| Null Chamber | Near-silence: single sub-bass + sparse crystalline hits — yokluk sesli |

### Boss Music
Adaptif değil, tam loop (2-3 dk). 8-bar intro → boss'un ilk saldırı penceresinde drop. Biome tonal identity'si paylaşılır ama yeni ritmik element eklenir. Boss ölüm: 1.5 sn silence → victory stinger.

### Menu Music
Ana menü: slow-evolving drone, no BPM, no percussion. Meta ekran: drone + low-volume pulse. 90+ sn loop.

---

## SFX Palette

### Philosophy: Clinical Synthesis → Biological Disruption
Varsayılan sesler temiz ve elektronik (sine/square wave, filtered noise, digital clicks). Mutasyon sayısı arttıkça sesler organik doku kazanır (wetness, distortion, irregular envelopes).

### Core Texture Families

| Family | Kullanım | Karakter |
|--------|---------|----------|
| Clean Synthesis | UI, pasif pickup, XP, hareket | Pure tones, fast attack, minimal reverb |
| Hard Transients | Auto-attack, projectile impact | Punchy, front-loaded, dry |
| Metallic / Crystalline | Slot mutasyon equip, elite spawn | Ring-modulated, resonant decay |
| Organic / Wet | Geç oyun mutasyonları, corruption enemies | Filtered, low-mid heavy, pitch wobble |
| Digital Noise | Synergy, wave clear, boss death | Bit-crush, FM sweep, glitch burst |
| Sub-Bass Impact | Boss slam, major kills | <100Hz, felt as much as heard |

### Audio Aging (Mutasyon Sayısına Göre)

| Mutasyon Sayısı | Ses Karakteri | Teknik |
|-----------------|--------------|--------|
| 0-4 | **Clinical** — pure synthesis, no reverb | Temiz variants |
| 5-9 | **Early Decay** — subtle harmonics, light reverb | ±3% pitch randomization |
| 10-14 | **Mid-Mutation** — complex tones, moderate reverb | Organic texture layer eklenir |
| 15+ | **Biological** — distorted, wet, organic | Full organic variant swap |

Not: Tasarım anları (level-up jingle, synergy) her zaman "clinical" kalır (aging yok).

---

## Mix Philosophy

### Priority Hierarchy

| # | Ses | Davranış |
|---|-----|---------|
| 1 | Player hasar alma | Her zaman duyulur, distinct frequency |
| 2 | Player ölüm | — |
| 3 | Boss girişi / Elite uyarısı | — |
| 4 | Synergy aktivasyonu | En yüksek non-critical ses |
| 5 | Slot mutasyon equip | — |
| 6 | Auto-attack hit confirmation | — |
| 7 | Wave clear / wave start | — |
| 8 | XP collect / level-up | — |
| 9 | Passive pickup | — |
| 10 | Ambient SFX | — |
| 11 | Music | En düşük öncelik |

Priority 1-5 tetiklenince müzik ve ambient -6 to -12 dB duck eder (0.5-1.5 sn).

### Mixing Groups

| Group | Base Level | Ducking |
|-------|-----------|---------|
| Master | 0 dB | — |
| Music | -6 dB | -14 dB during heavy action |
| SFX | -3 dB | No ducking (primary) |
| UI | -8 dB | No ducking (always audible) |
| Ambient | -12 dB | Fades with environment |

### Targets
- Integrated loudness: -14 LUFS
- Peak ceiling: -1 dBTP
- Music masters: -16 LUFS
- SFX peaks: -12 to -6 dBTP

---

## Audio Budget

### SFX

| Kategori | Sayı | Notlar |
|----------|------|--------|
| Player Actions | 8 | Hit, dash, death, level-up, mutations, damage |
| Combat/Projectiles | 10 | Fire, impact, explosions, crits, synergy |
| Enemy Actions | 8 | Spawn, death (normal/elite/boss), alerts |
| Progression | 6 | XP, game over, pause, boss intro |
| UI | 7 | Buttons, cards, notifications |
| Ambience | 1 | Arena hum loop |
| **Toplam** | **~40** | + aging variants = ~55 clip |

### Music

| Tür | Sayı |
|-----|------|
| Biome stems (6 biome × 5 stem) | 30 |
| Boss tracks | 6 |
| Stingers (wave, synergy, level-up) | 5 |
| Menu ambient | 2 |
| Victory / Death | 2 |
| **Toplam** | **~45 dosya** |

### File Format
- Music/Ambient: `.ogg` Vorbis, -16 LUFS, 44.1kHz, stereo, streaming
- SFX: `.ogg` Vorbis, -12 to -6 dBTP, 44.1kHz, mono, preloaded
- Naming: `sfx_[context]_[name]_[variant].ogg` / `mus_[biome]_[name]_[stem].ogg`

---

## Reference Dokümanlarda

- **SFX Detaylı Tablo:** `design/gdd/sfx-specification.md` (40 ses event'i, mixing, variation, implementation)
- **Audio Director Brief:** `design/audio-director-brief.md`

## Implementation Complexity: Moderate
Layer-based adaptive müzik = AudioMixer snapshot blending ile yapılabilir. FMOD gereksiz MVP'de. Vertical Slice'da değerlendirilebilir.
