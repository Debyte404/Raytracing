# Raytracing Lab Tutorial Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a beginner-friendly Unity ray tracing lab and expanded HTML tutorial inspired by Sebastian Lague's ray tracing video.

**Architecture:** Add independent Scene View visualizer components that explain one concept each: projection rays, sphere hits, plane hits, hemisphere bounces, and a small bounce path. Expand the HTML article so each lesson pairs a concept, beginner explanation, code snippet, and Unity visualizer.

**Tech Stack:** Unity 6, C#, Gizmos/Handles, standalone HTML/CSS tutorial.

---

### Task 1: Concept Visualizer Scripts

**Files:**
- Create: `Assets/basics/SphereIntersectionVisualizer.cs`
- Create: `Assets/basics/PlaneIntersectionVisualizer.cs`
- Create: `Assets/basics/HemisphereBounceVisualizer.cs`
- Create: `Assets/basics/PathTraceStepVisualizer.cs`
- Create: matching `.meta` files with stable GUIDs

- [x] **Step 1: Create scripts that draw educational gizmos**

Each script must be independent, safe in edit mode, and use only mathematical spheres/planes.

- [x] **Step 2: Keep all controls visible in the Inspector**

Expose ray origin, direction, sphere center/radius, plane point/normal, bounce count, and colors.

### Task 2: Attach Visualizers To Scene Camera

**Files:**
- Modify: `Assets/Scenes/hellowworld.unity`
- Modify: `Assets/Scenes/SampleScene.unity`

- [x] **Step 1: Attach visualizers to Main Camera**

The existing camera already contains `RayDirectionVisualizer`. Add the new visualizers disabled or enabled with safe default values so the user can inspect them immediately.

### Task 3: Expand Tutorial Article

**Files:**
- Modify: `compute-shaders-raytracing-tutorial.html`

- [x] **Step 1: Add a "guided lab" roadmap**

Explain the Unity visualizers and how they map to Sebastian Lague's transcript.

- [x] **Step 2: Add deeper sections for skipped basics**

Cover vectors, rays, dot product, projection plane, ray-sphere hit, ray-plane hit, closest hit, materials, emission, random diffuse bounce, hemisphere sampling, trace loops, noise, and accumulation.

- [x] **Step 3: Keep beginner language**

Assume programming basics only and connect concepts to web development analogies where helpful.
