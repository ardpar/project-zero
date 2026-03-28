# The Atrium — Level Design

> **Status**: Designed
> **Biome**: 1 of 6 | Era 1 — Active Operation
> **Created**: 2026-03-28
> **Play Time**: 10-15 min (6 waves + boss)

## Overview

The Atrium is the Arena's original intake zone — pristine, sterile, fully functional. A 30x30 unit **chamfered square** arena (cut corners prevent death traps). 4 symmetrical pillars at mid-ring provide cover against Shooters. No environmental hazards (this is the Arena working exactly as intended). Player spawns at center.

## Layout

```
     -15  -10   -5    0    +5   +10  +15
+15 --+----+----+OOOO+OOOO+----+----+--
      |   /                      \   |
+10 --+  /                        \  +--
      | /   [P1]            [P2]   \ |
+5  --+/          (-7,+6)  (+7,+6)  \+--
      |                              |
0   --+       [CENTER 0,0]           +--
      |                              |
-5  --+\          (-7,-6)  (+7,-6)  /+--
      | \   [P3]            [P4]   / |
-10 --+  \                        /  +--
      |   \                      /   |
-15 --+----+----+OOOO+OOOO+----+----+--

LEGEND: OOOO=Wall  ////=Chamfered corner  [Pn]=Pillar (2x2)
        [CENTER]=Player spawn + boss entry target
```

## Pillars

| Property | Value |
|----------|-------|
| Count | 4 (symmetrical, one per quadrant) |
| Size | 2x2 units |
| Positions | (±7, ±6) — mid-ring |
| Collision | Solid (player + enemies) |
| Destructible | No (MVP) |

**Purpose:** Cover against Shooters (Wave 3+). Enemies stack against pillars (no pathfinding = natural funnel for AoE). Boss fight: pillars become pinch points (boss pushes you into corners).

## Spatial Zones

| Zone | Radius | Character |
|------|--------|-----------|
| **Center** | ~5u from 0,0 | Max exposure 360°. AoE build territory. Dangerous with Shooters. |
| **Pillar Ring** | ~9u from 0,0 | Partial cover. Primary skillful play zone. |
| **Wall Edge** | <3u from wall | 180° exposure. Retreat, but chamfered corners can funnel. |

## Wave Pacing

| Wave | Feel | Arena Use |
|------|------|-----------|
| 1-2 | Arena feels enormous, learning space | Pillars irrelevant, open exploration |
| 3-4 | Shooters create cover value, Exploders punish camping | Pillar orbiting begins |
| 5-6 | Density makes arena feel half-sized, every position is a trade-off | Claustrophobic, contested lanes |
| Boss | Boss hitbox (~3x3) makes pillars into traps | Constant circular motion, full perimeter |

## Environmental Storytelling

| Element | Position | Communicates |
|---------|----------|-------------|
| Center floor marking | (0,0) | You were placed here deliberately |
| Wall status panels (4x) | One per wall, centered | System is running, logging |
| Bioluminescent blue wall strip | Full perimeter | Arena is operational |
| Symmetrical pillars | Four quadrants | Engineering, not nature |
| No exits, no doors | Everywhere | You are inside. Nowhere to go. |
| Boss entry from top wall | (0, +15) | The Arena does not explain its logistics |

## Visual Direction

- **Floor:** Clean white lab tiles, cool gray grout, uniform (no variation = engineered sterility)
- **Walls:** Off-white, thin bioluminescent blue strip at base (status indicator)
- **Pillars:** Cool gray, darker base plate
- **Center:** Faint calibration marking in muted gray
- **Lighting:** Even, diffuse, cool white (slightly blue-shifted), subtle edge vignette

## Color Palette

| Element | Color | Hex |
|---------|-------|-----|
| Floor | Cool white | #E8ECF0 |
| Grout | Gray | #B0B8C8 |
| Walls | Off-white | #D0D8E0 |
| Blue strip | Bioluminescent | #4A90D9 |
| Pillars | Cool gray | #98A8B8 |
| Pillar base | Dark gray | #708090 |
| Center mark | Muted gray | #8898AA |
| **Player** | **Warm cream/amber** | **(contrast — intruder)** |
| **Enemies** | **Cold palette** | **(belong to the system)** |

## Audio Cues (for audio-director)

| Moment | Trigger | Emotional Target |
|--------|---------|-----------------|
| Run start | Player spawns | Cold machinery at rest |
| Wave 1 | First enemy | Rhythmic pulse begins |
| Wave 3 | Shooters enter | New layer — ranged threat voice |
| Wave 5-6 | 100+ enemies | System accelerating (not breaking) |
| Wave break | Between waves | Machine exhaling (3s) |
| Boss entry | Boss crosses boundary | Escalation apex |
| Boss death | HP = 0 | Clean release, not celebration |
