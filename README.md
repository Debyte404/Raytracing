# 🌌 Unity Compute Shader Raytracing Lab

> A beginner-friendly Unity raytracing playground built with compute shaders, mathematical spheres, GPU buffers, and Scene View visualizers.

This project is inspired by Sebastian Lague's wonderful video [Coding Adventure: Ray Tracing](https://www.youtube.com/watch?v=Qz0KTGYJtUk). I am using it as a learning lab to rebuild the core ideas step by step inside Unity: camera rays, ray-sphere intersections, materials, emission, and eventually path tracing.

## 📸 Screenshots

Paste your screenshots into `docs/images/` and update these image paths.

| Scene View | Game View |
| --- | --- |
| ![Scene View placeholder](docs/images/scene-view.png) | ![Game View placeholder](docs/images/game-view.png) |

| Material Inspector | Compute Shader Result |
| --- | --- |
| ![Material Inspector placeholder](docs/images/material-inspector.png) | ![Compute Shader Result placeholder](docs/images/compute-shader-result.png) |

## ✨ What It Does

- Shoots one ray per pixel from the active Unity camera
- Uses a compute shader to render into a `RenderTexture`
- Uploads Unity sphere data to the GPU with a `ComputeBuffer`
- Intersects rays with mathematical spheres instead of mesh triangles
- Supports per-sphere material properties:
  - base color
  - emission color
  - emission strength
  - roughness
- Shows the result in Game View and Scene View
- Includes a fly camera controller using Unity's Input System
- Includes educational visualizer scripts for raytracing concepts

## 🧠 Concepts Learned

This project helped me learn:

- how pixels become world-space rays
- how camera matrices are passed to compute shaders
- how ray-sphere collision works with the quadratic formula
- how to store closest-hit data in a `hitInfo` struct
- how Unity C# structs must match HLSL buffer layout
- why GPU data is usually packed in groups of four floats
- how material data can travel from the Inspector to the GPU
- how emission is added to a raytraced color
- how editor lifecycle bugs happen with `ComputeBuffer` and `RenderTexture`
- how Scene View image effects work with `[ExecuteAlways]`

More detail: [docs/CONCEPTS.md](docs/CONCEPTS.md)

## 🕹️ Controls

Attach `cameracontroll` to the camera.

| Input | Action |
| --- | --- |
| `WASD` | Move |
| `Space` | Move up |
| `Left Ctrl` | Move down |
| `Left Shift` | Sprint |
| Hold right mouse | Look around |

## 🗂️ Project Layout

```text
Assets/
  attempt1/
    customtracer.compute        # Current compute raytracer
    RaytracingSphere.cs         # Sphere data + material properties
    cameracontroll.cs           # Fly camera controller

  basics/
    shadermanager.cs            # Camera effect, GPU upload, dispatch
    ComputeBlend.shader         # Blends compute texture to camera output
    *Visualizer.cs              # Scene View learning helpers

  Scenes/
    hellowworld.unity
    SampleScene.unity

docs/
  CONCEPTS.md
  README.md
  tutorials/
    unity-compute-shader-raytracing.md
  images/
    paste screenshots here
```

## 🚀 Getting Started

1. Open the project in Unity `6000.0.23f1` or a compatible Unity 6 editor.
2. Open `Assets/Scenes/hellowworld.unity` or `Assets/Scenes/SampleScene.unity`.
3. Select the camera.
4. On `shadermanager`, assign:
   - `testingshader` -> `Assets/attempt1/customtracer.compute`
   - `blendMaterial` -> the compute blend material
   - `Spheres` -> any GameObjects with `RaytracingSphere`
5. Add `RaytracingSphere` to Unity sphere GameObjects.
6. Change color and emission values from the Inspector.
7. Press Play or enable Scene View shader preview.

## 📖 Tutorial

There is one cleaned-up tutorial now:

👉 [Unity Compute Shader Raytracing Tutorial](docs/tutorials/unity-compute-shader-raytracing.md)

Older AI-generated tutorial drafts were removed from the active docs because they repeated themselves and drifted away from the current code.

## 🔬 How The Renderer Works

The render loop is:

```text
Camera renders
    ↓
shadermanager gathers sphere data
    ↓
ComputeBuffer uploads spheres/materials
    ↓
customtracer.compute traces rays
    ↓
Result texture is blended back to camera
```

The GPU sphere layout is packed as:

```text
position.xyz + radius
color.rgb + emissionStrength
emissionColor.rgb + roughness
```

That is `12 floats` per sphere, so the C# buffer stride is:

```csharp
sizeof(float) * 12
```

## 🛠️ Current Status

Implemented:

- camera rays
- analytic sphere intersections
- closest-hit tracking
- material upload
- emission color
- Scene View preview
- fly camera movement
- educational gizmo visualizers

Next ideas:

- sky gradient
- emissive light spheres
- shadows
- diffuse bounce rays
- accumulation over frames
- rough reflections
- Cornell box scene

## 🙏 Credit

Inspired by Sebastian Lague's [Coding Adventure: Ray Tracing](https://www.youtube.com/watch?v=Qz0KTGYJtUk). His video made the raytracing concepts feel approachable, and this repo is my Unity-based learning version of those ideas.

