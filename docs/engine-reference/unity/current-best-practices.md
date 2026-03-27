# Unity 6.3 LTS — Current Best Practices

**Last verified:** 2026-03-27

Modern Unity 6 patterns that may not be in the LLM's training data.
These are production-ready recommendations as of Unity 6.3 LTS.

---

## Project Setup (New Unity 6.3 Projects)

1. Start with **URP 2D Renderer** (not Built-in pipeline)
2. Enable **Render Graph** from the start (Compatibility Mode is read-only in 6.3)
3. Use **Sprite Atlas** from day one — retrofitting is painful
4. Set up **Assembly Definitions** early for compile time optimization
5. Use the **Project Auditor** package for early problem detection

### Choose the Right Render Pipeline
- **URP (Universal)**: Mobile, cross-platform, good performance — recommended for most games
- **HDRP (High Definition)**: High-end PC/console, photorealistic
- **Built-in**: Deprecated, avoid for new projects

---

## 2D Sprite Rendering (SYNTHBORN-relevant)

### Sprite Atlas Management
- **Always use Sprite Atlases** — packing sprites into one atlas reduces draw calls
- **Use the Sprite Atlas Analyzer** (new in 6.3) to detect common mistakes
- **Separate atlases by usage context** — UI, gameplay, VFX in different atlases

### 2D + 3D Mixed Rendering (New in 6.3)
- 2D URP Renderer now supports 3D elements (Mesh Renderer, Skinned Mesh Renderer)
- 3D elements can receive 2D lighting and interact with Sprite Masks
- Use Sorting Groups for proper 2D/3D draw order

### Performance for Many-Entity Games
- **SRP Batcher** over dynamic batching for Unity 6 projects
- **Object pooling** is critical for survivor-type games (enemies, projectiles, XP gems)
- **Tilemaps** over individual GameObjects for static backgrounds
- **Legacy Animation** evaluation is ~30% faster in 6.3 for complex hierarchies
- **GPU Instancing** for thousands of repeated objects (enemies with same mesh)

```csharp
// GPU Instancing for many identical objects
Graphics.RenderMeshInstanced(
    new RenderParams(material),
    mesh,
    0,
    matrices // NativeArray<Matrix4x4>
);
```

---

## Physics (2D)

### Box2D v3 API (New in 6.3)
- New low-level 2D physics API based on Box2D v3
- Runs alongside existing API; will eventually replace it
- Better performance for many-body simulations — ideal for survivor games

---

## Scripting (C# 9+ in Unity 6)

```csharp
// Record types for data
public record PlayerData(string Name, int Level, float Health);

// Init-only properties
public class Config {
    public string GameMode { get; init; }
}

// Pattern matching
var result = enemy switch {
    Boss boss => boss.Enrage(),
    Minion minion => minion.Flee(),
    _ => null
};
```

---

## Input

### Use Input System Package (Not Legacy Input)

```csharp
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour {
    private PlayerControls controls;

    void Awake() {
        controls = new PlayerControls();
        controls.Gameplay.Jump.performed += ctx => Jump();
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();
}
```

---

## UI

### Use UI Toolkit for Runtime UI (Production-Ready in Unity 6)

```csharp
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour {
    void OnEnable() {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var playButton = root.Q<Button>("play-button");
        playButton.clicked += StartGame;
    }
}
```

UXML (structure) + USS (styling) = HTML/CSS-like workflow.

---

## Asset Management

### Use Addressables (Not Resources)

```csharp
using UnityEngine.AddressableAssets;

public async Task SpawnEnemyAsync(string enemyKey) {
    var handle = Addressables.InstantiateAsync(enemyKey);
    var enemy = await handle.Task;
    Addressables.ReleaseInstance(enemy); // Cleanup
}
```

---

## Performance

### Use Burst Compiler + Jobs System

```csharp
[BurstCompile]
struct EnemyUpdateJob : IJobParallelFor {
    public NativeArray<float3> Positions;
    public NativeArray<float3> Velocities;
    public float DeltaTime;

    public void Execute(int index) {
        Positions[index] += Velocities[index] * DeltaTime;
    }
}
```

20-100x faster than equivalent managed C# — critical for survivor games with hundreds of enemies.

### Memory: Use NativeContainers

```csharp
using var data = new NativeArray<int>(1000, Allocator.TempJob);
// Auto-disposed, no GC, Burst-compatible
```

---

## Audio (New in 6.3)

### Scriptable Audio Processors
- Burst-compiled C# audio processing units
- Better performance than old AudioMixer for custom effects
- Customize audio at specific integration points

---

## Cross-Platform (New in 6.3)

### Platform Toolkit
- Single API for accounts, achievements, save data, controller ownership
- Supports PlayStation, Xbox, Switch, Steam, Android, iOS
- Replaces deprecated Social API

---

## Testing

### Unity Test Framework (NUnit-based)

```csharp
[UnityTest]
public IEnumerator Player_TakesDamage_HealthDecreases() {
    var player = new GameObject().AddComponent<Player>();
    player.Health = 100;
    player.TakeDamage(25);
    yield return null;
    Assert.AreEqual(75, player.Health);
}
```

---

## Summary: Unity 6.3 Tech Stack

| Feature | Use This (2026) | Avoid This (Legacy) |
|---------|------------------|----------------------|
| **Input** | Input System package | `Input` class |
| **UI** | UI Toolkit | UGUI (Canvas) |
| **ECS** | ISystem + IJobEntity | ComponentSystem |
| **Rendering** | URP + RenderGraph | Built-in pipeline |
| **Assets** | Addressables | Resources |
| **Jobs** | Burst + IJobParallelFor | Coroutines for heavy work |
| **2D Physics** | Box2D v3 API (6.3+) | Legacy 2D physics (still works) |
| **Audio** | Scriptable Audio Processors | Old AudioMixer (still works) |
| **Platform** | Platform Toolkit | Social API |

---

**Sources:**
- https://docs.unity3d.com/6000.3/Documentation/Manual/
- https://unity.com/blog/unity-6-3-lts-is-now-available
- https://docs.unity3d.com/6000.3/Documentation/Manual/WhatsNewUnity63.html
