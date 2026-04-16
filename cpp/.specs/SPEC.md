---
type: spec
name: C++ SDK
description: Header-only C++ wrapper providing RAII and modern C++ idioms
scope: SDK/cpp/
status: active
owner: 
created: 2026-02-05
last_updated: 2026-02-10
depends_on:
  - @file:../SPEC.md
---

# C++ SDK Specification

The C++ SDK provides a header-only wrapper over the C ABI, offering RAII resource management, modern C++ idioms, and
lambda-based callbacks for extending Windows apps into Mixed Reality.

## Overview

- **Header**: `#include <VolumetricApp.h>`
- **Namespace**: `va::`
- **Standard**: C++17 (C++20 optional features available)
- **Type**: Header-only (no static library needed)
- **Runtime**: Dynamically loads `VolumetricRuntime.dll` from platform package at first API call

## Prerequisites

- Windows 11
- Visual Studio with C++ desktop development
- CMake tools
- Mixed Reality Link + Immersive Addon

## Files

| File/Folder                     | Purpose                   |
|---------------------------------|---------------------------|
| `include/VaAbi/`                | C ABI headers             |
| `include/VaUtility/`            | Utility helpers           |
| `include/VolumetricCppLibrary/` | C++ wrapper classes       |
| `include/.../VolumetricApp.h`   | Single-header entry point |
| `include/.../api/`              | Public API classes        |
| `include/.../detail/`           | Implementation details    |
| `samples/`                      | C++ sample applications   |

## Project Configuration

### CMake

```cmake
add_subdirectory(path/to/include)
target_link_libraries(${TARGET} VolumetricSDK::VolumetricCppLibrary)
```

### MSBuild

Add to `AdditionalIncludeDirectories`:

```xml
<AdditionalIncludeDirectories>
  path\to\include\VolumetricCppLibrary;
  path\to\include\VaUtility;
  path\to\include\VaAbi;
  %(AdditionalIncludeDirectories)
</AdditionalIncludeDirectories>
```

## Key Classes

| Class              | Description                                        |
|--------------------|----------------------------------------------------|
| `VolumetricApp`    | Application entry point; manages session lifecycle |
| `Volume`           | 3D spatial container for content in Mixed Reality  |
| `VisualElement`    | Transformable scene node for rendering             |
| `ModelResource`    | Async glTF 2.0 model loader                        |
| `MeshResource`     | Editable mesh buffers for procedural geometry      |
| `MaterialResource` | PBR material property overrides                    |
| `HandTracker`      | Hand joint tracking (26 joints per hand)           |
| `SpaceLocator`     | Spatial coordinate space tracking                  |

## Hello World Example

```cpp
#include <VolumetricApp.h>

int main() {
    auto app = va::CreateVolumetricApp({"hello_world_cpp",
                                        { VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME }});
    app->onStart = [](va::VolumetricApp& app) {
        auto volume = app.CreateVolume<va::Volume>();
        volume->onReady = [](va::Volume& volume) {
            auto uri = va::windows::GetLocalAssetUri("world.glb");
            auto model = volume.CreateElement<va::ModelResource>(uri);
            volume.CreateElement<va::VisualElement>(*model);
        };
        volume->onClose = [&app](auto&) { app.RequestExit(); };
    };
    return app->Run();
}
```

## App Lifecycle

| Event          | Description                                              |
|----------------|----------------------------------------------------------|
| `onStart`      | First system connection established; create volumes here |
| `onDisconnect` | Lost connection to Mixed Reality system                  |
| `onReconnect`  | Re-established connection after disconnect               |
| `onStop`       | App is exiting; cleanup resources                        |

Note: `onStart` and `onReady` are guaranteed to fire exactly once, even if subscribed late.

## Volume Lifecycle

| Event      | Description                                           |
|------------|-------------------------------------------------------|
| `onReady`  | Volume created and ready; add content here            |
| `onUpdate` | Frame update; modify elements and request next update |
| `onPause`  | Volume backgrounded (e.g., system UI opened)          |
| `onResume` | Volume foregrounded after pause                       |
| `onClose`  | Volume closing; release element references            |

## Frame Updates

Request continuous updates for animation:

```cpp
volume->onReady = [](va::Volume& vol) {
    // ... create elements ...
    vol.RequestUpdate(va::VolumeUpdateMode::FullFramerate);
};

volume->onUpdate = [&](va::Volume& vol) {
    float seconds = vol.GetFrameState().frameTime * 1e-9f;
    float angle = seconds * M_PI / 2;
    va::Quaternion q{0, std::sin(angle/2), 0, std::cos(angle/2)};
    visual->SetOrientation(q);
};
```

For static content, skip `RequestUpdate()` to conserve resources.

## Extensions

Enable features via extensions at app creation:

```cpp
auto app = va::CreateVolumetricApp({
    "my_app",
    { VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME },  // required
    { VA_EXT_MESH_EDIT_EXTENSION_NAME }              // optional
});
```

| Extension                          | Feature                 | Version |
|------------------------------------|-------------------------|---------|
| `VA_EXT_GLTF2_MODEL_RESOURCE_...`  | Load glTF 2.0 models    |         |
| `VA_EXT_MATERIAL_RESOURCE_...`     | Edit PBR materials      |         |
| `VA_EXT_MESH_EDIT_...`             | Procedural mesh editing |         |
| `VA_EXT_TEXTURE_RESOURCE_...`      | Load image textures     |         |
| `VA_EXT_ADAPTIVE_CARD_ELEMENT_...` | Render Adaptive Cards   |         |
| `VA_EXT_LOCATE_JOINTS_...`         | Hand joint tracking     | 2       |
| `VA_EXT_LOCATE_SPACES_...`         | Space tracking (poses)  | 3       |

> **`VA_EXT_locate_joints` v2**: Added `dataSource` field at the end of `VaJointLocationsExt`.
> ABI-compatible — new SDK on an older runtime reads `dataSource` as `0` (`Unavailable`) due to zero-initialization.

## API Patterns

### RAII Resource Management

All resources are automatically cleaned up when they go out of scope:

```cpp
{
    auto model = volume.CreateElement<va::ModelResource>(uri);
    auto visual = volume.CreateElement<va::VisualElement>(*model);
}  // Both destroyed automatically
```

### Lambda Callbacks

Event handlers use `std::function`:

```cpp
volume->onUpdate = [&](va::Volume& vol) {
    rotation += deltaTime * 0.5f;
    visual->SetOrientation(va::Quaternion::FromAxisAngle({0, 1, 0}, rotation));
};
```

### Error Handling

Errors throw `va::VolumetricException`:

```cpp
try {
    auto model = volume.CreateElement<va::ModelResource>("invalid://uri");
} catch (const va::VolumetricException& e) {
    std::cerr << "Error: " << e.what() << " (code: " << e.code() << ")\n";
}
```

## Build Samples from GitHub Repo

Use CMake to build the samples:

```bash
cmake -B build
cmake --build build
```

## Samples

| Sample         | Description                    |
|----------------|--------------------------------|
| `SpinningCube` | Basic rotating cube            |
| `ModelViewer`  | Load and display glTF models   |
| `HandTracking` | Visualize hand joint positions |
