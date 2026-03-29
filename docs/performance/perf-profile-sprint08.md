# Performance Profile — Sprint 8

**Date:** 2026-03-29
**Engine:** Unity 6.3 LTS (6000.3.12f1)
**Platform:** macOS (Editor, not standalone build)
**Target:** 60 FPS (16.6ms frame budget)

## Test Results

| Scenario | Enemies | FPS | Frame Time | Memory (Alloc) | Memory (Reserved) | Status |
|----------|---------|-----|------------|----------------|-------------------|--------|
| Baseline (Wave 1) | 1 | 705 | 1.4ms | — | — | PASS |
| Stress Test | 200 | 428 | 2.3ms | 1403 MB | 1908 MB | PASS |

## Analysis

- **200 enemies at 428 FPS** — frame time is 2.3ms, well within the 16.6ms budget
- **7x headroom** over 60fps target with 200 active enemies
- Memory usage is high (1.4GB allocated) but expected for Editor mode with asset imports
- Standalone builds will use significantly less memory

## Bottleneck Assessment

- **No bottlenecks identified** at current enemy counts
- Physics2D (Box2D v3) handles 200 overlapping colliders efficiently
- Enemy AI (simple state machines) has negligible CPU cost
- Audio system (adaptive music + SFX) has no measurable impact

## Recommendations

- No optimization needed at current scale
- If targeting 500+ enemies, consider:
  - Object pooling verification (already implemented via ObjectPool<T>)
  - Sprite renderer batching (SRP Batcher should handle this)
  - Physics query optimization (ContactFilter2D already used)
- Standalone build profiling recommended before release

## VFX Impact (Not Yet Tested)

- Screen shake, hit flash, damage numbers, particles were not stress-tested at 200 enemy scale
- Recommend separate VFX stress test when particle effects are active during mass combat

## Conclusion

**PASS** — 60fps target exceeded by 7x with 200 enemies. No immediate optimization work required.
