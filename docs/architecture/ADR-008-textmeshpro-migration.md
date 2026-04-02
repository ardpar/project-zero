# ADR-008: TextMeshPro Migration Strategy

## Status
Accepted

## Date
2026-04-02

## Context

### Problem Statement
Early SYNTHBORN UI code used Unity's built-in `UnityEngine.UI.Text` component. As the project moved into Arena content sprints (Sprint 13+), runtime-generated UI (Signal Archive entries, Stat Point rows, Adaptation Parameter rows) required rich-text support (bold, color tags, size tags), sub-pixel font rendering at small sizes (11–13px), and the ability to create text components entirely in code without a prefab. Unity's legacy `Text` component does not support rich-text reliably at small sizes and renders noticeably worse at the pixel art aesthetic's target resolution. The migration to TextMeshPro was required.

A secondary problem emerged: which TMP type to use in serialised fields. `TextMeshProUGUI` works only inside a Canvas (UI layer), while `TextMeshPro` works in world space. Using the concrete subclass in serialised fields forces each component to be committed to one rendering context at authoring time. For runtime-generated text, the type must be specified at instantiation — but the field that owns the reference should be flexible.

### Constraints
- `Synthborn.UI` and `Synthborn.Lore` assemblies need TMP — both declare `Unity.TextMeshPro` in their `.asmdef` reference lists. Other assemblies (`Synthborn.Core`, `Synthborn.Waves`, etc.) must not reference TMP to keep the package dependency narrow.
- All serialised `[SerializeField]` text fields in MonoBehaviours must accept both UI-canvas and world-space text objects where possible.
- Runtime-generated text components (in `SignalArchiveScreen`, `StatPointUI`) must use the concrete `TextMeshProUGUI` type at instantiation, since these objects are always created inside a Canvas.
- The legacy `TutorialOverlay.LegacyTutorialSequence()` coroutine must continue to function; migrating it to TMP must not break the legacy fallback path.
- `TMP_FontAsset` must be injectable at the component level (via `[SerializeField]`) so individual UI panels can use the project's custom pixel font without hardcoded font references.

### Requirements
- Serialised text field type: `TMP_Text` (abstract base class) — not `TextMeshProUGUI` or `TextMeshPro`.
- Runtime-created text components: use `typeof(TextMeshProUGUI)` via `new GameObject("...", typeof(TextMeshProUGUI))` — always inside a Canvas.
- Assembly wiring: `Unity.TextMeshPro` added to `Synthborn.UI.asmdef` and the Lore assembly's asmdef (or the Lore namespace compiles inside `Synthborn.UI`'s assembly scope).
- `TMP_FontAsset` exposed as a `[SerializeField]` on components that create text at runtime (`StatPointUI`), assigned in the Unity Inspector per panel.
- All font references must be serialised — no `Resources.Load` for font assets (consistent with the project's Addressables-first policy).

## Decision

**Serialised fields use `TMP_Text` (base class).** Every `[SerializeField]` text reference in the codebase uses `TMP_Text` rather than either concrete subclass. This allows the assigned object in the Inspector to be either a `TextMeshProUGUI` (Canvas child) or a `TextMeshPro` (world space) without changing the declaring class. All 12 UI files with TMP usage follow this pattern for their inspector-wired fields.

**Runtime-generated text uses `TextMeshProUGUI` explicitly.** `SignalArchiveScreen.CreateListItem()` and `StatPointUI.CreateStatRow()` construct GameObjects entirely in code and add components via `typeof(TextMeshProUGUI)`. Since these objects are always children of a `RectTransform` inside a Canvas, the concrete UI type is correct and no base-class flexibility is needed. `GetComponent<TextMeshProUGUI>()` is then called immediately to configure `text`, `fontSize`, `color`, `alignment`, and `raycastTarget`.

**`TMP_FontAsset` for runtime-created text.** `StatPointUI` exposes a `[SerializeField] private TMP_FontAsset _font` field. After creating each `TextMeshProUGUI` at runtime, `if (_font != null) text.font = _font` is applied. This allows the project's pixel font to be injected from the Inspector without hardcoding a font path. Components that do not need a custom font (e.g., `WaveBanner`, `LoreDiscoveryPopup`) do not expose this field and rely on TMP's default fallback font.

**Assembly declaration.** `Synthborn.UI.asmdef` declares `Unity.TextMeshPro` as an explicit reference. The Lore scripts (`SignalArchiveScreen`, `LoreDiscoveryPopup`) compile within the `Synthborn.UI` assembly scope (they are located in a subdirectory without their own `.asmdef`), so they inherit the TMP reference from `Synthborn.UI`. No other assembly declares TMP, keeping the dependency contained to the UI layer.

**Legacy fallback preserved.** `TutorialOverlay._hintText` is declared as `TMP_Text`. The `LegacyTutorialSequence()` coroutine sets `_hintText.text = "..."` — this assignment is valid on `TMP_Text` because `text` is a virtual property on the base class. The legacy path required no structural changes; only the field type declaration changed from the pre-migration `Text` to `TMP_Text`.

### Architecture Diagram

```
Assembly boundary: Synthborn.UI (declares Unity.TextMeshPro)
  │
  ├── Serialised fields (Inspector-assigned)
  │     TMP_Text _hintText          (TutorialOverlay, WaveBanner, PrototypeHUD, etc.)
  │     TMP_Text _titleText         (SignalArchiveScreen detail panel)
  │     TMP_Text _bannerText        (WaveBanner)
  │     TMP_FontAsset _font         (StatPointUI — optional pixel font injection)
  │
  ├── Runtime-generated text (always in Canvas)
  │     new GameObject("Text", typeof(TextMeshProUGUI))    ← StatPointUI.CreateStatRow()
  │     new GameObject("Entry", typeof(TextMeshProUGUI))   ← SignalArchiveScreen.CreateListItem()
  │     GetComponent<TextMeshProUGUI>() → configure text/fontSize/color/alignment
  │     if (_font != null) text.font = _font               ← font injection
  │
  └── Legacy fallback
        TutorialOverlay.LegacyTutorialSequence()
          _hintText.text = "..."     ← TMP_Text.text, valid on base class
          No structural change required

Other assemblies (Synthborn.Core, .Waves, .Combat, .Enemies, etc.)
  → Do NOT reference Unity.TextMeshPro
  → No TMP types in public APIs crossing assembly boundaries
```

### Key Interfaces

```csharp
// Pattern 1 — serialised field (all inspector-wired text)
[SerializeField] private TMP_Text _labelText;     // accepts both UI and world-space TMP

// Pattern 2 — serialised arrays (AdaptationPointUI)
[SerializeField] private TMP_Text[] _paramLabels = new TMP_Text[5];

// Pattern 3 — optional font injection (runtime-generated panels)
[SerializeField] private TMP_FontAsset _font;     // null = use TMP default

// Pattern 4 — runtime component creation (always inside Canvas)
var textGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
textGO.transform.SetParent(container, false);
var tmp = textGO.GetComponent<TextMeshProUGUI>();
tmp.text      = "value";
tmp.fontSize  = 13;
tmp.color     = Color.white;
tmp.alignment = TextAlignmentOptions.MidlineLeft;
if (_font != null) tmp.font = _font;
tmp.richText  = true;
tmp.raycastTarget = false;   // performance: disable for non-interactive labels
```

## Alternatives Considered

### Alternative 1: Use TextMeshProUGUI in All Serialised Fields
- **Description**: Declare all serialised text fields as `TextMeshProUGUI` instead of `TMP_Text`.
- **Pros**: No ambiguity — every field is explicitly a UI-canvas text component; `GetComponent<TextMeshProUGUI>()` works without virtual dispatch.
- **Cons**: Prevents assigning a world-space `TextMeshPro` component to the same field without changing the declaring class. Unity 6.3 supports mixed 2D/3D rendering in the same scene, making world-space text plausible for damage numbers or floating labels in future sprints.
- **Rejection Reason**: `TMP_Text` carries negligible overhead and future-proofs serialised fields against world-space use without requiring a field type change across all components. The concrete type `TextMeshProUGUI` is still used at runtime-creation sites where the Canvas context is guaranteed.

### Alternative 2: TMP Wrapper Component (ITMPText Interface)
- **Description**: Define a custom `ITMPText` interface or a wrapper `MonoBehaviour` that abstracts over TMP variants, allowing code to depend on an interface rather than the TMP class hierarchy.
- **Pros**: Complete decoupling from the TMP package API surface; easier to mock in unit tests.
- **Cons**: Significant boilerplate for wrapping a well-established, stable package API. The `TMP_Text` base class already provides the abstraction (`text`, `fontSize`, `color`, `alignment` properties are virtual on the base). An additional interface layer adds indirection without meaningful benefit at current scale.
- **Rejection Reason**: Over-engineering. `TMP_Text` is the correct abstraction level — it is the base class specifically designed for polymorphic text use in TMP's own type hierarchy.

### Alternative 3: Prefab-Based Text (LabelPrefab.prefab with TextMeshProUGUI)
- **Description**: Author a minimal `LabelPrefab` with a `TextMeshProUGUI` pre-configured, and instantiate it wherever dynamic text creation is needed instead of using `new GameObject(...)`.
- **Pros**: Styling (font, font size, color, material) lives in the prefab Inspector rather than code; easier for non-programmers to adjust.
- **Cons**: Adds a runtime `Instantiate()` call (possible GC allocation) and requires loading the prefab via Addressables. For the 100+ dynamically created archive entries, prefab instantiation introduces more overhead than direct `new GameObject` construction. The font injection pattern (`_font` field) already provides designer-adjustable font configuration without a prefab.
- **Rejection Reason**: Direct `new GameObject` construction is simpler and faster for code-generated UI at the current scale. Prefab-based rows would be reconsidered if entries required rich layout that code configuration cannot cleanly support.

## Consequences

### Positive
- `TMP_Text` as the serialised field type is forward-compatible with any TMP subclass, including future `TextMeshPro` world-space instances.
- `raycastTarget = false` is explicitly set on all non-interactive runtime-generated labels, preventing invisible raycasting overhead on 100+ archive entries.
- `TMP_FontAsset` injection via Inspector lets the art team assign the project's pixel font to any panel without a code change.
- Rich-text tags (`<color>`, `<b>`, `<size>`) work correctly on `TMP_Text.text` assignments, enabling styled labels in the archive, stat panel, and tutorial hints within the same text field.
- Legacy `TutorialOverlay` required only a type change (`Text` → `TMP_Text`) with no logic changes — the migration had zero risk of behaviour regression.

### Negative
- `GetComponent<TextMeshProUGUI>()` after `new GameObject("...", typeof(TextMeshProUGUI))` is repeated boilerplate in `SignalArchiveScreen` and `StatPointUI`. A private helper method (`CreateTMPLabel(Transform parent, string text, ...)`) would reduce duplication. This is a known quality improvement deferred to a future polish sprint.
- `TMP_FontAsset` is not exposed on all runtime-generating components (e.g., `SignalArchiveScreen` does not have a `_font` field). Archive entries will always use TMP's default fallback font unless this is added. Current visual fidelity is acceptable; the gap is documented.
- TMP components added via `typeof(TextMeshProUGUI)` in `new GameObject(...)` will use the default TMP settings asset. If the project's TMP Settings asset specifies a different default font, behaviour will vary between prefab-authored text and code-authored text. This must be verified during visual QA.

### Risks
- **Missing `Unity.TextMeshPro` reference in future assemblies.** If a new assembly is created for a UI subsystem (e.g., `Synthborn.SkillTreeUI`) and the programmer forgets to add `Unity.TextMeshPro` to its `.asmdef`, compilation will fail with an opaque `TMP_Text type not found` error. Mitigation: Document the TMP reference requirement in the coding standards; the code review checklist for new UI systems should verify the asmdef.
- **TMP version drift in Unity 6.x.** TMP is now bundled with Unity as `com.unity.textmeshpro` and the namespace/class names are stable. However, Unity 6.3's package version is newer than the LLM's knowledge cutoff. Any future Unity upgrade that moves TMP to a different package name would require updating all `.asmdef` reference strings. Mitigation: Pin the TMP package version in `manifest.json` and verify against the upgrade guide during engine updates.
- **`_font == null` silent fallback.** Components that expose `_font` but leave it unassigned in a scene will use TMP's project-default font without any warning. If the project default is not the pixel font, archive entries and stat rows will render in the wrong typeface. Mitigation: Add a `Debug.LogWarning` in `Awake()` or `Start()` for components where custom font is expected.

## Performance Implications
- **CPU**: `TMP_Text` virtual property access (`text`, `fontSize`, etc.) is negligible. Runtime `new GameObject` + `GetComponent` calls in `StatPointUI.Refresh()` and `SignalArchiveScreen.PopulateList()` occur on button press or screen open, not per frame — acceptable.
- **Memory**: Each `TextMeshProUGUI` component holds a mesh and material instance. For 100+ archive entries, each visible only within the archive screen, Unity's destroy-on-clear pattern in `PopulateList()` means at most one screen's worth of TMP meshes is resident at a time.
- **Load Time**: `TMP_FontAsset` assets are loaded on first reference (when the panel opens). No synchronous load at scene startup.
- **Network**: Not applicable.

## Validation Criteria
- All `[SerializeField]` text fields in the project declare `TMP_Text`, not `TextMeshProUGUI` or `TextMeshPro` (verifiable by code search).
- `SignalArchiveScreen.Show()` renders category headers and entry labels with correct fonts and colours with no null reference exceptions, regardless of whether `_font` is assigned.
- `StatPointUI.Refresh()` with a `TMP_FontAsset` assigned: rendered labels use the custom font. With no font assigned: rendered labels use the TMP default without exception.
- `TutorialOverlay.LegacyTutorialSequence()` displays all four hints correctly when `TutorialManager` is absent from the scene.
- Unity assembly compilation succeeds with `Unity.TextMeshPro` present only in `Synthborn.UI.asmdef`; no other assembly declares it.
- `raycastTarget = false` is set on all non-interactive TMP labels created at runtime (verifiable by selecting the GameObjects in the Inspector during a play session and checking the TMP component).

## Related Decisions
- ADR-001: Core Architecture Patterns — assembly separation rules that constrain where `Unity.TextMeshPro` may be referenced.
- ADR-005: Lore Delivery System — `SignalArchiveScreen` runtime UI generation is a primary application of the `TextMeshProUGUI` runtime creation pattern documented here.
- ADR-006: Event-Driven Tutorial System — `TutorialOverlay._hintText` uses the `TMP_Text` base-class serialised field pattern; legacy fallback migration is a direct consequence of this decision.
