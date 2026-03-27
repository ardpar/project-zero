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
- **Draw Calls**: [TO BE CONFIGURED — profile after MVP]
- **Memory Ceiling**: [TO BE CONFIGURED — profile after MVP]

## Testing

- **Framework**: Unity Test Framework (NUnit-based)
- **Minimum Coverage**: [TO BE CONFIGURED]
- **Required Tests**: Balance formulas, gameplay systems, enemy spawning

## Forbidden Patterns

<!-- Add patterns that should never appear in this project's codebase -->
- Legacy `Input` class — use Input System package
- `Resources.Load()` — use Addressables
- Built-in Render Pipeline — use URP
- `ComponentSystem` (old DOTS) — use `ISystem`

## Allowed Libraries / Addons

<!-- Add approved third-party dependencies here -->
- [None configured yet — add as dependencies are approved]

## Architecture Decisions Log

<!-- Quick reference linking to full ADRs in docs/architecture/ -->
- [No ADRs yet — use /architecture-decision to create one]
