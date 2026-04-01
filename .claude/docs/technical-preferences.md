# Technical Preferences

<!-- Populated by /setup-engine. Updated as the user makes decisions throughout development. -->
<!-- All agents reference this file for project-specific standards and conventions. -->

## Engine & Language

- **Engine**: Unity 6.3 LTS (6000.3.11f1)
- **Language**: C#
- **Rendering**: URP (Universal Render Pipeline) — 2D Renderer
- **Physics**: Unity 2D Physics (Box2D v3 API available in 6.3)

## Naming Conventions

- **Classes**: PascalCase (e.g., `PlayerController`)
- **Public fields/properties**: PascalCase (e.g., `MoveSpeed`)
- **Private fields**: _camelCase (e.g., `_moveSpeed`)
- **Methods**: PascalCase (e.g., `TakeDamage()`)
- **Events/Delegates**: PascalCase with `On` prefix (e.g., `OnHealthChanged`)
- **Files**: PascalCase matching class (e.g., `PlayerController.cs`)
- **Scenes/Prefabs**: PascalCase (e.g., `MainMenu.unity`, `EnemyBat.prefab`)
- **Constants**: PascalCase or UPPER_SNAKE_CASE (e.g., `MaxHealth` or `MAX_HEALTH`)

## Performance Budgets

- **Target Framerate**: 60 fps
- **Frame Budget**: 16.6 ms
- **Draw Calls**: [PENDING BUILD PROFILING — editor baseline captured 2026-04-02]
- **Memory Ceiling**: [PENDING BUILD PROFILING — estimated 200-400 MB for standalone]
- **Profiling Baseline**: `tests/performance/profiling-baseline-2026-04-02.md`

## Testing

- **Framework**: Unity Test Framework (NUnit-based)
- **Test Assembly**: `Synthborn.Tests.EditMode` (41 tests passing)
- **Minimum Coverage**: Core stat systems, progression formulas, config defaults
- **Required Tests**: Balance formulas, gameplay systems, enemy spawning

## Forbidden Patterns

<!-- Add patterns that should never appear in this project's codebase -->
- Legacy `Input` class — use Input System package
- `Resources.Load()` — use Addressables
- Built-in Render Pipeline — use URP
- `ComponentSystem` (old DOTS) — use `ISystem`
- `UnityEngine.UI.Text` — use `TMPro.TMP_Text` / `TextMeshProUGUI` (migrated Sprint 18)
- `Shader.Find()` at runtime — use serialized material references
- `new List<T>()` in hot paths — pre-allocate and reuse
- `Vector2.Distance()` for range checks — use `sqrMagnitude`

## Allowed Libraries / Addons

<!-- Add approved third-party dependencies here -->
- [None configured yet — add as dependencies are approved]

## Architecture Decisions Log

<!-- Quick reference linking to full ADRs in docs/architecture/ -->
- ADR-003: ObjectPool System (`docs/architecture/adr-003-object-pool.md`)
- ADR-004: Adaptation Points System (`docs/architecture/adr-004-adaptation-points.md`)
