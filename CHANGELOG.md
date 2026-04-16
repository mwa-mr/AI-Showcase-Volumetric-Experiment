# Changelog

All notable changes to this SDK will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

# [0.3.25] Current Version
- Added support for raising `onRestoreResult` when the caller subscribes to the event after the runtime already raised it.

# [0.3.24] 03/17/2026
- Added `VA_ERROR_CALL_ORDER_INVALID` for ABI call-order violations.
- Added `VA_ELEMENT_ASYNC_ERROR_LIMIT_REACHED` async error indicating a `VA_ELEMENT_TYPE_MESH_RESOURCE_EXT` element could not transition to `VA_ELEMENT_ASYNC_STATE_READY` because the platform limit for `VA_EXT_mesh_edit` resources was reached.
- Added `VA_ELEMENT_ASYNC_ERROR_MESH_BUFFER_DESCRIPTOR_UNSUPPORTED_EXT` async error to `VA_EXT_mesh_edit`, returned when `VaMeshResourceInitBuffersInfoExt` contains an unsupported `VaMeshBufferDescriptorExt` entry.
- Fixed C# `MeshResource.WriteMeshBuffers` returning `false` after a successful write.
- Fixed C++ `MeshResource::WriteMeshBuffers` failing to allocate `VaMeshBufferDataExt` storage before calling the ABI.

# [0.3.23] 02/26/2026
- Added per-hand data source reporting to `VA_EXT_locate_joints` extension via new `VaHandTrackingDataSourceExt` enum (`Unavailable`, `HandTracking`, `ControllerSimulated`) and `dataSource` field on `VaJointLocationsExt`.
  - The new field is appended at the end of the struct for ABI compatibility; apps built against v1 are unaffected.
- Added `local_floor` and `neck` reference space poses to `VA_EXT_locate_spaces` extension.

# [0.3.22]
- Added `vaWaitEvent` ABI function with optional `VaEventWaitInfo` struct to support timed event waits.

# [0.3.21] 02/12/2026
- Breaking change: The `VA_VOLUME_STATE_CREATED` is renamed to `VA_VOLUME_STATE_IDLE` to reflect its usage
  when volumes are paused and later resume to a running state.
    - If the application uses the ABI definition in C headers, you need to rename it accordingly.
    - If the application consumes the C#, C++ or Python library, no change is needed.
- Breaking change: In the C# library, replaced the `VaUuid` type with a custom implementation featuring refined helper functions.
  The `VaUuidUtilities` extension class is removed.
- The C# volumetric library minimum .NET version changed from .netstandard2.0 to .netstandard2.1.
- Python library: Added `VolumetricApp.run_async()` method to run the app event loop in a background thread,
  allowing the main thread to perform other tasks while the volumetric app is running.

# [0.3.20] 01/08/2026
- Disabled C# library print debug string to console by default, and added VaTrace.EnableTraceToConsole property to enable it when needed.

# [0.3.19] 12/04/2025
- Python library, `Volume.request_update_after` is deprecated due to inaccuracy when a small time slice is provided.
- Added `request_update_after_seconds` function to replace above, that takes float value in seconds for the delay.
- All C# samples are upgraded to use net10

# [0.3.18] 11/18/2025
- Upgraded C# Volumetric sample apps that are depending on `WindowsAppSDK` to version 1.8.251003001, for latest security fix.
- Fixed csproj settings for packaged app for proper single project packaging support.
- Fixed issues that sometimes unsupported Gltf extensions might fail the model loading at all.
- Fixed issues that sometimes the Volumes' `onClose` event might not be raised after app request exit.
- Fixed issues that sometimes the Volumetric runtime may crash when the 3D model file path contains unicode characters.

# [0.3.17] 10/3/2025
- Added `VA_SPACE_TYPE_LOCAL_EXT` to `VA_EXT_locate_space` extension to better support Volumetric apps get space tracking for gravity direction.
- Fixed the issue where `VA_SPACE_TYPE_VOLUME_CONTAINER_EXE` tracking is not rotating with user's interface to volume container.
- Fixed some other minor space tracking issues in interactive mode.

# [0.3.16] 8/29/2025
- Breaking change: In Python library, fixed spelling of `VisualElement.set_visibile` to `VisualElement.set_visible`. All Python apps need to be updated.
- Breaking change: In C# library, the `VolumetricApp::Continue` function is renamed to `PollEvents`. There's no behavior change.  All C# apps need to be recompiled.
- Breaking change: In C++ library, the `va::App` class is renamed to `va::VolumetricApp`
and the `va::App::Continue` function is renamed to `va::VolumetricApp::PollEvents`.
There's no behavior change.  All C++ apps need to be recompiled.
- Added `VA_ERROR_VOLUME_DUPLICATE_ID` which is an error that indicates an app has tried to create a second volume using the same volumeRestoreId as a volume that is already created. This is not allowed.
- Breaking change: In ABI, renamed the IDLE volume state from `VA_VOLUME_STATE_IDLE` to `VA_VOLUME_STATE_CREATED`

# [0.3.15] 8/12/2025
- Added `@classmethods` for python `VisualElement` creation. The VisualElement now enforces the rules regarding parameters to its constructor.
  - `create_with_visual_resource`: Accepts only a visual resource as a parameter. Used to attach this visual element to a resource.
  - `create_named_node`: Accepts a visual element and a string name. This allows the element to be associated to a named node in the provided visual element reference.
  - `create_with_parent`: Accepts a visual element and treats that element as the parent of the new visual element.

# [0.3.14] 06/30/2025
- Added new extension `VA_EXT_volume_thumbnail` to enable volume to customize the thumbnail 3D model and thumbnail icon that represents the volume in system UX.
- Added new API for `onPause` and `onResume` events on Volume class to notify the app pausing and resuming of volume updates in response to user opening the volume management UI which hides all volumes.

# [0.3.13] 06/19/2025
- Added `onFatalError` event to libraries to avoid app crashing on unhandled exceptions and enable apps to monitor fatal errors.
- Fixed a bug where `VA_ERROR_EXTENSION_NOT_PRESENT` was not returned when setting `VA_ELEMENT_PROPERTY_GLTF2_MODEL_URI_EXT` property without requesting `VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME` extension.
- Added volume restore feature to the C# library.

# [0.3.12] 06/18/2025
- Add `VA_TYPE_EVENT_VOLUME_RESTORE_ID_INVALIDATED` to `VA_EXT_volume_restore` which is raised an a restore id can no longer be used to restore a volume.

# [0.3.11] 06/09/2025
- Renamed `VA_VOLUME_CONTAINER_MODE_SHARABLE_IN_TEAMS_EXT` to `VA_VOLUME_CONTAINER_MODE_SHAREABLE_IN_TEAMS_EXT` to fix misspelling.

# [0.3.10] 06/06/2025
- Add async state changed handler for all elements in the python library. This allows developers to be notified when the async state changes on an element without direct polling.

# [0.3.09] 05/31/2025
- Added new API for `VA_ELEMENT_TYPE_VISUAL`
  - Support setting a visual parent (P) for a visual element (C) in visual tree through `VA_ELEMENT_PROPERTY_VISUAL_PARENT`. In this setting, C's transform is interpreted in P's local coordinate space.

# [0.3.08] - 05/30/2025
- Breaking change to `VaSessionCreateInfo`. Adds `suppressSystemWaitUx` which let's the app opt-out of runtime behavior that shows a 2d dialog if the ImmersiveAddOn is not running.

# [0.3.07] - 05/27/2025
- Added new API for `VA_EXT_volume_container_modes` extension
  - Added new APIs for volumes to enable "interactive mode", "unbounded mode", "one-to-one mode" and "Teams sharing mode".
  - By default a volume doesn't support above modes.
  - Added new APIs for applications to receive notification when the user enters above volume modes.
- Breaking change to the `VA_EXT_locate_joints` extension
  - In order to receive hand tracking data, the app must enable the "interactive mode" for this volume
    and wait for the user to enter the interactive mode.
- Breaking change: removed `VA_ELEMENT_PROPERTY_VOLUME_CONTENT_CLIP_BEHAVIOR` property
  - The clipping behavior is replaced by the "unbounded mode" that's controled by the user.
  - By default a volume is always clipped to its bound until the user enters the unbounded mode.

# [0.3.06] - 05/23/2025
- Added new `VA_EXT_texture_resource` extension and their usage in `VA_EXT_material_resource` extension.
- Breaking change to `VA_EXT_material_resource` extension.
  - `VA_EXT_MATERIAL_RESOURCE_EXTENSION_NAME` needs to be specified in VaSessionCreateInfo.enabledExtensionNames to call material resource relevant functions.
- Renamed a few MaterialResource function names related to material base color, metallic and roughness factors to prefix with `Pbr` to clarify their usage.

# [0.3.05] - 05/21/2025
- Breaking change to the `VA_EXT_volume_restore` extension.
  - Removed `VA_VOLUME_RESTORE_BEHAVIOR_BY_PLATFORM_INDIVIDUAL_VOLUME_EXT` as a restore option.
  - Removed support for the relaunch token flow.

# [0.3.04] - 05/08/2025
- Breaking change to the `VA_EXT_adaptive_card_element` extension.
  - Removed placement, visibility behavior, and backplate visible properties.
  - Moved `VA_TYPE_ADAPTIVE_CARD_ACTION_INVOKED_DATA_EXT` into the extension structures
- Breaking change to `VA_EXT_gltf2_model_resource` extension.
  - Removed unused structures and functions that supported 0.2 model loading.

# [0.3.03] - 04/29/2025
- Breaking change to the `VA_EXT_locate_spaces` and `VA_EXT_locate_joints` extension.
  - The coordinate system in above spatial tracking APIs are corrected to be consistent to OpenXR standard, that is right hand system with Y up, X right and Z backward.
  - The "VOLUME" reference space is split into two spaces.  The "VOLUME CONTAINER" space is always gravity aligned and its placement is controled by the user, where the "VOLUME CONTENT" space replaces the old "VOLUME" space as the root of all visual elements and its placement can be controlled by the application.
  - Added the APIs for apps to inspect the actual size, position, and scale when the volume is in "SIZE_BEHAVIOR_AUTO_SIZE" mode, so that the app can observe auto layout status.
  - Added hand joint radius data for hand joint tracking input and added "hasDataSource" concept to align with latest OpenXR standard.
  - Removed some unsupported spatial input APIs in previous design.
- Applications that's using above 2 extensions needs to be updated accordingly and recompile.
  - Developers can reference the diff of all samples in this release to understand how to migrate your application to accomondate this breaking change.

# [0.3.02] - 04/28/2025
- Breaking: Added `errorMessage` to the return of `vaGetNextElementAsyncError` function.
  - This allows the application to inspect detail of async errors in addition to error code
  - Also fixed a but that the function didn't report all error codes on error.
  - All apps that sync with latest volumetric library need to recompile.  But there's no need to change app's code.

# [0.3.01] - 04/28/2025
- Simplify the C# library's extension APIs to a collection of const strings.
- Fixed a bug that loading the 2nd GLB file always replace the 1st one.
- Added adaptive card support in the C# volumetric library.
- There are known issues for adaptive card placement and will be fixed in future releases.

# [0.3.00] - 03/31/2025

- The initial draft of the version 0.3 Volumetric SDK.
