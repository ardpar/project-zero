# Prototype: Core Loop

> **PROTOTYPE — NOT FOR PRODUCTION**
> **Question:** Is the move → auto-attack → XP → level-up → mutate loop fun?
> **Date:** 2026-03-27
> **Engine:** Unity 6.3 LTS (6000.3)

## Unity Setup Instructions

### 1. Create Unity Project
- Open Unity Hub → New Project → **2D (URP)**
- Name: `SYNTHBORN-Prototype`
- Unity version: 6.3 LTS

### 2. Import Scripts
- Copy the entire `Scripts/` folder into `Assets/Scripts/`

### 3. Create Scene Setup
1. Create an empty scene called `Prototype`
2. Add a **Camera** (already exists as Main Camera)
   - Set to Orthographic, Size = 8
3. Create an empty GameObject called `GameManager`
   - Add `GameManager.cs`
   - Add `WaveSpawner.cs`
   - Add `ObjectPool.cs`
4. Create a **Player** (empty GO):
   - Add `SpriteRenderer` (assign a white square sprite, scale to 0.5)
   - Add `CircleCollider2D` (radius: 0.3, Is Trigger: false)
   - Add `Rigidbody2D` (Gravity Scale: 0, Freeze Rotation Z: true)
   - Add `PlayerController.cs`
   - Add `PlayerHealth.cs`
   - Add `AutoAttack.cs`
   - Tag: "Player"
5. Create **Enemy Prefab** (empty GO):
   - Add `SpriteRenderer` (red square, scale 0.4)
   - Add `CircleCollider2D` (radius: 0.25, Is Trigger: true)
   - Add `Rigidbody2D` (Gravity Scale: 0, Freeze Rotation Z: true)
   - Add `EnemyAI.cs`
   - Add `EnemyHealth.cs`
   - Tag: "Enemy", Layer: "Enemy"
   - Save as Prefab
6. Create **Projectile Prefab** (empty GO):
   - Add `SpriteRenderer` (yellow circle, scale 0.15)
   - Add `CircleCollider2D` (radius: 0.1, Is Trigger: true)
   - Add `Projectile.cs`
   - Save as Prefab
7. Create **XP Gem Prefab** (empty GO):
   - Add `SpriteRenderer` (green diamond, scale 0.2)
   - Add `CircleCollider2D` (radius: 0.15, Is Trigger: true)
   - Add `XPGem.cs`
   - Tag: "XPGem"
   - Save as Prefab
8. Create **Arena Bounds**:
   - 4 empty GameObjects with BoxCollider2D along edges (top/bottom/left/right)
   - Position at ±15 units from center
   - Layer: "Wall"
9. Create **Canvas** (Screen Space - Overlay):
   - Add `PrototypeHUD.cs`
   - Add child UI elements (see HUD script for structure)
10. Create **MutationPanel** (child of Canvas, initially inactive):
    - Add `MutationSelectionUI.cs`
    - 3 Button children for mutation cards

### 4. Wire References
- GameManager: assign Enemy Prefab, Projectile Prefab, XP Gem Prefab
- AutoAttack: assign Projectile Prefab reference (or get from GameManager)
- PrototypeHUD: assign HP bar, XP bar, wave text references

### 5. Play!
- WASD to move, Space to dash
- Attacks fire automatically
- Collect green gems for XP
- Level up → pick a mutation card
- Survive as long as possible
