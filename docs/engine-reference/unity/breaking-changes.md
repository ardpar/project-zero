# Unity 6.3 LTS — Breaking Changes

**Last verified:** 2026-03-27

This document tracks breaking API changes and behavioral differences between Unity 2022 LTS
(likely in model training) and Unity 6.3 LTS (current version). Organized by version.

---

## Unity 6.0 (from 2022 LTS) — HIGH RISK

### Entities/DOTS API Complete Overhaul

```csharp
// OLD (pre-Unity 6, GameObjectEntity pattern)
public class HealthComponent : ComponentData {
    public float Value;
}

// NEW (Unity 6+, IComponentData)
public struct HealthComponent : IComponentData {
    public float Value;
}

// OLD: ComponentSystem
public class DamageSystem : ComponentSystem { }

// NEW: ISystem (unmanaged, Burst-compatible)
public partial struct DamageSystem : ISystem {
    public void OnCreate(ref SystemState state) { }
    public void OnUpdate(ref SystemState state) { }
}
```

### Input System — Legacy Input Deprecated

```csharp
// OLD: Input class (deprecated)
if (Input.GetKeyDown(KeyCode.Space)) { }

// NEW: Input System package
using UnityEngine.InputSystem;
if (Keyboard.current.spaceKey.wasPressedThisFrame) { }
```

### URP/HDRP Renderer Feature API Changes

```csharp
// OLD: ScriptableRenderPass.Execute signature
public override void Execute(ScriptableRenderContext context, ref RenderingData data)

// NEW: Uses RenderGraph API
public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
```

### Other 6.0 Changes
- Addressables: asset loading failures now throw exceptions (use try/catch or TryLoad)
- Physics: default solver iterations increased
- WebGL: WebGPU is now the default
- Android: minimum API level raised to 24 (Android 7.0)
- iOS: minimum deployment target raised to iOS 13

---

## Unity 6.1 (from 6.0) — MEDIUM RISK

| Area | Change | Action Required |
|------|--------|-----------------|
| **Graphics API** | Default Auto Graphics API is now DirectX12 (Windows) | New projects use DX12. Change in Player Settings if needed |
| **Package Manager** | Custom keyboard shortcuts to PM/Asset Store/Services windows broken | Recreate shortcuts — windows moved to new submenus |
| **Networking** | MbedTLS updated to 3.6 — TLS 1.0 and 1.1 removed | Ensure servers/APIs support TLS 1.2+ |
| **Platform** | Windows 7 support removed from UPM | No action unless targeting Win7 |
| **Packages** | Sequences, Live-capture, Python for Unity deprecated | Migrate to alternatives |

### New Features in 6.1
- **Tile Set asset** — generative tile capabilities, auto-propagation of source changes
- **Variable Rate Shading (VRS) API** — per-feature shading rate control in URP
- **Project Auditor** — early problem detection with resolution guidance
- **Facebook Instant Games** — Web build target support

---

## Unity 6.2 (from 6.1) — HIGH RISK

| Area | Change | Action Required |
|------|--------|-----------------|
| **URP** | `SetupRenderPasses` API deprecated | Rewrite Scriptable Renderer Features using Render Graph + `AddRenderPasses` API |
| **Shaders** | Set of shader APIs deprecated | Check 6.2 release notes for full list; migrate to replacement APIs |
| **Rendering** | `AfterRendering` injection point now always executes after final blit | Update any code relying on pre-blit AfterRendering behavior |
| **Editor** | `RenderPipelineEditorUtility.FetchFirstCompatibleTypeUsingScriptableRenderPipelineExtension` deprecated | Use `GetDerivedTypesSupportedOnCurrentPipeline` |
| **Attributes** | `CustomEditorForRenderPipelineAttribute` and `VolumeComponentMenuForRenderPipelineAttribute` deprecated | Use `CustomEditor` and `VolumeComponentMenu` |
| **Textures** | Old `Texture2D` constructors deprecated | Use constructors with `MipmapLimitDescriptor` |

---

## Unity 6.3 LTS (from 6.2) — HIGH RISK

| Area | Change | Action Required |
|------|--------|-----------------|
| **Render Graph** | `RenderGraphSettings.enableRenderCompatibilityMode` now read-only | Must convert to URP Render Graph before upgrading to 6.3 |
| **UI Toolkit** | USS parser upgraded — stricter validation | Fix invalid USS that was previously silently accepted |
| **Accessibility** | `AccessibilityRole` changed from flags enum to standard enum | Remove any bitwise operations on AccessibilityRole values |
| **2D Physics** | New low-level 2D physics API based on Box2D v3 | Runs alongside existing API; eventual replacement |

### New Features in 6.3 LTS
- **Platform Toolkit** — unified API for accounts, achievements, save data across platforms
- **Scriptable Audio Processors** — Burst-compiled C# audio processing pipeline
- **2D+3D Mixed Rendering** — 2D URP renderer supports 3D elements in same scene
- **Shader Graph Terrain** — create terrain materials/shaders for URP and HDRP
- **xAtlas Lightmap Packing** — new default lightmap algorithm
- **Sprite Atlas Analyzer** — tool to detect sprite atlas performance issues
- **Legacy Animation Performance** — up to 30% faster evaluation
- **Unity Building Blocks** — sample assets for Achievements, Leaderboards, Multiplayer

---

## Migration Checklist (2022 LTS → 6.3 LTS)

- [ ] Audit all DOTS/ECS code (complete rewrite likely needed)
- [ ] Replace `Input` class with Input System package
- [ ] Update custom render passes to RenderGraph API
- [ ] Add exception handling to Addressables calls
- [ ] Test physics behavior (solver iterations changed)
- [ ] Consider migrating UGUI to UI Toolkit for new UI
- [ ] Update WebGL shaders for WebGPU
- [ ] Verify minimum platform versions (Android/iOS)
- [ ] Fix invalid USS (stricter parser in 6.3)
- [ ] Remove bitwise ops on AccessibilityRole

---

**Sources:**
- https://docs.unity3d.com/6000.3/Documentation/Manual/UpgradeGuideUnity6.html
- https://docs.unity3d.com/6000.1/Documentation/Manual/UpgradeGuideUnity61.html
- https://docs.unity3d.com/6000.2/Documentation/Manual/UpgradeGuideUnity62.html
- https://docs.unity3d.com/6000.4/Documentation/Manual/UpgradeGuideUnity63.html
