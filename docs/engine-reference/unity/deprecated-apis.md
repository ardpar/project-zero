# Unity 6.3 LTS — Deprecated APIs

**Last verified:** 2026-03-27

Quick lookup table for deprecated APIs and their replacements.
Format: **Don't use X** → **Use Y instead**

---

## Input

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| `Input.GetKey()` | `Keyboard.current[Key.X].isPressed` | New Input System |
| `Input.GetKeyDown()` | `Keyboard.current[Key.X].wasPressedThisFrame` | New Input System |
| `Input.GetMouseButton()` | `Mouse.current.leftButton.isPressed` | New Input System |
| `Input.GetAxis()` | `InputAction` callbacks | New Input System |
| `Input.mousePosition` | `Mouse.current.position.ReadValue()` | New Input System |

---

## Rendering / URP (6.2+ changes)

| Deprecated | Replacement | Since |
|------------|-------------|-------|
| `SetupRenderPasses` (Scriptable Renderer Features) | Render Graph + `AddRenderPasses` API | 6.2 |
| `RenderGraphSettings.enableRenderCompatibilityMode` (write) | Read-only in 6.3; convert to Render Graph before upgrading | 6.3 |
| `RenderPipelineEditorUtility.FetchFirstCompatibleTypeUsingScriptableRenderPipelineExtension` | `GetDerivedTypesSupportedOnCurrentPipeline` | 6.2 |
| `CustomEditorForRenderPipelineAttribute` | `CustomEditor` | 6.2 |
| `VolumeComponentMenuForRenderPipelineAttribute` | `VolumeComponentMenu` | 6.2 |
| `CommandBuffer.DrawMesh()` (in SRP) | RenderGraph API | 6.0 |
| `OnPreRender()` / `OnPostRender()` | `RenderPipelineManager` callbacks | 6.0 |
| `Camera.SetReplacementShader()` | Custom render pass | 6.0 |

## Shaders / Textures (6.2+ changes)

| Deprecated | Replacement | Since |
|------------|-------------|-------|
| Legacy shader APIs (set deprecated in 6.2) | Check 6.2 release notes for specifics | 6.2 |
| Old `Texture2D` constructors | `Texture2D` constructors with `MipmapLimitDescriptor` | 6.2 |

## Accessibility (6.3 change)

| Deprecated | Replacement | Since |
|------------|-------------|-------|
| `AccessibilityRole` as flags enum (bitwise ops) | `AccessibilityRole` as standard enum (single value) | 6.3 |

---

## UI

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| `Canvas` (UGUI) | `UIDocument` (UI Toolkit) | UI Toolkit is production-ready in Unity 6 |
| `Text` component | `TextMeshPro` or UI Toolkit `Label` | Better rendering |
| `Image` component | UI Toolkit `VisualElement` with background | More flexible |

---

## DOTS/Entities

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| `ComponentSystem` | `ISystem` (unmanaged) | Entities 1.0+ |
| `JobComponentSystem` | `ISystem` with `IJobEntity` | Burst-compatible |
| `GameObjectEntity` | Pure ECS workflow | No GameObject conversion |
| `ComponentDataFromEntity<T>` | `ComponentLookup<T>` | Entities 1.0+ rename |

---

## AI / Navigation

| Deprecated | Replacement | Since |
|------------|-------------|-------|
| `UnityEngine.Experimental.AI` APIs | Most obsolete with no direct replacement; use NavMesh APIs | 6.2+ |

## Packages

| Deprecated | Replacement | Since |
|------------|-------------|-------|
| Sequences package | Custom timeline workflows | 6.1 |
| Live-capture package | Third-party alternatives | 6.1 |
| Python for Unity package | External Python tooling | 6.1 |
| Social API | Platform Toolkit (6.3+) or platform-specific SDKs | 6.0+ |

---

## Asset Loading

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| `Resources.Load()` | Addressables | Better memory control |
| Synchronous asset loading | `Addressables.LoadAssetAsync()` | Non-blocking |

## Physics

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| `Physics.RaycastAll()` | `Physics.RaycastNonAlloc()` | Avoid GC allocations |

## Animation

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| Legacy Animation component | Animator Controller | Mecanim system |

## Particles

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| Legacy Particle System | Visual Effect Graph | GPU-accelerated |

## Scripting

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| `WWW` class | `UnityWebRequest` | Modern async networking |
| `Application.LoadLevel()` | `SceneManager.LoadScene()` | Scene management |

---

## General Rule

When uncertain about an API's status in Unity 6.3:
1. Check this file first
2. Then `breaking-changes.md` for version-specific changes
3. If still unclear, use WebSearch to verify against official docs

**Sources:**
- https://docs.unity3d.com/6000.3/Documentation/Manual/
- https://docs.unity3d.com/6000.2/Documentation/Manual/UpgradeGuideUnity62.html
- https://docs.unity3d.com/6000.4/Documentation/Manual/UpgradeGuideUnity63.html
