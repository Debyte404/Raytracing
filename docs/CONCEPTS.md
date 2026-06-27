# Concepts Learned

This project is a running notebook for learning raytracing through Unity compute shaders. It is inspired by Sebastian Lague's [Coding Adventure: Ray Tracing](https://www.youtube.com/watch?v=Qz0KTGYJtUk), with the ideas rebuilt in a Unity scene so each concept can be inspected and modified.

## Camera Rays

Every pixel becomes a ray. The compute shader takes the pixel coordinate, converts it into screen space, applies the inverse projection matrix, and then transforms it through the camera-to-world matrix.

Key idea:

```text
pixel -> screen space -> view space -> world space ray
```

The ray is stored as:

```text
origin = camera position
direction = normalized vector from camera to world target
```

## Mathematical Spheres

Unity sphere GameObjects are used as authoring handles, but the compute shader does not trace their mesh triangles. Instead, each sphere sends only:

```text
position
radius
material
```

The GPU then uses the ray-sphere equation:

```text
ray point = origin + direction * distance
sphere = all points radius units away from center
```

Solving that gives a quadratic equation. The discriminant tells whether the ray missed, touched, or passed through the sphere.

## Hit Information

The shader records the closest collision in a `hitInfo` struct:

```text
didHit
distance
hit point
normal
material
```

This is the core bridge between geometry and shading. Once a ray knows what it hit, the renderer can decide what color, light, emission, reflection, or bounce should happen next.

## Materials

Material data is owned by `RaytracingSphere` in C# and packed into the compute buffer.

Current material fields:

```text
color
emissionColor
emissionStrength
roughness
```

The GPU layout is packed as three four-float groups:

```text
position.xyz + radius
color.rgb + emissionStrength
emissionColor.rgb + roughness
```

This keeps the C# and HLSL layouts predictable.

## Compute Buffers

The scene is uploaded to the GPU with a `ComputeBuffer`. The important lesson is that the CPU-side struct and HLSL struct must have the same order and size. The field names can differ, but the memory layout must match.

The shader manager also releases buffers and render textures during editor lifecycle events because `[ExecuteAlways]` scripts can survive unusual play mode and scene view transitions.

## Scene View Preview

The renderer supports Scene View previews through:

```csharp
[ExecuteAlways]
[ImageEffectAllowedInSceneView]
```

This makes the project easier to learn from because you can move objects in the editor and immediately see the mathematical raytraced result.

## Visualizers

The `Assets/basics/*Visualizer.cs` scripts are educational helpers. They draw rays, hit points, normals, hemisphere samples, and path tracing steps in the Scene View so the math is not trapped inside the compute shader.

## From Here

The next natural concepts are:

- sky gradients
- diffuse hemisphere bounces
- random sampling
- accumulation over frames
- shadows
- reflection and roughness
- multiple bounces
- temporal reset when the camera or scene changes

