# The Atrium — Visual Direction

> **Status**: Designed
> **Created**: 2026-03-28
> **Art Style**: 16x16 pixel art
> **Color Strategy**: Player = warm intruder against cold clinical arena

## Color Palette (8 colors)

| # | Use | Hex | RGB | Notes |
|---|-----|-----|-----|-------|
| 1 | Floor base | #E8ECF0 | (232,236,240) | Cool white, primary background |
| 2 | Floor grout / grid | #B0B8C8 | (176,184,200) | Subtle lab tile lines |
| 3 | Walls | #D0D8E0 | (208,216,224) | Slightly darker than floor |
| 4 | Wall accent (bio-light) | #4A90D9 | (74,144,217) | Only color source in arena — status strip |
| 5 | Pillars | #98A8B8 | (152,168,184) | Engineered, neutral |
| 6 | Pillar bases | #708090 | (112,128,144) | Recessed plate |
| 7 | Player accent | #F5DEB3 | (245,222,179) | Warm wheat — biological anomaly in cold space |
| 8 | Danger/warning | #C0392B | (192,57,43) | Exploder prime, low HP only |

## Color Strategy: Warm Intruder

The Atrium is entirely cold (white, blue-gray, cyan). The player character is the only warm element — slight cream/amber tone. This communicates: **you don't belong here.** Enemies share the cold palette because they are the system asserting itself. The player is the biological anomaly.

## Tile Design

| Tile | Size | Count | Description |
|------|------|-------|-------------|
| Floor base | 16x16 | 1 | Clean white, minimal grout lines (1px gray) |
| Floor center mark | 16x16 | 4 (2x2 arrangement) | Faint circular calibration glyph |
| Wall horizontal | 16x16 | 1 | Off-white with thin blue strip at bottom |
| Wall corner | 16x16 | 2 | Normal corner + chamfered diagonal |
| Pillar | 32x32 (2x2 tiles) | 1 | Gray block with darker base plate |
| Wall panel (decorative) | 16x16 | 1 | Status display (scrolling glyph texture) |

**Total unique tiles: ~8-10**

## Enemy Visual Integration

| Element | Approach |
|---------|----------|
| Normal enemies | Cold blue-gray tones, darker than floor — visible but "belongs" |
| Elite enemies | Same palette + blue glow outline (matches wall bio-light) |
| Boss | Largest entity, same cold palette but darker, imposing silhouette |
| Projectiles (player) | Warm amber/yellow — matches player's warm accent |
| Projectiles (enemy/shooter) | Cold cyan — matches arena's blue accent |
| XP gems | Green (#2ECC71) — natural contrast against cold blue-white |
| HP orbs | Red (#E74C3C) — universal health signal |

## Sprite Asset List (MVP)

| Category | Count | Description |
|----------|-------|-------------|
| Floor tiles | 3 | Base, center mark (2x2), grout variant |
| Wall tiles | 4 | Horizontal, corner, chamfered corner, panel |
| Pillar | 1 | 2x2 tile block |
| Player base | 1 sheet | 4 directions × 4 frames = 16 frames (16x16) |
| Player mutations | 12 sheets | 3 per slot × 4 slots, each 4×4 frames |
| Enemy: Chaser | 1 sheet | 4 dir × 2 frames = 8 frames |
| Enemy: Runner | 1 sheet | 4 dir × 2 frames (faster anim) |
| Enemy: Shooter | 1 sheet | 4 dir × 2 frames + fire pose |
| Enemy: Exploder | 1 sheet | 4 dir × 2 frames + prime (expanding) |
| Boss | 1 sheet | 4 dir × 2 frames (larger sprite, 32x32) |
| XP Gem | 1 | 3 sizes (small/med/large), 2 frame sparkle |
| HP Orb | 1 | 2 frame pulse |
| Projectile (player) | 1 | Simple, warm-toned |
| Projectile (enemy) | 1 | Simple, cold-toned |
| **Total unique sprites** | **~25-30** | Achievable for solo dev |

## Lighting

- Even diffuse, cool white (#F0F4F8), no shadows (2D flat)
- Subtle edge vignette (center slightly brighter)
- Wall bio-light provides the only directional color accent
- Boss fight: bio-light strip optional pulse (low priority)

## Art Pipeline Notes

- All sprites use the same 8-color palette for The Atrium
- Point filtering (no anti-aliasing) — pixel art rule
- Sprite Atlas: all Atrium tiles + player + enemies in one atlas
- Pixels-per-unit: 16
- Sorting layers: Background (floor) → Entities (player/enemies) → UI
