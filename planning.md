# Volumetric Shape Spawner — Research & Implementation Plan

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [SDK Research Findings](#2-sdk-research-findings)
3. [Architecture Design](#3-architecture-design)
4. [Implementation Plan](#4-implementation-plan)
5. [Detailed Technical Specifications](#5-detailed-technical-specifications)
6. [File & Folder Structure](#6-file--folder-structure)
7. [Implementation Steps](#7-implementation-steps)
8. [Risk Assessment & Mitigations](#8-risk-assessment--mitigations)

---

## 1. Project Overview

### App Concept
A headless Volumetric app (no desktop UI window) that:
1. Launches and displays a **wireframe cube** matching the volumetric container boundaries.
2. When the user enables **interactive mode** on the volume, the app detects whether the user's hands are inside the wireframe.
3. If the user **pinches** (index finger tip + thumb tip) and releases inside the wireframe, a **random shape** of a **random color** spawns with a **smooth scale-up animation**.
4. Above each spawned shape, a **floating text label** appears on a procedurally generated rounded-rectangle plane with a texture showing the shape name and color (e.g., "Red Cube").
5. If the user **pokes** a spawned shape with either index finger, the shape and its label **smooth-scale down** quickly and are destroyed.

### Target Platform
- Windows desktop (headless console app, `WinExe` output type for no console window)
- Wirelessly connected Meta Quest 3 headset via Volumetric SDK
- Desktop testing via a keyboard-shortcut simulation mode (see §5.10)

---

## 2. SDK Research Findings

### 2.1 Core Architecture (C# Library)

The SDK follows an **event-driven** pattern with these core classes:

| Class | Purpose |
|-------|---------|
| `VolumetricApp` | Session entry point. Created with extension strings. Provides `OnStart`, `OnStop`, etc. |
| `Volume` | A spatial 3D container. Has `OnReady`, `OnUpdate`, `OnClose` lifecycle events. |
| `VolumeContent` | Sets the content root transform, size, and size behavior. |
| `VolumeContainer` | System integration — display name, rotation locks, interactive mode. |
| `Element` | Abstract base for all visual/resource elements. Has async state tracking. |
| `VisualElement` | A renderable 3D node. Set position, orientation, scale, visibility, resource. |
| `ModelResource` | Loads glTF 2.0 models asynchronously by URI. |
| `MaterialResource` | References a named material in a glTF model. Exposes PBR properties. |
| `TextureResource` | Loads images by URI for use as material textures. |
| `MeshResource` | Provides writeable mesh buffers for procedural geometry modification. |
| `HandTracker` | Provides 26 joints per hand (left=index 0, right=index 1). |
| `SpaceLocator` | Locates spatial reference frames (container, content, viewer, local). |

### 2.2 Extension Strings Required

Based on the features we need, the app must enable these extensions:

| Extension | Purpose |
|-----------|---------|
| `VA_EXT_gltf2_model_resource` | Load glTF models for shapes and wireframe |
| `VA_EXT_material_resource` | Dynamically set colors on shapes |
| `VA_EXT_texture_resource` | Apply generated label textures to planes |
| `VA_EXT_mesh_edit` | Procedural mesh modification (wireframe, rounded-rect plane) |
| `VA_EXT_locate_joints` | Hand tracking (pinch & poke detection) |
| `VA_EXT_locate_spaces` | Spatial reference frames (for hand-in-volume checks) |
| `VA_EXT_volume_container_modes` | Enable interactive mode |

### 2.3 Lifecycle Pattern (from SDK samples)

```
VolumetricApp created with extensions
    → OnStart fires
        → Create Volume
            → Volume.OnReady fires
                → Create elements (models, visuals, hand tracker, etc.)
                → Set content size, size behavior, interactive mode
                → Request update mode (FullFramerate)
            → Volume.OnUpdate fires every frame
                → Update hand tracker
                → Update space locator
                → Process game logic (pinch/poke detection, animations)
            → Volume.OnClose fires
                → Cleanup, request exit
```

### 2.4 Hand Tracking API

**HandTracker** provides real-time joint positions for both hands:

- `JointLocations[0]` = left hand, `JointLocations[1]` = right hand
- Each hand has 26 joints (enum `VaHandJointExt`)
- Key joints for our app:
  - `VaHandJointExt.VA_HAND_JOINT_THUMB_TIP_EXT` (index 5) — for pinch detection
  - `VaHandJointExt.VA_HAND_JOINT_INDEX_TIP_EXT` (index 10) — for pinch & poke detection
- Each joint has:
  - `Pose(joint).position` — `VaVector3f` with x, y, z
  - `Pose(joint).orientation` — `VaQuaternionf`
  - `Radius(joint)` — float radius in meters
- `IsTracked` — whether the hand is currently being tracked
- Must call `_handTracker.Update()` each frame

**Pinch Detection Strategy** (derived from SDK patterns):
- Calculate distance between thumb tip and index finger tip
- When distance < threshold → "pinching"
- Track state transitions: not-pinching → pinching → released = **pinch event**

**Poke Detection Strategy** (from SpatialPad sample):
- Get index finger tip position (`Pose(10).position`)
- Calculate distance from finger tip to each spawned shape's position
- When distance < shape radius → **poke event**

### 2.5 Interactive Mode

- Enabled via `Container.AllowInteractiveMode(true)`
- When the user selects interactive mode on the headset, hand inputs are delivered to the volume
- The `Container.onInteractiveModeChanged` event fires when interactive mode is toggled
- Hand tracking data is only available when in interactive mode

### 2.6 Model & Material System

**Loading a glTF model:**
```csharp
var model = new ModelResource(volume, VolumetricApp.GetAssetUri("cube.glb"));
var visual = new VisualElement(volume, model);
```

**Changing material color dynamically:**
```csharp
var material = new MaterialResource(model, "materialName");
material.SetBaseColorFactor(sRGBToLinear(new VaColor4f { r = 1, g = 0, b = 0, a = 1 }));
```

**Important:** Colors must be converted from sRGB to linear space before passing to `SetBaseColorFactor`.

### 2.7 Texture & Label System

**TextureResource** loads images by URI:
```csharp
var texture = new TextureResource(volume);
texture.SetImageUri("path/to/image.png");
material.SetPbrBaseColorTexture(texture);
```

For the floating text labels, we will **generate PNG images at runtime** using `System.Drawing` (GDI+) which is available on Windows without extra dependencies. The generated PNGs are saved to a temp directory and loaded via `TextureResource.SetImageUri()`.

### 2.8 Procedural Mesh System (MeshResource)

**Key finding:** `MeshResource` always requires a `ModelResource` (glTF) as a base — there is no pure "create from nothing" API. However, `WriteMeshBuffers` supports a **resize overload** that can change vertex and index counts:

```csharp
// The resize overload allows completely replacing geometry:
_meshResource.WriteMeshBuffers(
    bufferTypes,
    (uint)newIndexCount,     // Can be different from original
    (uint)newVertexCount,    // Can be different from original
    (meshBuffers) => {
        Marshal.Copy(indices, 0, meshBuffers[0].Buffer, ...);
        Marshal.Copy(positions, 0, meshBuffers[1].Buffer, ...);
        Marshal.Copy(normals, 0, meshBuffers[2].Buffer, ...);
    });
```

**Strategy:** Use a single minimal "template" glTF file (`template.glb`) containing one triangle with a single material. At runtime, create a `ModelResource` from this template, then use `MeshResource.WriteMeshBuffers` with the resize overload to replace the geometry entirely with procedurally generated vertices/indices for cubes, spheres, cylinders, cones, pyramids, wireframes, and label planes.

This approach is validated by the **VolumetricMusicPlayer** sample which uses this exact pattern to create a visualization grid from a simple base mesh.

### 2.9 Available Assets

The SDK ships with useful assets in `assets/`:
- `BoxTextured.glb` — textured cube (can be used as a template for mesh editing)
- `cube-blue.glb`, `cube-red.glb` — colored cubes
- `Duck.glb` — test model

### 2.10 Coordinate System & Sizing

- `VolumeContent.SetSize(VaExtent3Df)` — sets the content region size in meters
- `VolumeContent.SetSizeBehavior(VaVolumeSizeBehavior.Fixed)` — prevents auto-sizing
- `VolumeContent.ActualScale` — the actual scale factor applied by the system
- All positions are in the volume's content coordinate space
- The content region is centered at the origin; a 1m cube spans from -0.5 to +0.5 on each axis

---

## 3. Architecture Design

### 3.1 Class Diagram

```
Program (entry point)
  └── ShapeSpawnerVolume : Volume
        ├── WireframeManager        — Manages wireframe cube visualization
        ├── HandInteractionManager  — Hand tracking, pinch/poke detection
        ├── ShapeManager            — Spawns/destroys shapes with animations
        │     └── SpawnedShape      — Individual shape instance (visual + label + state)
        ├── LabelManager            — Creates floating text labels
        └── ColorHelper             — sRGB ↔ linear conversion, random color generation
```

### 3.2 State Machine for Pinch Detection

```
                    ┌──────────────┐
                    │   IDLE       │
                    │ (not pinching)│
                    └──────┬───────┘
                           │ thumb-index distance < PINCH_START_THRESHOLD
                           ▼
                    ┌──────────────┐
                    │  PINCHING    │
                    │ (fingers close)│
                    └──────┬───────┘
                           │ thumb-index distance > PINCH_RELEASE_THRESHOLD
                           ▼
                    ┌──────────────┐
                    │  RELEASED    │──→ SPAWN SHAPE EVENT
                    │ (one frame)  │
                    └──────┬───────┘
                           │ (auto-transition)
                           ▼
                    ┌──────────────┐
                    │   IDLE       │
                    └──────────────┘
```

Two thresholds (start < release) prevent jitter/flickering.

### 3.3 Shape Lifecycle

```
PINCH RELEASED
    → Create ModelResource from template.glb
    → Create MeshResource → WriteMeshBuffers with procedural shape data (resize overload)
    → Create VisualElement (hidden, scale=0)
    → Create MaterialResource → set random color (sRGB→linear)
    → Create label: ModelResource from template.glb
    → Create label MeshResource → WriteMeshBuffers with rounded-rect plane data
    → Generate label texture at runtime (System.Drawing → PNG → temp file)
    → Apply texture via TextureResource.SetImageUri()
    → Begin SCALE_UP animation (0 → 1 over 0.3s, ease-out)
    → Shape is ALIVE

POKE DETECTED on shape
    → Begin SCALE_DOWN animation (1 → 0 over 0.15s, ease-in)
    → Shape is DYING

SCALE_DOWN complete
    → Destroy all elements (visual, model, mesh, material, texture, label)
    → Remove from active shapes list
```

---

## 4. Implementation Plan

### 4.1 Minimal Template glTF Asset

Since `MeshResource` requires a `ModelResource` (glTF base), we need **one** minimal template file:

| Asset | Description | Material Name |
|-------|-------------|---------------|
| `assets/shapes/template.glb` | Single-triangle mesh with one PBR material | `mat` |

This template will be generated once by a Python script (`tools/generate_template.py` using `trimesh`) — it's a single triangle with positions, normals, UVs, and a material named `mat`. Every procedural mesh (wireframe, shapes, label planes) will use this as a base, then completely replace the geometry via `WriteMeshBuffers`.

### 4.2 Procedural Mesh Generation (All at Runtime)

All geometry is generated procedurally in C# at runtime. A static `ProceduralMeshes` class provides vertex/index data for each shape:

| Mesh | Vertices | Indices | Description |
|------|----------|---------|-------------|
| Wireframe cube | 8 corners × 4 verts per edge × 12 edges = ~96 | ~144 | Thin rectangular prisms for each edge |
| Cube | 24 (4 per face × 6 faces) | 36 | Standard unit cube with normals |
| Sphere | ~240 (UV sphere, 16 slices × 14 stacks) | ~1344 | Smooth sphere |
| Cylinder | ~128 (16 sides, top/bottom caps) | ~360 | Capped cylinder |
| Cone | ~80 (16 sides, base cap) | ~192 | Capped cone |
| Pyramid | 16 (4 base + 4×3 sides) | 18 | 4-sided pyramid |
| Rounded-rect label plane | ~40 | ~60 | Flat quad with rounded corners and UVs |

Each generator returns:
```csharp
public static (float[] positions, float[] normals, float[] texcoords, uint[] indices)
    GenerateCube()  // (and Sphere, Cylinder, Cone, Pyramid, WireframeCube, LabelPlane)
```

At spawn time, a `ModelResource` is created from `template.glb`, a `MeshResource` is created on it, and `WriteMeshBuffers` with the resize overload injects the procedural data.

### 4.3 Runtime Label Texture Generation

Label textures are generated **at runtime** using `System.Drawing` (GDI+, available on Windows natively):

```csharp
public static string GenerateLabelTexture(string colorName, string shapeName)
{
    int width = 512, height = 128;
    using var bmp = new Bitmap(width, height);
    using var gfx = Graphics.FromImage(bmp);

    gfx.SmoothingMode = SmoothingMode.AntiAlias;
    gfx.TextRenderingHint = TextRenderingHint.AntiAlias;

    // Draw rounded rectangle background (semi-transparent dark)
    using var bgBrush = new SolidBrush(Color.FromArgb(200, 30, 30, 30));
    DrawRoundedRect(gfx, bgBrush, 0, 0, width, height, 20);

    // Draw text centered
    string text = $"{colorName} {shapeName}";
    using var font = new Font("Segoe UI", 36, FontStyle.Bold);
    using var textBrush = new SolidBrush(Color.White);
    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
    gfx.DrawString(text, font, textBrush, new RectangleF(0, 0, width, height), sf);

    // Save to temp file and return URI
    string path = Path.Combine(Path.GetTempPath(), "ShapeSpawner", $"{colorName}_{shapeName}.png");
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    bmp.Save(path, ImageFormat.Png);
    return new Uri(path).AbsoluteUri;
}
```

Textures are generated on-demand when a shape spawns (cached after first generation so each color+shape combo is only rendered once).

### 4.4 Configuration Constants

```csharp
// Volume
const float VOLUME_SIZE = 0.4f;            // 0.4m content cube

// Pinch detection
const float PINCH_START_THRESHOLD = 0.02f;  // 2cm — fingers considered pinching
const float PINCH_RELEASE_THRESHOLD = 0.04f; // 4cm — fingers considered released
const float PINCH_COOLDOWN = 0.5f;          // Seconds between spawns

// Poke detection
const float POKE_THRESHOLD = 0.03f;         // 3cm — finger touching shape

// Shape sizing
const float SHAPE_SIZE = 0.04f;             // 4cm spawned shapes
const float LABEL_OFFSET_Y = 0.05f;         // 5cm above shape center
const float LABEL_SIZE = 0.06f;             // 6cm wide label plane

// Animations
const float SCALE_UP_DURATION = 0.3f;       // Seconds
const float SCALE_DOWN_DURATION = 0.15f;    // Seconds
```

---

## 5. Detailed Technical Specifications

### 5.1 Wireframe Cube (Procedural)

The wireframe cube is generated entirely at runtime using the procedural mesh system. Each of the 12 edges is a thin rectangular prism (4 triangles per edge = 2 quads).

**Wireframe edge generation approach:**
```csharp
// For each edge of a unit cube, generate a thin rectangular prism
const float WIRE_THICKNESS = 0.003f; // 3mm thick edges

// 8 cube corners at ±0.5
static readonly VaVector3f[] CubeCorners = {
    (-0.5f, -0.5f, -0.5f), ( 0.5f, -0.5f, -0.5f),
    ( 0.5f,  0.5f, -0.5f), (-0.5f,  0.5f, -0.5f),
    (-0.5f, -0.5f,  0.5f), ( 0.5f, -0.5f,  0.5f),
    ( 0.5f,  0.5f,  0.5f), (-0.5f,  0.5f,  0.5f),
};

// 12 edges (pairs of corner indices)
static readonly (int, int)[] CubeEdges = {
    (0,1),(1,2),(2,3),(3,0), // front face
    (4,5),(5,6),(6,7),(7,4), // back face
    (0,4),(1,5),(2,6),(3,7), // connecting edges
};

// Per edge: generate 8 vertices (rectangular prism) and 12 indices (4 triangulated quads)
// Total: 12 edges × 8 verts = 96 vertices, 12 edges × 12 indices = 144 indices
```

**Integration:**
1. Load `template.glb` as `ModelResource`
2. Create `MeshResource` with Position + Normal descriptors
3. `WriteMeshBuffers(buffers, 144, 96, ...)` to inject wireframe geometry
4. Create `MaterialResource` → set to light cyan, semi-transparent (`a=0.6`)
5. Scale the `VisualElement` to match `VolumeContent.Size`

### 5.2 Hand-in-Volume Detection

To check if hands are inside the wireframe:

```csharp
bool IsInsideVolume(VaVector3f position, VaExtent3Df volumeSize)
{
    float halfW = volumeSize.width / 2f;
    float halfH = volumeSize.height / 2f;
    float halfD = volumeSize.depth / 2f;
    return position.x >= -halfW && position.x <= halfW
        && position.y >= -halfH && position.y <= halfH
        && position.z >= -halfD && position.z <= halfD;
}
```

Hand positions from `HandTracker` are already in volume content space, so this is a simple AABB check.

### 5.3 Pinch Detection Algorithm

```csharp
enum PinchState { Idle, Pinching }

struct HandPinchState
{
    public PinchState State;
    public float LastSpawnTime;
}

// Per-frame per-hand:
void UpdatePinch(ref HandPinchState state, JointLocations hand, float currentTime)
{
    if (!hand.IsTracked) { state.State = PinchState.Idle; return; }

    var thumbTip = hand.Pose(VaHandJointExt.VA_HAND_JOINT_THUMB_TIP_EXT).position;
    var indexTip = hand.Pose(VaHandJointExt.VA_HAND_JOINT_INDEX_TIP_EXT).position;

    float distance = Distance(thumbTip, indexTip);

    switch (state.State)
    {
        case PinchState.Idle:
            if (distance < PINCH_START_THRESHOLD)
                state.State = PinchState.Pinching;
            break;

        case PinchState.Pinching:
            if (distance > PINCH_RELEASE_THRESHOLD)
            {
                state.State = PinchState.Idle;
                if (currentTime - state.LastSpawnTime > PINCH_COOLDOWN)
                {
                    // Midpoint between thumb and index = spawn position
                    var spawnPos = Midpoint(thumbTip, indexTip);
                    if (IsInsideVolume(spawnPos))
                    {
                        SpawnShape(spawnPos);
                        state.LastSpawnTime = currentTime;
                    }
                }
            }
            break;
    }
}
```

### 5.4 Poke Detection Algorithm

```csharp
void CheckPokes(JointLocations leftHand, JointLocations rightHand)
{
    VaVector3f[] fingerTips = GetTrackedIndexTips(leftHand, rightHand);

    foreach (var shape in _activeShapes.ToList())
    {
        if (shape.State != ShapeState.Alive) continue;

        foreach (var tip in fingerTips)
        {
            float dist = Distance(tip, shape.Position);
            if (dist < POKE_THRESHOLD + SHAPE_SIZE / 2f)
            {
                shape.BeginDestroy(); // Start scale-down
                break;
            }
        }
    }
}
```

### 5.5 Smooth Scale Animation

Using an easing function for natural feel:

```csharp
// Ease-out cubic for spawn (fast start, slow end)
float EaseOutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);

// Ease-in cubic for destroy (slow start, fast end)
float EaseInCubic(float t) => t * t * t;

void UpdateAnimations(float deltaTime)
{
    foreach (var shape in _activeShapes)
    {
        switch (shape.State)
        {
            case ShapeState.ScalingUp:
                shape.AnimProgress += deltaTime / SCALE_UP_DURATION;
                if (shape.AnimProgress >= 1f)
                {
                    shape.AnimProgress = 1f;
                    shape.State = ShapeState.Alive;
                }
                float scaleUp = EaseOutCubic(shape.AnimProgress) * SHAPE_SIZE;
                shape.Visual.SetScale(scaleUp);
                shape.LabelVisual.SetScale(scaleUp);
                break;

            case ShapeState.ScalingDown:
                shape.AnimProgress += deltaTime / SCALE_DOWN_DURATION;
                if (shape.AnimProgress >= 1f)
                {
                    DestroyShape(shape); // Remove from list, destroy elements
                }
                else
                {
                    float scaleDown = (1f - EaseInCubic(shape.AnimProgress)) * SHAPE_SIZE;
                    shape.Visual.SetScale(scaleDown);
                    shape.LabelVisual.SetScale(scaleDown);
                }
                break;
        }
    }
}
```

### 5.6 Random Color & Shape Selection

```csharp
static readonly (string Name, VaColor4f Color)[] Colors = new[]
{
    ("Red",    new VaColor4f { r = 1.0f, g = 0.1f, b = 0.1f, a = 1.0f }),
    ("Blue",   new VaColor4f { r = 0.1f, g = 0.3f, b = 1.0f, a = 1.0f }),
    ("Green",  new VaColor4f { r = 0.1f, g = 0.8f, b = 0.2f, a = 1.0f }),
    ("Yellow", new VaColor4f { r = 1.0f, g = 0.9f, b = 0.1f, a = 1.0f }),
    ("Purple", new VaColor4f { r = 0.6f, g = 0.1f, b = 0.9f, a = 1.0f }),
    ("Orange", new VaColor4f { r = 1.0f, g = 0.5f, b = 0.0f, a = 1.0f }),
    ("Cyan",   new VaColor4f { r = 0.0f, g = 0.9f, b = 0.9f, a = 1.0f }),
    ("Pink",   new VaColor4f { r = 1.0f, g = 0.4f, b = 0.7f, a = 1.0f }),
    ("White",  new VaColor4f { r = 1.0f, g = 1.0f, b = 1.0f, a = 1.0f }),
};

static readonly string[] ShapeNames = { "Cube", "Sphere", "Cylinder", "Cone", "Pyramid" };
```

### 5.7 Floating Label Implementation (Runtime Text Rendering)

Each spawned shape gets a floating label above it, generated entirely at runtime:

1. **Label Plane Mesh:** Procedurally generated rounded-rectangle plane using the template glTF + `MeshResource`. Vertices include UV coordinates for texture mapping.
2. **Label Texture:** Generated at runtime via `System.Drawing` — a PNG with a dark semi-transparent rounded-rect background and white bold text (e.g., "Red Cube"). Cached to temp directory after first generation.
3. **At spawn time:**
   ```csharp
   // Generate or retrieve cached label texture
   string textureUri = LabelTextureCache.GetOrCreate(colorName, shapeName);

   // Create label plane from template
   var labelModel = new ModelResource(volume, templateUri);
   var labelVisual = new VisualElement(volume, labelModel);
   labelVisual.SetPosition(shapePos + new VaVector3f { y = LABEL_OFFSET_Y });
   labelVisual.SetScale(0); // Start hidden for animation

   // Inject rounded-rect plane mesh procedurally
   var labelMesh = new MeshResource(labelModel, 0, 0, descriptors, false, false);
   labelMesh.WriteMeshBuffers(buffers, indexCount, vertexCount, (data) => {
       // Write rounded-rect plane with UVs
       Marshal.Copy(planePositions, 0, data[0].Buffer, ...);
       Marshal.Copy(planeNormals, 0, data[1].Buffer, ...);
       Marshal.Copy(planeUVs, 0, data[2].Buffer, ...);
   });

   // Apply runtime-generated texture
   var labelMaterial = new MaterialResource(labelModel, "mat");
   var labelTexture = new TextureResource(volume);
   labelTexture.SetImageUri(textureUri);
   labelMaterial.SetPbrBaseColorTexture(labelTexture);
   ```
4. The label plane will be oriented to face the viewer using a billboard effect (update orientation each frame based on `SpaceLocator.Locations.viewer`).

**Label Texture Cache:** A `Dictionary<(string color, string shape), string uri>` ensures each combo is only rendered once. The cache is populated lazily on first spawn of each combo.

### 5.8 Billboard Effect for Labels

To make labels always face the viewer:

```csharp
void UpdateLabelOrientations()
{
    if (_locator?.IsReady != true) return;
    _locator.Update();

    var viewerPos = _locator.Locations.viewer.pose.position;

    foreach (var shape in _activeShapes)
    {
        if (shape.State == ShapeState.ScalingDown) continue;

        var labelPos = shape.LabelPosition;
        // Calculate direction from label to viewer
        float dx = viewerPos.x - labelPos.x;
        float dz = viewerPos.z - labelPos.z;
        float angle = MathF.Atan2(dx, dz);

        // Y-axis rotation quaternion
        var q = new VaQuaternionf
        {
            x = 0,
            y = MathF.Sin(angle / 2),
            z = 0,
            w = MathF.Cos(angle / 2)
        };
        shape.LabelVisual.SetOrientation(in q);
    }
}
```

### 5.9 Procedural Shape Mesh Generators

All shapes are generated by a static `ProceduralMeshes` class. Each method returns `(float[] positions, float[] normals, float[] texcoords, uint[] indices)`. Shapes are unit-sized (fit within a 1m bounding box) and centered at origin.

| Shape | Algorithm |
|-------|-----------|
| **Cube** | 6 faces × 4 vertices with face normals. Standard box geometry. |
| **Sphere** | UV sphere: iterate latitude (stacks) × longitude (slices), compute positions via spherical coordinates, normals = normalized positions. |
| **Cylinder** | Side: ring of quads along height. Top/bottom: triangle fan caps. |
| **Cone** | Side: triangle fan from apex to base ring. Bottom: triangle fan cap. |
| **Pyramid** | 4 triangular faces from apex to square base + 2 base triangles. |
| **Wireframe** | 12 thin rectangular prisms (see §5.1). |
| **Label Plane** | Rounded rectangle with configurable corner radius, UVs mapped 0-1 for texture. |

### 5.10 Desktop Testing Mode

Since the app is headless and normally requires a Quest headset for hand tracking, a **desktop simulation mode** is provided for rapid iteration:

**Launch with `--desktop-test` command-line flag:**
```
CsShapeSpawner.exe --desktop-test
```

**What it does:**
- Opens a small console window that prints status info and accepts keyboard input
- Simulates hand interactions via keyboard shortcuts:

| Key | Action |
|-----|--------|
| `Space` | Simulate a pinch-and-release at the center of the volume → spawns a random shape |
| `1`–`9` | Poke (destroy) shape #1 through #9 from the active list |
| `D` | Dump current state (active shapes, positions, etc.) to console |
| `Q` | Request exit |

**Implementation:**
- When `--desktop-test` is detected, the `OutputType` is `Exe` (console) instead of `WinExe`
- A background thread reads `Console.ReadKey()` and dispatches actions to the volume's update loop via `Volume.DispatchToNextUpdate(Action)`
- The simulated pinch spawns shapes at random positions within the volume bounds
- The volume still launches normally and appears on a connected headset if available — keyboard just provides an additional input method

```csharp
if (args.Contains("--desktop-test"))
{
    Console.WriteLine("Desktop test mode active. Press Space to spawn, 1-9 to poke, Q to quit.");
    Task.Run(() =>
    {
        while (!app.IsStopped)
        {
            var key = Console.ReadKey(true);
            volume?.DispatchToNextUpdate(() =>
            {
                switch (key.Key)
                {
                    case ConsoleKey.Spacebar:
                        shapeManager.SpawnAtRandom();
                        break;
                    case ConsoleKey.D:
                        shapeManager.DumpState();
                        break;
                    case ConsoleKey.Q:
                        app.RequestExit();
                        break;
                    default:
                        if (key.KeyChar >= '1' && key.KeyChar <= '9')
                            shapeManager.PokeShape(key.KeyChar - '1');
                        break;
                }
            });
        }
    });
}
```

---

## 6. File & Folder Structure

```
cs/Samples/ShapeSpawner/
├── CsShapeSpawner.csproj          # Project file
├── Program.cs                      # Entry point (handles --desktop-test flag)
├── ShapeSpawnerVolume.cs           # Main Volume subclass
├── HandInteractionManager.cs       # Pinch & poke detection
├── ShapeManager.cs                 # Shape spawning, animations, destruction
├── SpawnedShape.cs                 # Individual shape data class
├── ProceduralMeshes.cs             # Static generators for all shape meshes
├── WireframeManager.cs             # Wireframe cube (procedural mesh)
├── LabelManager.cs                 # Label plane creation, texture gen, billboard
├── LabelTextureCache.cs            # Runtime PNG generation + caching
├── ColorHelper.cs                  # Color utilities (sRGB↔linear, random)
├── DesktopTestMode.cs              # Keyboard simulation for desktop testing
└── Constants.cs                    # All tunable constants

assets/shapes/
└── template.glb                    # Minimal single-triangle glTF template

tools/
└── generate_template.py            # Script to generate the template GLB
```

---

## 7. Implementation Steps

### Phase 1: Project Scaffolding
1. Create `cs/Samples/ShapeSpawner/` directory structure
2. Create `CsShapeSpawner.csproj` referencing the library (with `AllowUnsafeBlocks` for `Marshal.Copy`)
3. Create `Program.cs` with basic `VolumetricApp` setup and all required extensions
4. Create `ShapeSpawnerVolume.cs` extending `Volume` with lifecycle hooks
5. Create `Constants.cs` with all tunable values
6. Verify the project builds and runs (empty volume)

### Phase 2: Template Asset & Procedural Mesh System
7. Write `tools/generate_template.py` using `trimesh` to create `assets/shapes/template.glb` (single triangle + "mat" material)
8. Create `ProceduralMeshes.cs` with static generators for all shapes:
   - `GenerateCube()`, `GenerateSphere(slices, stacks)`, `GenerateCylinder(sides)`
   - `GenerateCone(sides)`, `GeneratePyramid()`
   - `GenerateWireframeCube(thickness)`, `GenerateLabelPlane(width, height, cornerRadius)`
9. Unit test each mesh generator (verify vertex/index counts, winding order)

### Phase 3: Wireframe Cube (Procedural)
10. Create `WireframeManager.cs` — loads template.glb, creates MeshResource, injects wireframe geometry via WriteMeshBuffers
11. Set material to semi-transparent cyan
12. Integrate into `ShapeSpawnerVolume.OnReady`
13. Verify wireframe renders at volume boundaries

### Phase 4: Desktop Testing Mode
14. Create `DesktopTestMode.cs` with keyboard input loop on background thread
15. Wire up `--desktop-test` flag in `Program.cs` (switch to Exe output, start input thread)
16. Implement Space=spawn, 1-9=poke, D=dump, Q=quit
17. Use `Volume.DispatchToNextUpdate()` to marshal keyboard actions to the update loop
18. Test basic lifecycle: spawn and destroy shapes via keyboard

### Phase 5: Hand Tracking & Pinch Detection
19. Create `HandInteractionManager.cs` with `HandTracker`, `SpaceLocator`
20. Implement pinch state machine (Idle → Pinching → Released) with dual thresholds
21. Implement hand-in-volume AABB check
22. Fire `OnPinchReleased(VaVector3f position)` event when pinch detected inside the volume
23. Integrate into `ShapeSpawnerVolume.OnUpdate`

### Phase 6: Shape Spawning & Materials
24. Create `ColorHelper.cs` with sRGB↔linear conversion and random color/shape selection
25. Create `SpawnedShape.cs` data class (state, visual elements, animation progress)
26. Create `ShapeManager.cs` — on pinch event: load template, create MeshResource, inject procedural shape, set random color via MaterialResource
27. Implement scale-up animation in update loop

### Phase 7: Runtime Label Textures & Floating Labels
28. Create `LabelTextureCache.cs` — generates PNG labels at runtime via `System.Drawing`, caches to temp dir
29. Create `LabelManager.cs` — creates label plane (template + procedural rounded-rect mesh), applies cached texture
30. Implement billboard orientation update each frame
31. Integrate labels into `SpawnedShape` — label spawns with shape, animates together

### Phase 8: Poke Detection & Shape Destruction
32. Add poke detection to `HandInteractionManager` — check index finger tip distance to each shape
33. Implement scale-down animation and element cleanup in `ShapeManager`
34. Handle element destruction (`Destroy()` calls on model, visual, material, texture, mesh, label elements)

### Phase 9: Polish & Testing
35. Tune constants (thresholds, animation speeds, sizes) using desktop test mode
36. Add cooldown between spawns to prevent rapid-fire
37. Add max active shapes limit (e.g., 20) to prevent performance issues
38. Test with Meta Quest 3 headset via desktop test mode first, then live hand tracking
39. Handle edge cases (hand tracking loss, interactive mode toggle, volume close)
40. Clean up temp label textures on app exit

---

## 8. Risk Assessment & Mitigations

### 8.1 Runtime Texture Generation
**Risk:** `System.Drawing` may produce PNGs that the SDK's `TextureResource` can't load, or temp file paths may not work as URIs.
**Mitigation:** Save PNGs to a well-known temp directory, convert to `file:///` URIs via `new Uri(path).AbsoluteUri`. Test with a simple single-texture app first. Fallback: use `SkiaSharp` as an alternative renderer if GDI+ has issues.

### 8.2 Procedural Mesh via Template GLB
**Risk:** The `WriteMeshBuffers` resize overload may not work correctly with a minimal template, or the template may need specific buffer configurations.
**Mitigation:** The VolumetricMusicPlayer sample proves this pattern works (resize from small mesh to large grid). Generate the template with `trimesh` which produces well-formed glb. Test each shape generator individually using desktop test mode. Fallback: use `BoxTextured.glb` from assets as the template instead.

### 8.3 Performance with Many Shapes
**Risk:** Each spawned shape creates ~5 elements (model, visual, material, texture, label model, label visual, label material, label texture). Many simultaneous shapes could degrade performance.
**Mitigation:** Cap active shapes at 20. Destroy elements promptly on scale-down completion. Use shared `ModelResource` instances where possible (one model per shape type, multiple visuals reference it).

### 8.4 Hand Tracking Availability
**Risk:** Hand tracking data may be unavailable if the user hasn't entered interactive mode, or if controllers are being used instead.
**Mitigation:** Check `hand.IsTracked` and `hand.DataSource` before processing. The `JointLocations.HasDataSource` property indicates whether hand device is available. Gracefully handle missing data — the wireframe still displays, shapes are just not spawned.

### 8.5 Billboard Label Orientation
**Risk:** The `SpaceLocator.Locations.viewer` position may not update reliably, causing labels to not face the viewer.
**Mitigation:** Check `viewer.isTracked` before using the position. Fallback: orient labels toward the volume container center if viewer position is unavailable.

### 8.6 sRGB vs Linear Color Space
**Risk:** Setting colors in sRGB space directly will produce washed-out or incorrect colors.
**Mitigation:** Always convert through `sRGBToLinear()` before calling `SetBaseColorFactor()`. This is a proven pattern from the MaterialExplorer and SpatialPad samples.

### 8.7 Shared ModelResource Optimization
**Risk:** Creating a new `ModelResource` per spawned shape is wasteful since many shapes share the same glb.
**Mitigation:** Since all shapes use the same `template.glb`, we create one `ModelResource` per shape instance but the file only loads once (SDK caches by URI). Each spawn needs its own `ModelResource` → `MeshResource` pair because the mesh data differs per shape type. However, we could potentially pre-create a pool of ready-to-use model+mesh combos during `OnReady` for the most common shapes.

### 8.8 Desktop Test Mode Thread Safety
**Risk:** Keyboard input arrives on a background thread while the Volumetric SDK is single-threaded.
**Mitigation:** All keyboard actions are dispatched via `Volume.DispatchToNextUpdate(Action)` which queues them for the next update cycle on the correct thread. Never mutate SDK state directly from the input thread.

---

*Document generated for the AI-Showcase-Volumetric-Experiment project.*
*Based on analysis of microsoft/volumetric-sdk-staging C# library and all sample applications.*
