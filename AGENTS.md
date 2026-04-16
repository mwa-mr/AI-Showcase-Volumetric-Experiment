# Volumetric SDK

The Volumetric SDK lets Windows apps add Volumes — spatial 3D containers that display and interact with 3D content on wirelessly connected headsets (e.g. Meta Quest 3). Language bindings in C++, C#, and Python all build on a common C ABI layer (`cpp/include/VaAbi/`).

## Directory Layout

| Directory | Purpose |
|-----------|---------|
| `assets/` | Shared glTF assets for samples |
| `cpp/include/VaAbi/` | C ABI layer |
| `cpp/include/VaUtility/` | Utility helpers |
| `cpp/include/VolumetricCppLibrary/` | C++ wrapper classes |
| `cpp/samples/` | C++ samples |
| `cs/Library/` | C# library source |
| `cs/Samples/` | C# console samples |
| `cs/UnitySamples/` | Unity integration samples |
| `python/package/` | Python package source |
| `python/samples/` | Python samples |

## Object Model

```
VolumetricApp (Session)
  └── Volume (3D container)
        ├── VolumeContainer (system integration)
        ├── VolumeContent (content region)
        └── Elements
              ├── VisualElement (renderable node)
              ├── ModelResource (glTF loader)
              ├── MaterialResource (PBR properties)
              └── MeshResource (procedural mesh)
```

## Key Patterns

- **Event-driven**: All SDKs use lifecycle events (`onStart`, `onReady`, `onUpdate`, `onClose`)
- **Extensions**: Features enabled via extension strings at session creation
- **Async loading**: Resources load asynchronously; check state before use

## Code Conventions

| Language | Naming Style | Entry Point |
|----------|--------------|-------------|
| C# | `PascalCase` | `new VolumetricApp()` |
| Python | `snake_case` | `va.VolumetricApp()` |

C/C++ conventions are in the instruction file loaded automatically for C++ files.

## Dependencies

- SDK libraries **must be self-contained** — no internal library dependencies; only language standard libraries and OS system libraries
