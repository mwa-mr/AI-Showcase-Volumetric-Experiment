---
type: spec
name: Python SDK
description: Python package with Pythonic API patterns
scope: SDK/python/
status: active
owner: 
created: 2026-02-05
last_updated: 2026-02-10
depends_on:
  - @file:../SPEC.md
---

# Python SDK Specification

The Python SDK provides a Pythonic wrapper over the C ABI, using snake_case naming, context managers, and Python-native
patterns for extending Windows apps into Mixed Reality.

## Overview

- **Package**: `volumetric` (import as `va`)
- **Module**: `import volumetric as va`
- **Python Version**: 3.11+
- **Type**: Self-contained module with C extension
- **Runtime**: Dynamically loads `VolumetricRuntime.dll` from platform package

## Prerequisites

- Windows 11
- Python 3.11 or later
- Visual Studio Code (recommended)
- Mixed Reality Link + Immersive Addon

## Files

| File/Folder           | Purpose                    |
|-----------------------|----------------------------|
| `package/volumetric/` | Main package source        |
| `package/.../api.py`  | Public API (generated)     |
| `package/setup.py`    | Package installation       |
| `samples/`            | Python sample applications |

## Key Classes

| Class              | Description                                        |
|--------------------|----------------------------------------------------|
| `VolumetricApp`    | Application entry point; manages session lifecycle |
| `Volume`           | 3D spatial container for content in Mixed Reality  |
| `VolumeContainer`  | System UI integration (display name, thumbnail)    |
| `VolumeContent`    | Content region size and placement configuration    |
| `VisualElement`    | Transformable scene node for rendering             |
| `ModelResource`    | Async glTF 2.0 model loader                        |
| `MeshResource`     | Editable mesh buffers for procedural geometry      |
| `MaterialResource` | PBR material property overrides                    |
| `MeshBufferWriter` | Context manager for mesh buffer editing            |

## Hello World Example

Using class-based Volume (recommended pattern):

```python
import sys
import volumetric as va

class HelloWorld(va.Volume):
    def on_ready(self) -> None:
        uri = self.app.get_local_asset_uri("world.glb")
        model = va.ModelResource(self, uri)
        visual = va.VisualElement(self, model)

    def on_close(self) -> None:
        self.app.request_exit()

if __name__ == '__main__':
    app = va.VolumetricApp("Python Hello World", [va.VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME])
    app.on_start = lambda _: HelloWorld(app)
    sys.exit(app.run())
```

## App Lifecycle

| Event           | Description                                              |
|-----------------|----------------------------------------------------------|
| `on_start`      | First system connection established; create volumes here |
| `on_disconnect` | Lost connection to Mixed Reality system                  |
| `on_reconnect`  | Re-established connection after disconnect               |
| `on_stop`       | App is exiting; cleanup resources                        |

Note: `on_start` and `on_ready` are guaranteed to fire exactly once, even if subscribed late.

## Volume Lifecycle

| Event       | Description                                           |
|-------------|-------------------------------------------------------|
| `on_ready`  | Volume created and ready; add content here            |
| `on_update` | Frame update; modify elements and request next update |
| `on_pause`  | Volume backgrounded (e.g., system UI opened)          |
| `on_resume` | Volume foregrounded after pause                       |
| `on_close`  | Volume closing; release element references            |

## Frame Updates

Request continuous updates for animation:

```python
def on_ready(self) -> None:
    # ... create elements ...
    self.request_update(va.VA_VOLUME_UPDATE_MODE_FULL_FRAMERATE)

def on_update(self) -> None:
    seconds = self.frame_state.frame_time * 1e-9
    angle = seconds * math.pi / 2
    self._visual.set_orientation(0, math.sin(angle/2), 0, math.cos(angle/2))
```

For static content, skip `request_update()` to conserve resources.

## Extensions

Enable features via extensions at app creation:

```python
app = va.VolumetricApp("my_app",
    required_extensions=[va.VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME],
    optional_extensions=[va.VA_EXT_MESH_EDIT_EXTENSION_NAME])
```

| Extension                          | Feature                 |
|------------------------------------|-------------------------|
| `VA_EXT_GLTF2_MODEL_RESOURCE_...`  | Load glTF 2.0 models    |
| `VA_EXT_MATERIAL_RESOURCE_...`     | Edit PBR materials      |
| `VA_EXT_MESH_EDIT_...`             | Procedural mesh editing |
| `VA_EXT_TEXTURE_RESOURCE_...`      | Load image textures     |
| `VA_EXT_ADAPTIVE_CARD_ELEMENT_...` | Render Adaptive Cards   |
| `VA_EXT_LOCATE_JOINTS_...`         | Hand joint tracking     |
| `VA_EXT_LOCATE_SPACES_...`         | Space tracking (poses)  |

## API Patterns

### Snake Case Naming

All methods use Python snake_case:

```python
visual.set_position(x, y, z)
visual.set_orientation(qx, qy, qz, qw)
model.get_async_state()
```

### Class-Based Volumes

Subclass `va.Volume` to override lifecycle methods:

```python
class MyVolume(va.Volume):
    def on_ready(self) -> None:
        # Initialize content
        pass
    
    def on_update(self) -> None:
        # Frame update logic
        pass
    
    def on_close(self) -> None:
        self.app.request_exit()
```

### Context Managers

Mesh editing uses context managers:

```python
with mesh.acquire_buffer_writer() as writer:
    writer.set_position(0, [1.0, 2.0, 3.0])
    writer.set_normal(0, [0.0, 1.0, 0.0])
# Buffer automatically released
```

## Build Samples from GitHub Repo

Build with pip:

```bash
cd package
pip install -e .
```
