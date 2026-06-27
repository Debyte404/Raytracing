# Unity Compute Shader Raytracing Tutorial

This is the canonical tutorial for this repository. Older generated tutorial drafts were merged into this one article so the explanations, code structure, and project layout stay consistent.

The project is inspired by Sebastian Lague's [Coding Adventure: Ray Tracing](https://www.youtube.com/watch?v=Qz0KTGYJtUk). The video is a fantastic mental model for the renderer. This tutorial follows the same learning spirit, but maps the ideas to this Unity project: compute shaders, Scene View visualizers, Unity sphere handles, and GPU buffers.

## 1. The Goal

Normal Unity rendering draws meshes with triangles. This project uses Unity differently:

```text
Unity scene objects -> simple data -> compute shader -> raytraced image
```

The sphere GameObjects still exist in the scene, but the compute shader does not trace their triangles. Instead, each sphere sends only:

```text
position
radius
material
```

That lets the GPU use fast mathematical ray-sphere intersection.

## 2. The Main Files

```text
Assets/attempt1/customtracer.compute
```

The compute shader. It runs once per pixel, creates a ray, tests that ray against all uploaded spheres, and writes the final color.

```text
Assets/attempt1/RaytracingSphere.cs
```

The component attached to Unity sphere GameObjects. It exposes material data in the Inspector and derives position/radius from the transform.

```text
Assets/basics/shadermanager.cs
```

The camera-side bridge. It creates the render texture, uploads sphere data through a `ComputeBuffer`, sends camera matrices, dispatches the compute shader, and blends the result back into the camera output.

```text
Assets/attempt1/cameracontroll.cs
```

The fly camera controller using Unity's Input System package.

## 3. How A Pixel Becomes A Ray

The compute shader runs in groups of threads:

```hlsl
[numthreads(8, 8, 1)]
void HelloWorld(uint3 id : SV_DispatchThreadID)
```

Each `id.xy` is one pixel. The shader first turns that pixel into normalized UV coordinates:

```hlsl
float2 uv = (id.xy + 0.5) / float2(width, height);
float2 screenSpace = uv * 2.0 - 1.0;
```

Then it uses the inverse projection matrix and camera-to-world matrix to build a world-space ray:

```hlsl
float4 viewTarget = mul(_CameraInverseProjection, float4(screenSpace, 1, 1));
viewTarget /= viewTarget.w;

float3 worldTarget = mul(_CameraToWorld, viewTarget).xyz;

ray.rayOrigin = _CameraPosition;
ray.rayDir = normalize(worldTarget - ray.rayOrigin);
```

The ray means:

```text
start at the camera
move through this pixel
continue into the scene
```

That is the foundation of raytracing.

## 4. Ray-Sphere Intersection

A ray can be written like this:

```text
point = origin + direction * distance
```

A sphere is all points that are `radius` units away from its center:

```text
distance(point, sphereCenter) = radius
```

Substituting the ray equation into the sphere equation creates a quadratic:

```text
a*t*t + b*t + c = 0
```

In the shader, that becomes:

```hlsl
float3 offset = ray.rayOrigin - sphere.position;

float a = dot(ray.rayDir, ray.rayDir);
float b = 2.0 * dot(offset, ray.rayDir);
float c = dot(offset, offset) - sphere.radius * sphere.radius;

float discriminant = b * b - 4.0 * a * c;
```

The discriminant tells us the result:

```text
less than 0 -> no hit
equal to 0 -> tangent hit
greater than 0 -> ray enters and exits the sphere
```

When the ray hits, the shader chooses the closest positive distance:

```hlsl
float t1 = (-b - sqrtDiscriminant) / (2.0 * a);
float t2 = (-b + sqrtDiscriminant) / (2.0 * a);
float dst = t1 > 0 ? t1 : t2;
```

Negative distances are behind the camera, so they are ignored.

## 5. Hit Information

The shader stores collision results in `hitInfo`:

```hlsl
struct hitInfo
{
    bool didHit;
    float dst;
    float3 hitpoint;
    float3 normal;
    MaterialProperty material;
};
```

This struct answers:

```text
Did the ray hit?
How far away was the hit?
Where did it hit?
What direction is the surface facing?
What material was hit?
```

When testing multiple spheres, the shader keeps the closest hit:

```hlsl
for (int i = 0; i < _SphereCount; i++)
{
    hitInfo currentHit = RaySphere(ray, _Spheres[i]);

    if (currentHit.didHit && currentHit.dst < ray.HitInfo.dst)
    {
        ray.HitInfo = currentHit;
    }
}
```

This is why objects closer to the camera appear in front of objects behind them.

## 6. Unity Spheres As Authoring Handles

`RaytracingSphere.cs` keeps the Unity side simple:

```csharp
public Vector3 Position => transform.position;

public float Radius => 0.5f * Mathf.Max(
    transform.lossyScale.x,
    transform.lossyScale.y,
    transform.lossyScale.z
);
```

The default Unity sphere has a diameter of `1`, so its base radius is `0.5`. Scaling the transform changes the mathematical radius.

For this learning renderer, use uniform sphere scale when possible. Non-uniform scale visually creates an ellipsoid in Unity, but the compute shader still treats it as a sphere using the largest axis.

## 7. Material Properties

The material data lives in a serializable C# struct:

```csharp
[System.Serializable]
public struct MaterialProperty
{
    public Color color;
    public Color emissionColor;
    [Min(0f)] public float emissionStrength;
    [Range(0f, 1f)] public float roughness;
}
```

`[System.Serializable]` lets Unity show and save the struct inside the Inspector.

The GPU receives a matching layout:

```hlsl
struct MaterialProperty
{
    float3 color;
    float emissionStrength;
    float3 emissionColor;
    float roughness;
};
```

The field names do not matter as much as the memory order and size. This project packs each sphere as three groups of four floats:

```text
position.xyz + radius
color.rgb + emissionStrength
emissionColor.rgb + roughness
```

That is why the buffer stride is:

```csharp
sizeof(float) * 12
```

## 8. Uploading Spheres To The GPU

The shader manager converts all `RaytracingSphere` components into `SphereData`:

```csharp
struct SphereData
{
    public Vector3 position;
    public float radius;
    public MaterialPropertyData material;
}
```

Then it uploads the array:

```csharp
sphereBuffer.SetData(sphereData);
testingshader.SetBuffer(kernelIndex, "_Spheres", sphereBuffer);
testingshader.SetInt("_SphereCount", sphereData.Length);
```

The compute shader reads it as:

```hlsl
StructuredBuffer<Sphere> _Spheres;
int _SphereCount;
```

This is the key CPU-to-GPU bridge.

## 9. Coloring The Hit

The current shader uses the hit sphere's material:

```hlsl
float3 litColor = ray.HitInfo.material.color;
float3 emission = ray.HitInfo.material.emissionColor * ray.HitInfo.material.emissionStrength;
Result[id.xy] = float4(litColor + emission, 1);
```

Earlier debug versions used normals as color:

```hlsl
ray.HitInfo.normal * 0.5 + 0.5
```

That debug view is useful because normals naturally show surface direction as RGB colors.

## 10. Scene View Support

The shader manager uses:

```csharp
[ExecuteAlways]
[ImageEffectAllowedInSceneView]
```

This lets the raytraced result appear in the Scene View, not only the Game View. Because editor scripts can survive play mode transitions in unusual ways, the manager explicitly releases and recreates GPU resources:

```text
ComputeBuffer
RenderTexture
Scene View repaint
```

This avoids stale GPU state after entering and exiting Play Mode.

## 11. Camera Controls

The camera controller uses Unity's Input System package.

Controls:

```text
WASD        move
Space       move up
Left Ctrl   move down
Left Shift  sprint
Right mouse look around
```

If you see an error about `UnityEngine.Input`, the project is using the new Input System and old input calls should not be used.

## 12. Learning Path

Use this order when studying the project:

1. Open the scene and confirm the camera image effect is working.
2. Attach `RaytracingSphere` to one Unity sphere.
3. Drag that sphere into the `Spheres` array on `shadermanager`.
4. Change the sphere's color in the Inspector.
5. Move the sphere and watch the raytraced result follow.
6. Read the ray generation code in `customtracer.compute`.
7. Read `RaySphere` and trace how `hitInfo` is filled.
8. Read `UpdateSphereBuffer` in `shadermanager.cs`.
9. Add another material property, such as metallic or specular.
10. Add lighting, shadows, or bounces.

## 13. Concepts From Sebastian Lague's Video

The concepts from Sebastian Lague's raytracing explanation that show up directly in this project are:

- shooting rays from the camera into the world
- representing objects mathematically
- solving ray-object intersections
- keeping the nearest hit
- using surface normals for shading
- giving objects material properties
- adding emission as a light-like contribution
- preparing for random bounces and path tracing

The project is currently focused on the early and middle stages: rays, spheres, hit data, materials, emission, and editor visualization. The next big step is true light transport: multiple bounces, random hemisphere sampling, accumulation, and noise reduction.

## 14. Screenshots To Add

Drop screenshots into `docs/images/` and link them from the root README.

Suggested screenshots:

```text
docs/images/scene-view.png
docs/images/game-view.png
docs/images/material-inspector.png
docs/images/compute-shader-result.png
```

## 15. Where To Go Next

Good next features:

- sky gradient background
- light-emitting spheres
- hard shadows
- diffuse bounce rays
- frame accumulation
- camera-movement reset for accumulation
- roughness-driven reflections
- plane and box primitives
- a small Cornell box scene

