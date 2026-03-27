# Unity Engine — Version Reference

| Field | Value |
|-------|-------|
| **Engine Version** | Unity 6.3 LTS (6000.3.11f1) |
| **Release Date** | December 2025 |
| **Project Pinned** | 2026-03-27 |
| **Last Docs Verified** | 2026-03-27 |
| **LLM Knowledge Cutoff** | May 2025 |
| **LTS Support Until** | December 2027 |

## Knowledge Gap Warning

The LLM's training data likely covers Unity up to ~6.0 LTS / early 6000.0.x.
Versions 6.1, 6.2, and 6.3 introduced significant changes that the model may
NOT know about. Always cross-reference this directory before suggesting Unity
API calls.

## Post-Cutoff Version Timeline

| Version | Release | Risk Level | Key Theme |
|---------|---------|------------|-----------|
| 6.0 LTS | Apr 2025 | LOW | Unity 6 rebrand, Render Graph default, DOTS production-ready, Input System default |
| 6.1 | Mid 2025 | MEDIUM | DirectX12 default (Win), Tile Set asset, VRS API, Facebook Instant Games, TLS 1.0/1.1 removed |
| 6.2 | Late 2025 | HIGH | Shader API deprecations, SetupRenderPasses deprecated, AfterRendering behavior change, Texture2D constructor changes |
| 6.3 LTS | Dec 2025 | HIGH | Box2D v3 2D physics, Platform Toolkit, Scriptable Audio, USS parser strictness, AccessibilityRole enum change, Render Graph Compatibility Mode read-only |

## Key Features for This Project (SYNTHBORN — 2D Pixel Art Survivor)

- **2D + 3D Mixed Rendering (6.3)**: 2D URP Renderer supports 3D elements in same scene
- **Box2D v3 Physics (6.3)**: New low-level 2D physics API — better performance for many-body scenarios
- **Sprite Atlas Analyzer (6.3)**: Tool to detect sprite atlas performance issues
- **Legacy Animation 30% faster (6.3)**: Significant for many-entity games
- **SRP Batcher**: Preferred over dynamic batching for modern Unity 6

## Verified Sources

- Official docs: https://docs.unity3d.com/6000.3/Documentation/Manual/
- 6.0 upgrade guide: https://docs.unity3d.com/6000.3/Documentation/Manual/UpgradeGuideUnity6.html
- 6.1 upgrade guide: https://docs.unity3d.com/6000.1/Documentation/Manual/UpgradeGuideUnity61.html
- 6.2 upgrade guide: https://docs.unity3d.com/6000.2/Documentation/Manual/UpgradeGuideUnity62.html
- 6.3 upgrade guide: https://docs.unity3d.com/6000.4/Documentation/Manual/UpgradeGuideUnity63.html
- What's new in 6.3: https://docs.unity3d.com/6000.3/Documentation/Manual/WhatsNewUnity63.html
- Release support: https://unity.com/releases/unity-6/support
