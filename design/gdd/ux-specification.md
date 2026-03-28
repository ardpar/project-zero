# SYNTHBORN — MVP UI/UX Specification

> **Status**: Designed
> **Created**: 2026-03-28
> **Pillar**: Kolay Giriş/Derin Derinlik (Pillar 4)
> **Technology**: UI Toolkit or UGUI (UI programmer decides)
> **Input**: Keyboard+Mouse AND Gamepad

---

## Screen Flow

```
                    ┌──────────────┐
   App Launch ────► │  MAIN MENU   │◄──────────── Run End (Main Menu)
                    └──────┬───────┘
                           │ Start Run
                           ▼
                    ┌──────────────┐
              ┌────►│ GAMEPLAY HUD │◄────── Mutation Selected
              │     └──┬───────┬───┘        Pause → Resume
              │        │       │
              │   ESC/Start  Level-up
              │        │       │
              │        ▼       ▼
              │  ┌──────────┐ ┌──────────────────┐
              │  │  PAUSE   │ │ MUTATION SELECTION│
              │  │  MENU    │ │    OVERLAY        │
              │  └──┬───────┘ └──────────────────┘
              │     │
              │  Quit to Menu ──► Main Menu
              │
              │  Player Death / Boss Killed
              │        │
              │        ▼
              │  ┌──────────────┐
              └──│ RUN END      │──► Restart (skip menu)
                 │ SCREEN       │
                 └──────────────┘

  SETTINGS: modal overlay, accessible from Main Menu + Pause Menu
```

---

## Screen Wireframes

### Gameplay HUD

```
┌──────────────────────────────────────────────────┐
│ [HP ████░░] 120/150  [ARM ▓▓░] 30   WAVE 3 1:24 │
│                                                   │
│                                          [Mut ⬡]  │
│              <<game world>>              [Mut ⬡]  │
│                                          [Mut ⬡]  │
│                                          [Mut ⬡]  │
│                                          [Syn ✦]  │
│ [◉ DASH]                                          │
│──────────────────────────────────────────────────│
│ [XP ████████████████████░░░░░░░░░░░░░] LVL 7    │
└──────────────────────────────────────────────────┘
Boss fight: top row becomes full-width boss HP bar + boss name
```

### Mutation Selection

```
┌──────────────────────────────────────────────────┐
│░░░░░░░░░  CHOOSE YOUR MUTATION  ░░░░░░░░░░░░░░░░│
│░░  ┌──────────┐ ┌──────────┐ ┌──────────┐  ░░░░│
│░░  │ [ICON]   │ │ [ICON]   │ │ [ICON]   │  ░░░░│
│░░  │ Name     │ │ Name     │ │ Name     │  ░░░░│
│░░  │ [Slot]   │ │ [Slot]   │ │ [Passive]│  ░░░░│
│░░  │ +20% DMG │ │ +5 Armor │ │ +15% XP  │  ░░░░│
│░░  │ Desc...  │ │ Desc...  │ │ Desc...  │  ░░░░│
│░░  │ ✦SYNERGY │ │          │ │          │  ░░░░│
│░░  └──────────┘ └──────────┘ └──────────┘  ░░░░│
│░░░░░░░ [1]          [2]          [3] ░░░░░░░░░░│
└──────────────────────────────────────────────────┘
Rarity borders: Common=gray, Uncommon=green, Rare=blue, Legendary=gold
```

### Pause / Main Menu / Run End / Settings

Minimal vertical menus. Default focus = most common action (Resume, Start Run, Restart).

---

## Input Mapping

| Screen | KB/Mouse | Gamepad | Quick Key |
|--------|----------|---------|-----------|
| **Gameplay** | WASD move, Space dash, ESC pause | Stick, RT dash, Start pause | — |
| **Mutation Select** | Click card / 1-2-3 keys | D-pad L/R, A confirm | 1/2/3 = instant select |
| **Menus** | Arrow/Tab, Enter, ESC back | D-pad, A select, B back | — |
| **Settings** | Arrow slider, Space toggle | D-pad, A toggle | — |

**Rules:** Tab order = top→bottom, left→right. Focus always visible. ESC/B = back. No screen requires mouse.

---

## Transitions

| Transition | Duration | Type |
|-----------|----------|------|
| Menu → Run | 0.4s + 0.4s | Fade to/from black |
| HUD → Pause | 0.15s | Dim + menu scale in |
| HUD → Mutation Select | 0.65s | Dim + cards flip (staggered) |
| Mutation → HUD | 0.3s | Selected card grows, fade out |
| HUD → Run End | 0.7s | HUD fade → black → stats fade in |
| Run End → Restart | 0.6s | Fade black → new run |

All transitions use `Time.unscaledDeltaTime` (immune to pause).

---

## Accessibility

| Requirement | MVP Status |
|-------------|-----------|
| Min text: 12px (1080p) | Included |
| Color contrast: WCAG AA (4.5:1) | Included |
| Colorblind: dual coding (shape+color, never color alone) | Included |
| Rarity: border thickness varies (not just color) | Included |
| Damage numbers: "+" prefix for heals, bounce for crits | Included |
| Screen shake toggle | Included (Settings) |
| Damage number toggle | Included (Settings) |
| KB-only navigable | Included |
| Gamepad-only navigable | Included |
| Control remapping | Post-MVP |
| UI scale slider | Post-MVP |
| Photosensitivity warning | Recommended (low cost) |

---

## Data Bindings (Read-Only)

| HUD Element | Source | Event |
|-------------|--------|-------|
| HP bar | EntityHealth | OnHealthChanged |
| Armor bar | EntityHealth | OnArmorChanged |
| XP bar | XPManager | OnXPChanged |
| Level label | XPManager | OnLevelUp |
| Wave info | WaveSpawner | OnWaveStarted |
| Boss HP | EntityHealth (boss) | OnBossHealthChanged |
| Dash CD | PlayerController | Per-frame read |
| Mutation icons | MutationSystem | OnMutationEquipped |
| Synergy icons | SynergyMatrix | OnSynergyActivated |
| Damage numbers | GameEvents | OnDamageDealt |
| Mutation cards | MutationPool | On UI open (3 cards) |
| Run stats | RunManager | On screen open |

**UI writes nothing to game state.** Card selection fires `MutationSelectionEvent(id)` — Mutation System consumes.
