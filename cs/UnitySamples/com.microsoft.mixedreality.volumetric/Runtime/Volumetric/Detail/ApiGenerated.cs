// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable
// This file is generated from spec.xml

namespace Microsoft.MixedReality.Volumetric.Detail
{
    using System;
    using System.Runtime.InteropServices;
    using VaSession = System.IntPtr;
    using VaVolume = System.IntPtr;
    using VaElement = System.IntPtr;

    using VaVector2f = Microsoft.MixedReality.Volumetric.VaVector2f;
    using VaVector3f = Microsoft.MixedReality.Volumetric.VaVector3f;
    using VaVector4f = Microsoft.MixedReality.Volumetric.VaVector4f;
    using VaQuaternionf = Microsoft.MixedReality.Volumetric.VaQuaternionf;
    using VaExtent3Df = Microsoft.MixedReality.Volumetric.VaExtent3Df;
    using VaPosef = Microsoft.MixedReality.Volumetric.VaPosef;
    using VaColor3f = Microsoft.MixedReality.Volumetric.VaColor3f;
    using VaColor4f = Microsoft.MixedReality.Volumetric.VaColor4f;
    using VaUuid = Microsoft.MixedReality.Volumetric.VaUuid;
    using VaSpaceLocationExt = Microsoft.MixedReality.Volumetric.VaSpaceLocationExt;
    using VaMeshBufferDescriptorExt = Microsoft.MixedReality.Volumetric.VaMeshBufferDescriptorExt;
    using VaMeshBufferDataExt = Microsoft.MixedReality.Volumetric.VaMeshBufferDataExt;

    internal partial class Api
    {
        public static VaVersion ApiVersion = Api.VaMakeVersion(0, 3, 25);

        public const System.UInt32 VA_MAX_EXTENSION_NAME_SIZE = 128;
        public const System.UInt32 VA_MAX_APPLICATION_NAME_SIZE = 128;
        public const System.UInt32 VA_MAX_LIBRARY_NAME_SIZE = 128;
        public const System.UInt32 VA_MAX_ERROR_STRING_SIZE = 256;
        public const VaDuration VA_NO_DURATION = (VaDuration)(0);
        public const VaDuration VA_INFINITE_DURATION = (VaDuration)(-1);
        public const VaBool32 VA_TRUE = (VaBool32)(1);
        public const VaBool32 VA_FALSE = (VaBool32)(0);
        public const System.UInt32 VA_HAND_JOINT_COUNT_EXT = 26;

        public enum VaObjectType : Int32
        {
            VA_OBJECT_TYPE_SESSION = 1,
            VA_OBJECT_TYPE_VOLUME = 2,
            VA_OBJECT_TYPE_ELEMENT = 3,
        }

        public enum VaResult : Int32
        {
            VA_SUCCESS = 0,
            VA_TIMEOUT_EXPIRED = 1,
            VA_EVENT_UNAVAILABLE = 2,
            VA_ERROR_VALIDATION_FAILURE = -1,
            VA_ERROR_RUNTIME_FAILURE = -2,
            VA_ERROR_OUT_OF_MEMORY = -3,
            VA_ERROR_API_VERSION_UNSUPPORTED = -4,
            VA_ERROR_INITIALIZATION_FAILED = -6,
            VA_ERROR_FUNCTION_UNSUPPORTED = -7,
            VA_ERROR_FEATURE_UNSUPPORTED = -8,
            VA_ERROR_EXTENSION_NOT_PRESENT = -9,
            VA_ERROR_LIMIT_REACHED = -10,
            VA_ERROR_SIZE_INSUFFICIENT = -11,
            VA_ERROR_HANDLE_INVALID = -12,
            VA_ERROR_ELEMENT_INVALID = -14,
            VA_ERROR_ELEMENT_TYPE_INVALID = -15,
            VA_ERROR_SYSTEM_DISCONNECTED = -16,
            VA_ERROR_SYSTEM_ID_INVALID = -17,
            VA_ERROR_SESSION_STOPPED = -18,
            VA_ERROR_VOLUME_INVALID = -20,
            VA_ERROR_VOLUME_NOT_READY = -21,
            VA_ERROR_VOLUME_NOT_RUNNING = -22,
            VA_ERROR_VOLUME_UPDATE_OUT_OF_SCOPE = -23,
            VA_ERROR_TIME_INVALID = -30,
            VA_ERROR_DURATION_INVALID = -31,
            VA_ERROR_INDEX_OUT_OF_RANGE = -40,
            VA_ERROR_NAME_INVALID = -45,
            VA_ERROR_RUNTIME_UNAVAILABLE = -51,
            VA_ERROR_PROPERTY_INVALID = -60,
            VA_ERROR_PROPERTY_VALUE_TYPE_INVALID = -61,
            VA_ERROR_VOLUME_DUPLICATE_ID = -62,
            VA_ERROR_CALL_ORDER_INVALID = -63,
        }

        public enum VaStructureType : Int32
        {
            VA_TYPE_UNKNOWN = 0,
            VA_TYPE_EXTENSION_PROPERTIES = 1,
            VA_TYPE_SESSION_CREATE_INFO = 2,
            VA_TYPE_EVENT_DATA_BUFFER = 4,
            VA_TYPE_EVENT_VOLUME_STATE_CHANGED = 6,
            VA_TYPE_VOLUME_CREATE_INFO = 12,
            VA_TYPE_ELEMENT_CREATE_INFO = 13,
            VA_TYPE_EVENT_CONNECTED_SYSTEM_CHANGED = 15,
            VA_TYPE_EVENT_UPDATE_VOLUME = 16,
            VA_TYPE_UPDATE_VOLUME_REQUEST_INFO = 19,
            VA_TYPE_UPDATE_VOLUME_REQUEST_RESULT = 20,
            VA_TYPE_UPDATE_VOLUME_BEGIN_INFO = 21,
            VA_TYPE_UPDATE_VOLUME_END_INFO = 22,
            VA_TYPE_UPDATE_VOLUME_FRAME_STATE = 23,
            VA_TYPE_EVENT_SESSION_STOPPED = 25,
            VA_TYPE_ELEMENT_ASYNC_ERROR_DATA = 30,
            VA_TYPE_EVENT_ELEMENT_ASYNC_STATE_CHANGED = 32,
            VA_TYPE_CHANGED_ELEMENTS_GET_INFO = 33,
            VA_TYPE_CHANGED_ELEMENTS = 31,
            VA_TYPE_EVENT_WAIT_INFO = 34,
            VA_TYPE_SPACE_LOCATE_INFO_EXT = 7001,
            VA_TYPE_SPACE_LOCATIONS_EXT = 7002,
            VA_TYPE_JOINT_LOCATE_INFO_EXT = 8001,
            VA_TYPE_JOINT_LOCATIONS_EXT = 8002,
            VA_TYPE_SESSION_CREATE_WITH_VOLUME_RESTORE_BEHAVIOR_EXT = 9002,
            VA_TYPE_VOLUME_CREATE_WITH_RESTORE_ID_EXT = 9003,
            VA_TYPE_VOLUME_CREATE_WITH_RESTORE_CONFIG_EXT = 9004,
            VA_TYPE_EVENT_VOLUME_RESTORE_REQUEST_EXT = 9005,
            VA_TYPE_EVENT_VOLUME_RESTORE_RESULT_EXT = 9006,
            VA_TYPE_EVENT_VOLUME_RESTORE_ID_INVALIDATED_EXT = 9007,
            VA_TYPE_GLTF2_MESH_RESOURCE_INDEX_INFO_EXT = 20001,
            VA_TYPE_EVENT_ADAPTIVE_CARD_ACTION_INVOKED_EXT = 21001,
            VA_TYPE_ADAPTIVE_CARD_ACTION_INVOKED_DATA_EXT = 21002,
            VA_TYPE_MESH_BUFFER_ACQUIRE_INFO_EXT = 22001,
            VA_TYPE_MESH_BUFFER_ACQUIRE_RESULT_EXT = 22002,
            VA_TYPE_MESH_BUFFER_RELEASE_INFO_EXT = 22003,
            VA_TYPE_MESH_RESOURCE_INIT_BUFFERS_INFO_EXT = 22004,
            VA_TYPE_MESH_BUFFER_RESIZE_INFO_EXT = 22005,
            VA_TYPE_EVENT_VOLUME_CONTAINER_MODE_CHANGED_EXT = 26001,
        }

        public enum VaVolumeState : Int32
        {
            VA_VOLUME_STATE_IDLE = 1,
            VA_VOLUME_STATE_RUNNING = 2,
            VA_VOLUME_STATE_CLOSED = 3,
        }

        public enum VaVolumeStateAction : Int32
        {
            VA_VOLUME_STATE_ACTION_ON_READY = 1,
            VA_VOLUME_STATE_ACTION_ON_CLOSE = 2,
            VA_VOLUME_STATE_ACTION_ON_PAUSE = 3,
            VA_VOLUME_STATE_ACTION_ON_RESUME = 4,
        }

        public enum VaElementType : Int32
        {
            VA_ELEMENT_TYPE_INVALID = 0,
            VA_ELEMENT_TYPE_VISUAL = 1,
            VA_ELEMENT_TYPE_MODEL_RESOURCE = 2,
            VA_ELEMENT_TYPE_VOLUME_CONTENT = 3,
            VA_ELEMENT_TYPE_VOLUME_CONTAINER = 4,
            VA_ELEMENT_TYPE_SPACE_LOCATOR_EXT = 7001,
            VA_ELEMENT_TYPE_HAND_TRACKER_EXT = 8001,
            VA_ELEMENT_TYPE_ADAPTIVE_CARD_EXT = 21001,
            VA_ELEMENT_TYPE_MESH_RESOURCE_EXT = 22001,
            VA_ELEMENT_TYPE_MATERIAL_RESOURCE_EXT = 23001,
            VA_ELEMENT_TYPE_TEXTURE_RESOURCE_EXT = 25001,
        }

        public enum VaElementProperty : Int32
        {
            VA_ELEMENT_PROPERTY_POSITION = 1,
            VA_ELEMENT_PROPERTY_ORIENTATION = 2,
            VA_ELEMENT_PROPERTY_SCALE = 3,
            VA_ELEMENT_PROPERTY_VISIBLE = 4,
            VA_ELEMENT_PROPERTY_ASYNC_STATE = 5,
            VA_ELEMENT_PROPERTY_VISUAL_RESOURCE = 6,
            VA_ELEMENT_PROPERTY_VISUAL_PARENT = 7,
            VA_ELEMENT_PROPERTY_VISUAL_REFERENCE = 8,
            VA_ELEMENT_PROPERTY_MODEL_REFERENCE = 9,
            VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_DISPLAY_NAME = 10,
            VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_ROTATION_LOCK = 11,
            VA_ELEMENT_PROPERTY_VOLUME_CONTENT_POSITION = 12,
            VA_ELEMENT_PROPERTY_VOLUME_CONTENT_ORIENTATION = 13,
            VA_ELEMENT_PROPERTY_VOLUME_CONTENT_SIZE = 14,
            VA_ELEMENT_PROPERTY_VOLUME_CONTENT_SIZE_BEHAVIOR = 15,
            VA_ELEMENT_PROPERTY_GLTF2_MODEL_URI_EXT = 20001,
            VA_ELEMENT_PROPERTY_GLTF2_NODE_NAME_EXT = 20002,
            VA_ELEMENT_PROPERTY_GLTF2_MATERIAL_NAME_EXT = 20003,
            VA_ELEMENT_PROPERTY_ADAPTIVE_CARD_TEMPLATE_EXT = 21001,
            VA_ELEMENT_PROPERTY_ADAPTIVE_CARD_DATA_EXT = 21002,
            VA_ELEMENT_PROPERTY_MATERIAL_TYPE_EXT = 23001,
            VA_ELEMENT_PROPERTY_MATERIAL_PBR_BASE_COLOR_FACTOR_EXT = 23002,
            VA_ELEMENT_PROPERTY_MATERIAL_PBR_METALLIC_FACTOR_EXT = 23003,
            VA_ELEMENT_PROPERTY_MATERIAL_PBR_ROUGHNESS_FACTOR_EXT = 23004,
            VA_ELEMENT_PROPERTY_MATERIAL_PBR_BASE_COLOR_TEXTURE_EXT = 23005,
            VA_ELEMENT_PROPERTY_MATERIAL_PBR_METALLIC_ROUGHNESS_TEXTURE_EXT = 23006,
            VA_ELEMENT_PROPERTY_MATERIAL_NORMAL_TEXTURE_EXT = 23007,
            VA_ELEMENT_PROPERTY_MATERIAL_OCCLUSION_TEXTURE_EXT = 23008,
            VA_ELEMENT_PROPERTY_MATERIAL_EMISSIVE_TEXTURE_EXT = 23009,
            VA_ELEMENT_PROPERTY_VOLUME_CONTENT_ACTUAL_SCALE_EXT = 24001,
            VA_ELEMENT_PROPERTY_VOLUME_CONTENT_ACTUAL_SIZE_EXT = 24002,
            VA_ELEMENT_PROPERTY_VOLUME_CONTENT_ACTUAL_POSITION_EXT = 24003,
            VA_ELEMENT_PROPERTY_TEXTURE_IMAGE_URI_EXT = 25001,
            VA_ELEMENT_PROPERTY_TEXTURE_NORMAL_SCALE_EXT = 25002,
            VA_ELEMENT_PROPERTY_TEXTURE_OCCLUSION_STRENGTH_EXT = 25003,
            VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_MODE_CAPABILITIES_EXT = 26001,
            VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_THUMBNAIL_MODEL_URI_EXT = 27001,
            VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_THUMBNAIL_ICON_URI_EXT = 27002,
        }

        public enum VaVolumeUpdateMode : Int32
        {
            VA_VOLUME_UPDATE_MODE_ON_DEMAND = 1,
            VA_VOLUME_UPDATE_MODE_FULL_FRAMERATE = 2,
            VA_VOLUME_UPDATE_MODE_HALF_FRAMERATE = 3,
            VA_VOLUME_UPDATE_MODE_THIRD_FRAMERATE = 4,
            VA_VOLUME_UPDATE_MODE_QUARTER_FRAMERATE = 5,
        }

        public enum VaElementAsyncState : Int32
        {
            VA_ELEMENT_ASYNC_STATE_READY = 1,
            VA_ELEMENT_ASYNC_STATE_PENDING = 2,
            VA_ELEMENT_ASYNC_STATE_ERROR = 3,
        }

        public enum VaElementAsyncError : Int32
        {
            VA_ELEMENT_ASYNC_ERROR_NO_MORE = 1,
            VA_ELEMENT_ASYNC_ERROR_USER_CANCELED = -1,
            VA_ELEMENT_ASYNC_ERROR_PLATFORM_FAILURE = -2,
            VA_ELEMENT_ASYNC_ERROR_LIMIT_REACHED = -3,
            VA_ELEMENT_ASYNC_ERROR_GLTF2_MODEL_URI_INVALID_EXT = -20001,
            VA_ELEMENT_ASYNC_ERROR_GLTF2_MODEL_URI_NOT_FOUND_EXT = -20002,
            VA_ELEMENT_ASYNC_ERROR_GLTF2_EXTENSION_UNSUPPORTED_EXT = -20003,
            VA_ELEMENT_ASYNC_ERROR_GLTF2_FILE_CONTENT_INVALID_EXT = -20004,
            VA_ELEMENT_ASYNC_ERROR_GLTF2_NODE_NAME_NOT_FOUND_EXT = -20005,
            VA_ELEMENT_ASYNC_ERROR_GLTF2_MESH_INDEX_NOT_FOUND_EXT = -20006,
            VA_ELEMENT_ASYNC_ERROR_GLTF2_MESH_PRIMITIVE_INDEX_NOT_FOUND_EXT = -20007,
            VA_ELEMENT_ASYNC_ERROR_GLTF2_MATERIAL_NAME_NOT_FOUND_EXT = -20008,
            VA_ELEMENT_ASYNC_ERROR_GLTF2_MESH_PARENT_MODEL_INVALID_EXT = -20009,
            VA_ELEMENT_ASYNC_ERROR_GLTF2_MESH_DATA_INIT_AFTER_LOAD_EXT = -20010,
            VA_ELEMENT_ASYNC_ERROR_ADAPTIVE_CARD_TEMPLATE_INVALID_EXT = -21001,
            VA_ELEMENT_ASYNC_ERROR_ADAPTIVE_CARD_DATA_INVALID_EXT = -21002,
            VA_ELEMENT_ASYNC_ERROR_MESH_BUFFER_DESCRIPTOR_UNSUPPORTED_EXT = -22001,
            VA_ELEMENT_ASYNC_ERROR_IMAGE_MODEL_URI_INVALID_EXT = -25001,
            VA_ELEMENT_ASYNC_ERROR_IMAGE_MODEL_URI_NOT_FOUND_EXT = -25002,
            VA_ELEMENT_ASYNC_ERROR_IMAGE_EXTENSION_UNSUPPORTED_EXT = -25003,
            VA_ELEMENT_ASYNC_ERROR_IMAGE_FILE_CONTENT_INVALID_EXT = -25004,
            VA_ELEMENT_ASYNC_ERROR_CONTAINER_THUMBNAIL_MODEL_URI_INVALID_EXT = -27001,
            VA_ELEMENT_ASYNC_ERROR_CONTAINER_THUMBNAIL_MODEL_INVALID_EXT = -27002,
            VA_ELEMENT_ASYNC_ERROR_CONTAINER_THUMBNAIL_MODEL_TOO_LARGE_EXT = -27003,
            VA_ELEMENT_ASYNC_ERROR_CONTAINER_THUMBNAIL_ICON_URI_INVALID_EXT = -27004,
            VA_ELEMENT_ASYNC_ERROR_CONTAINER_THUMBNAIL_ICON_SIZE_INVALID_EXT = -27005,
        }

        public enum VaVolumeSizeBehavior : Int32
        {
            VA_VOLUME_SIZE_BEHAVIOR_AUTO_SIZE = 1,
            VA_VOLUME_SIZE_BEHAVIOR_FIXED = 2,
        }

        public enum VaSessionWaitForSystemBehavior : Int32
        {
            VA_SESSION_WAIT_FOR_SYSTEM_BEHAVIOR_RETRY_WITH_USER_CANCEL = 1,
            VA_SESSION_WAIT_FOR_SYSTEM_BEHAVIOR_RETRY_SILENTLY = 2,
            VA_SESSION_WAIT_FOR_SYSTEM_BEHAVIOR_NO_WAIT = 3,
        }

        public enum VaSpaceTypeExt : Int32
        {
            VA_SPACE_TYPE_VOLUME_CONTAINER_EXT = 1,
            VA_SPACE_TYPE_VOLUME_CONTENT_EXT = 2,
            VA_SPACE_TYPE_VIEWER_EXT = 3,
            VA_SPACE_TYPE_LOCAL_EXT = 4,
        }

        public enum VaJointSetExt : Int32
        {
            VA_JOINT_SET_HAND_EXT = 1,
        }

        public enum VaSideExt : Int32
        {
            VA_SIDE_NONE_EXT = 0,
            VA_SIDE_LEFT_EXT = 1,
            VA_SIDE_RIGHT_EXT = 2,
        }

        public enum VaHandJointExt : Int32
        {
            VA_HAND_JOINT_PALM_EXT = 0,
            VA_HAND_JOINT_WRIST_EXT = 1,
            VA_HAND_JOINT_THUMB_METACARPAL_EXT = 2,
            VA_HAND_JOINT_THUMB_PROXIMAL_EXT = 3,
            VA_HAND_JOINT_THUMB_DISTAL_EXT = 4,
            VA_HAND_JOINT_THUMB_TIP_EXT = 5,
            VA_HAND_JOINT_INDEX_METACARPAL_EXT = 6,
            VA_HAND_JOINT_INDEX_PROXIMAL_EXT = 7,
            VA_HAND_JOINT_INDEX_INTERMEDIATE_EXT = 8,
            VA_HAND_JOINT_INDEX_DISTAL_EXT = 9,
            VA_HAND_JOINT_INDEX_TIP_EXT = 10,
            VA_HAND_JOINT_MIDDLE_METACARPAL_EXT = 11,
            VA_HAND_JOINT_MIDDLE_PROXIMAL_EXT = 12,
            VA_HAND_JOINT_MIDDLE_INTERMEDIATE_EXT = 13,
            VA_HAND_JOINT_MIDDLE_DISTAL_EXT = 14,
            VA_HAND_JOINT_MIDDLE_TIP_EXT = 15,
            VA_HAND_JOINT_RING_METACARPAL_EXT = 16,
            VA_HAND_JOINT_RING_PROXIMAL_EXT = 17,
            VA_HAND_JOINT_RING_INTERMEDIATE_EXT = 18,
            VA_HAND_JOINT_RING_DISTAL_EXT = 19,
            VA_HAND_JOINT_RING_TIP_EXT = 20,
            VA_HAND_JOINT_LITTLE_METACARPAL_EXT = 21,
            VA_HAND_JOINT_LITTLE_PROXIMAL_EXT = 22,
            VA_HAND_JOINT_LITTLE_INTERMEDIATE_EXT = 23,
            VA_HAND_JOINT_LITTLE_DISTAL_EXT = 24,
            VA_HAND_JOINT_LITTLE_TIP_EXT = 25,
        }

        public enum VaHandTrackingDataSourceExt : Int32
        {
            VA_HAND_TRACKING_DATA_SOURCE_UNAVAILABLE_EXT = 0,
            VA_HAND_TRACKING_DATA_SOURCE_HAND_TRACKING_EXT = 1,
            VA_HAND_TRACKING_DATA_SOURCE_CONTROLLER_SIMULATED_EXT = 2,
        }

        public enum VaVolumeRestoreBehaviorExt : Int32
        {
            VA_VOLUME_RESTORE_BEHAVIOR_NO_RESTORE_EXT = 1,
            VA_VOLUME_RESTORE_BEHAVIOR_BY_APP_EXT = 2,
            VA_VOLUME_RESTORE_BEHAVIOR_BY_PLATFORM_MULTIPLE_VOLUMES_EXT = 3,
        }

        public enum VaVolumeRestoredResultExt : Int32
        {
            VA_VOLUME_RESTORED_RESULT_SUCCESS_EXT = 0,
            VA_VOLUME_RESTORED_RESULT_INVALID_RESTORE_ID_EXT = -1,
            VA_VOLUME_RESTORED_RESULT_RESTORE_FAILED_EXT = -2,
        }

        public enum VaMeshBufferTypeExt : Int32
        {
            VA_MESH_BUFFER_TYPE_INDEX_EXT = 1,
            VA_MESH_BUFFER_TYPE_VERTEX_POSITION_EXT = 2,
            VA_MESH_BUFFER_TYPE_VERTEX_NORMAL_EXT = 3,
            VA_MESH_BUFFER_TYPE_VERTEX_TANGENT_EXT = 4,
            VA_MESH_BUFFER_TYPE_VERTEX_COLOR_EXT = 5,
            VA_MESH_BUFFER_TYPE_VERTEX_TEXCOORD_0_EXT = 6,
            VA_MESH_BUFFER_TYPE_VERTEX_TEXCOORD_1_EXT = 7,
        }

#pragma warning disable CA1720 // Some enum values are type names, and they are by design.
        public enum VaMeshBufferFormatExt : Int32
        {
            VA_MESH_BUFFER_FORMAT_UINT16_EXT = 1,
            VA_MESH_BUFFER_FORMAT_UINT32_EXT = 2,
            VA_MESH_BUFFER_FORMAT_FLOAT_EXT = 3,
            VA_MESH_BUFFER_FORMAT_FLOAT2_EXT = 4,
            VA_MESH_BUFFER_FORMAT_FLOAT3_EXT = 5,
            VA_MESH_BUFFER_FORMAT_FLOAT4_EXT = 6,
        }
#pragma warning restore CA1720

        public enum VaMaterialTypeExt : Int32
        {
            VA_MATERIAL_TYPE_PBR_EXT = 1,
        }

#pragma warning disable CA1711 // The 'Flags' in the name is by design.
        [Flags]
        public enum VaVolumeRotationLockFlags : UInt32
        {
            VA_VOLUME_ROTATION_LOCK_NONE = 0,
            VA_VOLUME_ROTATION_LOCK_X = 1,
            VA_VOLUME_ROTATION_LOCK_Y = 2,
            VA_VOLUME_ROTATION_LOCK_Z = 4,
        }
#pragma warning restore CA1711

#pragma warning disable CA1711 // The 'Flags' in the name is by design.
        [Flags]
        public enum VaElementChangeFilterFlags : UInt32
        {
            VA_ELEMENT_CHANGE_FILTER_NONE = 0,
            VA_ELEMENT_CHANGE_FILTER_ASYNC_STATE = 1,
        }
#pragma warning restore CA1711

#pragma warning disable CA1711 // The 'Flags' in the name is by design.
        [Flags]
        public enum VaVolumeContainerModeFlagsExt : UInt32
        {
            VA_VOLUME_CONTAINER_MODE_NONE_EXT = 0,
            VA_VOLUME_CONTAINER_MODE_INTERACTIVE_MODE_EXT = 1,
            VA_VOLUME_CONTAINER_MODE_ONE_TO_ONE_MODE_EXT = 2,
            VA_VOLUME_CONTAINER_MODE_SHAREABLE_IN_TEAMS_EXT = 4,
            VA_VOLUME_CONTAINER_MODE_UNBOUNDED_MODE_EXT = 8,
            VA_VOLUME_CONTAINER_MODE_SUBPART_MODE_EXT = 16,
            VA_VOLUME_CONTAINER_MODE_DEFAULT_ALLOWED_EXT = 30,
        }
#pragma warning restore CA1711

        [StructLayout(LayoutKind.Sequential)]
        public struct VaBaseInStructure
        {
            public VaStructureType type;
            public System.IntPtr next; // const struct VaBaseInStructure*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaBaseOutStructure
        {
            public VaStructureType type;
            public System.IntPtr next; // struct VaBaseOutStructure*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VaApplicationInfo
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)VA_MAX_APPLICATION_NAME_SIZE)]
            public string applicationName; // char[]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)VA_MAX_LIBRARY_NAME_SIZE)]
            public string libraryName; // char[]
            public System.UInt32 applicationVersion;
            public VaVersion apiVersion;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaSessionCreateInfo
        {
            public VaStructureType type;
            public System.IntPtr next; // const void*
            public VaApplicationInfo applicationInfo;
            public VaSessionWaitForSystemBehavior waitForSystemBehavior;
            public System.UInt32 enabledExtensionCount;
            public System.IntPtr enabledExtensionNames; // const char* const*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaVolumeCreateInfo
        {
            public VaStructureType type;
            public System.IntPtr next; // const void*
            public VaSystemId systemId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaElementCreateInfo
        {
            public VaStructureType type;
            public System.IntPtr next; // const void*
            public VaElementType elementType;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VaExtensionProperties
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)VA_MAX_EXTENSION_NAME_SIZE)]
            public string extensionName; // char[]
            public System.UInt32 extensionVersion;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaEventDataBuffer
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)4000)]
            public System.Byte[] varying; // uint8_t[]
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaEventDataBaseHeader
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaEventConnectedSystemChanged
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
            public VaSystemId systemId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaEventSessionStopped
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaEventVolumeStateChanged
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
            public VaVolume volume;
            public VaVolumeState state;
            public VaVolumeStateAction action;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaEventUpdateVolume
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
            public VaVolume volume;
            public VaFrameId frameId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaUpdateVolumeRequestInfo
        {
            public VaStructureType type;
            public System.IntPtr next; // const void*
            public VaVolumeUpdateMode updateMode;
            public VaDuration onDemandUpdateDelay;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaUpdateVolumeRequestResult
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaUpdateVolumeBeginInfo
        {
            public VaStructureType type;
            public System.IntPtr next; // const void*
            public VaFrameId frameId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaUpdateVolumeEndInfo
        {
            public VaStructureType type;
            public System.IntPtr next; // const void*
            public VaFrameId frameId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaUpdateVolumeFrameState
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
            public VaTime time;
            public VaDuration duration;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VaElementAsyncErrorData
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
            public VaElementAsyncError error;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)VA_MAX_ERROR_STRING_SIZE)]
            public string errorMessage; // char[]
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaEventElementAsyncStateChanged
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
            public VaVolume volume;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaChangedElementsGetInfo
        {
            public VaStructureType type;
            public System.IntPtr next; // const void*
            public VaElementChangeFilterFlags filterFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaChangedElements
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
            public System.UInt32 elementCapacityInput;
            public System.UInt32 elementCountOutput;
            public System.IntPtr elements; // VaElement*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaEventWaitInfo
        {
            public VaStructureType type;
            public System.IntPtr next; // const void*
            public VaDuration timeout;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaSpaceLocateInfoExt
        {
            public VaStructureType type;
            public System.IntPtr next; // const void*
            public VaSpaceTypeExt baseSpace;
            public System.UInt32 spaceCount;
            public System.IntPtr spaces; // const VaSpaceTypeExt*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaSpaceLocationsExt
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
            public System.UInt32 locationCount;
            public System.IntPtr locations; // VaSpaceLocationExt*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaJointLocateInfoExt
        {
            public VaStructureType type;
            public System.IntPtr next; // const void*
            public VaSpaceTypeExt baseSpace;
            public VaJointSetExt jointSet;
            public VaSideExt side;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaJointLocationsExt
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
            public VaBool32 hasDataSource;
            public VaBool32 isTracked;
            public VaTime time;
            public System.UInt32 jointCount;
            public System.IntPtr jointPoses; // VaPosef*
            public System.IntPtr jointRadii; // float*
            public VaHandTrackingDataSourceExt dataSource;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaSessionCreateWithVolumeRestoreBehaviorExt
        {
            public VaStructureType type;
            public System.IntPtr next; // const void*
            public VaVolumeRestoreBehaviorExt restoreBehavior;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaEventVolumeRestoreRequestExt
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
            public VaUuid volumeRestoreId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaEventVolumeRestoreResultExt
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
            public VaVolume volume;
            public VaVolumeRestoredResultExt volumeRestoreResult;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaEventVolumeRestoreIdInvalidatedExt
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
            public VaUuid volumeRestoreId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaVolumeCreateWithRestoreConfigExt
        {
            public VaStructureType type;
            public System.IntPtr next; // const void*
            public VaBool32 restorable;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaVolumeCreateWithRestoreIdExt
        {
            public VaStructureType type;
            public System.IntPtr next; // const void*
            public VaUuid volumeRestoreId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VaGltf2MeshResourceIndexInfoExt
        {
            public VaStructureType type;
            public System.IntPtr next; // const void*
            public VaElement modelResource;
            public string nodeName; // char[]
            public System.UInt32 meshIndex;
            public System.UInt32 meshPrimitiveIndex;
            public VaBool32 decoupleAccessors;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaEventAdaptiveCardActionInvokedExt
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
            public VaVolume volume;
            public VaElement element;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaAdaptiveCardActionInvokedDataExt
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
            public VaBool32 hasData;
            public System.UInt32 verbCapacityInput;
            public System.UInt32 verbCountOutput;
            public System.IntPtr verb; // char*
            public System.UInt32 dataCapacityInput;
            public System.UInt32 dataCountOutput;
            public System.IntPtr data; // char*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaMeshResourceInitBuffersInfoExt
        {
            public VaStructureType type;
            public System.IntPtr next; // const void*
            public VaBool32 initializeData;
            public System.UInt32 bufferDescriptorCount;
            public System.IntPtr bufferDescriptors; // const VaMeshBufferDescriptorExt*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaMeshBufferAcquireInfoExt
        {
            public VaStructureType type;
            public System.IntPtr next; // const void*
            public System.UInt32 bufferTypeCount;
            public System.IntPtr bufferTypes; // VaMeshBufferTypeExt*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaMeshBufferResizeInfoExt
        {
            public VaStructureType type;
            public System.IntPtr next; // const void*
            public System.UInt32 indexCount;
            public System.UInt32 vertexCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaMeshBufferAcquireResultExt
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
            public System.UInt32 bufferCount;
            public System.IntPtr buffers; // VaMeshBufferDataExt*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaMeshBufferReleaseInfoExt
        {
            public VaStructureType type;
            public System.IntPtr next; // const void*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VaEventVolumeContainerModeChangedExt
        {
            public VaStructureType type;
            public System.IntPtr next; // void*
            public VaVolume volume;
            public VaElement element;
            public VaVolumeContainerModeFlagsExt currentModes;
        }

        internal static VaVersion VaMakeVersion(ushort major, ushort minor, uint patch)
        {
            return (VaVersion)(((ulong)major << 48) | ((ulong)minor << 32) | (ulong)patch);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PFN_vaVoidFunction();

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaGetFunctionPointer(
            /* VaSession */ VaSession session,
            /* const char* */ string name,
            /* PFN_vaVoidFunction* */ out System.IntPtr function);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaEnumerateExtensions(
            /* uint32_t */ System.UInt32 propertyCapacityInput,
            /* uint32_t* */ out System.UInt32 propertyCountOutput,
            /* VaExtensionProperties* */ System.IntPtr properties);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaCreateSession(
            /* const VaSessionCreateInfo* */ VaSessionCreateInfo createInfo,
            /* VaSession* */ out VaSession session);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaDestroySession(
            /* VaSession */ VaSession session);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaRequestStopSession(
            /* VaSession */ VaSession session);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaPollEvent(
            /* VaSession */ VaSession session,
            /* VaEventDataBuffer* */ ref VaEventDataBuffer eventData);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaWaitForNextEvent(
            /* VaSession */ VaSession session,
            /* VaEventDataBuffer* */ ref VaEventDataBuffer eventData);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaWaitEvent(
            /* VaSession */ VaSession session,
            /* const VaEventWaitInfo* */ VaEventWaitInfo waitInfo,
            /* VaEventDataBuffer* */ ref VaEventDataBuffer eventData);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaCreateVolume(
            /* VaSession */ VaSession session,
            /* const VaVolumeCreateInfo* */ VaVolumeCreateInfo createInfo,
            /* VaVolume* */ out VaVolume volume);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaDestroyVolume(
            /* VaVolume */ VaVolume volume);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaRequestCloseVolume(
            /* VaVolume */ VaVolume volume);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaRequestUpdateVolume(
            /* VaVolume */ VaVolume volume,
            /* const VaUpdateVolumeRequestInfo* */ VaUpdateVolumeRequestInfo requestInfo,
            /* VaUpdateVolumeRequestResult* */ out VaUpdateVolumeRequestResult requestResult);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaBeginUpdateVolume(
            /* VaVolume */ VaVolume volume,
            /* const VaUpdateVolumeBeginInfo* */ VaUpdateVolumeBeginInfo beginInfo,
            /* VaUpdateVolumeFrameState* */ out VaUpdateVolumeFrameState frameState);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaEndUpdateVolume(
            /* VaVolume */ VaVolume volume,
            /* const VaUpdateVolumeEndInfo* */ VaUpdateVolumeEndInfo endInfo);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaCreateElement(
            /* VaVolume */ VaVolume volume,
            /* const VaElementCreateInfo* */ VaElementCreateInfo createInfo,
            /* VaElement* */ out VaElement element);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaDestroyElement(
            /* VaElement */ VaElement element);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaGetElementPropertyBool(
            /* VaElement */ VaElement element,
            /* VaElementProperty */ VaElementProperty property,
            /* VaBool32* */ out VaBool32 value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaGetElementPropertyEnum(
            /* VaElement */ VaElement element,
            /* VaElementProperty */ VaElementProperty property,
            /* int32_t* */ out System.Int32 value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaGetElementPropertyFloat(
            /* VaElement */ VaElement element,
            /* VaElementProperty */ VaElementProperty property,
            /* float* */ out float value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaGetElementPropertyVector3f(
            /* VaElement */ VaElement element,
            /* VaElementProperty */ VaElementProperty property,
            /* VaVector3f* */ out VaVector3f value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaGetElementPropertyQuaternionf(
            /* VaElement */ VaElement element,
            /* VaElementProperty */ VaElementProperty property,
            /* VaQuaternionf* */ out VaQuaternionf value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaGetElementPropertyExtent3Df(
            /* VaElement */ VaElement element,
            /* VaElementProperty */ VaElementProperty property,
            /* VaExtent3Df* */ out VaExtent3Df value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaSetElementPropertyBool(
            /* VaElement */ VaElement element,
            /* VaElementProperty */ VaElementProperty property,
            /* VaBool32 */ VaBool32 value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaSetElementPropertyEnum(
            /* VaElement */ VaElement element,
            /* VaElementProperty */ VaElementProperty property,
            /* int32_t */ System.Int32 value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaSetElementPropertyFlags(
            /* VaElement */ VaElement element,
            /* VaElementProperty */ VaElementProperty property,
            /* uint32_t */ System.UInt32 value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaSetElementPropertyString(
            /* VaElement */ VaElement element,
            /* VaElementProperty */ VaElementProperty property,
            /* const char* */ string value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaSetElementPropertyUInt32(
            /* VaElement */ VaElement element,
            /* VaElementProperty */ VaElementProperty property,
            /* uint32_t */ System.UInt32 value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaSetElementPropertyInt32(
            /* VaElement */ VaElement element,
            /* VaElementProperty */ VaElementProperty property,
            /* int32_t */ System.Int32 value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaSetElementPropertyHandle(
            /* VaElement */ VaElement element,
            /* VaElementProperty */ VaElementProperty property,
            /* VaElement */ VaElement value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaSetElementPropertyFloat(
            /* VaElement */ VaElement element,
            /* VaElementProperty */ VaElementProperty property,
            /* float */ float value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaSetElementPropertyVector3f(
            /* VaElement */ VaElement element,
            /* VaElementProperty */ VaElementProperty property,
            /* const VaVector3f* */ VaVector3f value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaSetElementPropertyColor3f(
            /* VaElement */ VaElement element,
            /* VaElementProperty */ VaElementProperty property,
            /* const VaColor3f* */ VaColor3f value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaSetElementPropertyColor4f(
            /* VaElement */ VaElement element,
            /* VaElementProperty */ VaElementProperty property,
            /* const VaColor4f* */ VaColor4f value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaSetElementPropertyQuaternionf(
            /* VaElement */ VaElement element,
            /* VaElementProperty */ VaElementProperty property,
            /* const VaQuaternionf* */ VaQuaternionf value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaSetElementPropertyExtent3Df(
            /* VaElement */ VaElement element,
            /* VaElementProperty */ VaElementProperty property,
            /* const VaExtent3Df* */ VaExtent3Df value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaGetNextElementAsyncError(
            /* VaElement */ VaElement element,
            /* VaElementAsyncErrorData* */ ref VaElementAsyncErrorData errorData);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaGetChangedElements(
            /* VaVolume */ VaVolume volume,
            /* const VaChangedElementsGetInfo* */ VaChangedElementsGetInfo getInfo,
            /* VaChangedElements* */ out VaChangedElements changedElements);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaLocateSpacesExt(
            /* VaElement */ VaElement spatialInputElement,
            /* const VaSpaceLocateInfoExt* */ VaSpaceLocateInfoExt locateInfo,
            /* VaSpaceLocationsExt* */ out VaSpaceLocationsExt locations);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaLocateJointsExt(
            /* VaElement */ VaElement handTrackingElement,
            /* const VaJointLocateInfoExt* */ VaJointLocateInfoExt locateInfo,
            /* VaJointLocationsExt* */ out VaJointLocationsExt locations);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaGetVolumeRestoreIdExt(
            /* VaVolume */ VaVolume volume,
            /* VaUuid* */ out VaUuid volumeRestoreId);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaRemoveRestorableVolumeExt(
            /* VaSession */ VaSession session,
            /* const VaUuid* */ VaUuid volumeRestoreId);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaGetNextAdaptiveCardActionInvokedDataExt(
            /* VaElement */ VaElement element,
            /* VaAdaptiveCardActionInvokedDataExt* */ out VaAdaptiveCardActionInvokedDataExt actionInvokedData);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaAcquireMeshBufferExt(
            /* VaElement */ VaElement meshElement,
            /* const VaMeshBufferAcquireInfoExt* */ VaMeshBufferAcquireInfoExt acquireInfo,
            /* VaMeshBufferAcquireResultExt* */ out VaMeshBufferAcquireResultExt acquireResult);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate VaResult PFN_vaReleaseMeshBufferExt(
            /* VaElement */ VaElement meshElement,
            /* const VaMeshBufferReleaseInfoExt* */ VaMeshBufferReleaseInfoExt releaseInfo);

    }
}
