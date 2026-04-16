// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable
// This file is generated from spec.xml

namespace Microsoft.MixedReality.Volumetric
{
    using System;
    using System.Runtime.InteropServices;

    using Api = Detail.Api;

    public enum VaVersion : System.UInt64 { }
    public enum VaBool32 : System.UInt32 { }
    public enum VaTime : System.Int64 { }
    public enum VaDuration : System.Int64 { }
    public enum VaSystemId : System.UInt32 { }
    public enum VaFrameId : System.UInt32 { }

    [StructLayout(LayoutKind.Sequential)]
    public struct VaVector2f
    {
        public float x;
        public float y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VaVector3f
    {
        public float x;
        public float y;
        public float z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VaVector4f
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VaQuaternionf
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VaExtent3Df
    {
        public float width;
        public float height;
        public float depth;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VaPosef
    {
        public VaQuaternionf orientation;
        public VaVector3f position;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VaColor3f
    {
        public float r;
        public float g;
        public float b;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VaColor4f
    {
        public float r;
        public float g;
        public float b;
        public float a;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VaSpaceLocationExt
    {
        public VaPosef pose;
        public VaBool32 isTracked;
        public VaTime time;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VaMeshBufferDescriptorExt
    {
        public VaMeshBufferTypeExt bufferType;
        public VaMeshBufferFormatExt bufferFormat;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VaMeshBufferDataExt
    {
        public VaMeshBufferDescriptorExt bufferDescriptor;
        public System.UInt64 bufferByteSize;
        public System.IntPtr buffer; // uint8_t*
    }

    public enum VaVolumeState : Int32
    {
        Idle = Api.VaVolumeState.VA_VOLUME_STATE_IDLE,
        Running = Api.VaVolumeState.VA_VOLUME_STATE_RUNNING,
        Closed = Api.VaVolumeState.VA_VOLUME_STATE_CLOSED,
    }

    public enum VaElementType : Int32
    {
        Invalid = Api.VaElementType.VA_ELEMENT_TYPE_INVALID,
        Visual = Api.VaElementType.VA_ELEMENT_TYPE_VISUAL,
        ModelResource = Api.VaElementType.VA_ELEMENT_TYPE_MODEL_RESOURCE,
        VolumeContent = Api.VaElementType.VA_ELEMENT_TYPE_VOLUME_CONTENT,
        VolumeContainer = Api.VaElementType.VA_ELEMENT_TYPE_VOLUME_CONTAINER,
        SpaceLocatorExt = Api.VaElementType.VA_ELEMENT_TYPE_SPACE_LOCATOR_EXT,
        HandTrackerExt = Api.VaElementType.VA_ELEMENT_TYPE_HAND_TRACKER_EXT,
        AdaptiveCardExt = Api.VaElementType.VA_ELEMENT_TYPE_ADAPTIVE_CARD_EXT,
        MeshResourceExt = Api.VaElementType.VA_ELEMENT_TYPE_MESH_RESOURCE_EXT,
        MaterialResourceExt = Api.VaElementType.VA_ELEMENT_TYPE_MATERIAL_RESOURCE_EXT,
        TextureResourceExt = Api.VaElementType.VA_ELEMENT_TYPE_TEXTURE_RESOURCE_EXT,
    }

    public enum VaElementProperty : Int32
    {
        Position = Api.VaElementProperty.VA_ELEMENT_PROPERTY_POSITION,
        Orientation = Api.VaElementProperty.VA_ELEMENT_PROPERTY_ORIENTATION,
        Scale = Api.VaElementProperty.VA_ELEMENT_PROPERTY_SCALE,
        Visible = Api.VaElementProperty.VA_ELEMENT_PROPERTY_VISIBLE,
        AsyncState = Api.VaElementProperty.VA_ELEMENT_PROPERTY_ASYNC_STATE,
        VisualResource = Api.VaElementProperty.VA_ELEMENT_PROPERTY_VISUAL_RESOURCE,
        VisualParent = Api.VaElementProperty.VA_ELEMENT_PROPERTY_VISUAL_PARENT,
        VisualReference = Api.VaElementProperty.VA_ELEMENT_PROPERTY_VISUAL_REFERENCE,
        ModelReference = Api.VaElementProperty.VA_ELEMENT_PROPERTY_MODEL_REFERENCE,
        VolumeContainerDisplayName = Api.VaElementProperty.VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_DISPLAY_NAME,
        VolumeContainerRotationLock = Api.VaElementProperty.VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_ROTATION_LOCK,
        VolumeContentPosition = Api.VaElementProperty.VA_ELEMENT_PROPERTY_VOLUME_CONTENT_POSITION,
        VolumeContentOrientation = Api.VaElementProperty.VA_ELEMENT_PROPERTY_VOLUME_CONTENT_ORIENTATION,
        VolumeContentSize = Api.VaElementProperty.VA_ELEMENT_PROPERTY_VOLUME_CONTENT_SIZE,
        VolumeContentSizeBehavior = Api.VaElementProperty.VA_ELEMENT_PROPERTY_VOLUME_CONTENT_SIZE_BEHAVIOR,
        Gltf2ModelUriExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_GLTF2_MODEL_URI_EXT,
        Gltf2NodeNameExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_GLTF2_NODE_NAME_EXT,
        Gltf2MaterialNameExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_GLTF2_MATERIAL_NAME_EXT,
        AdaptiveCardTemplateExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_ADAPTIVE_CARD_TEMPLATE_EXT,
        AdaptiveCardDataExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_ADAPTIVE_CARD_DATA_EXT,
        MaterialTypeExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_MATERIAL_TYPE_EXT,
        MaterialPbrBaseColorFactorExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_MATERIAL_PBR_BASE_COLOR_FACTOR_EXT,
        MaterialPbrMetallicFactorExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_MATERIAL_PBR_METALLIC_FACTOR_EXT,
        MaterialPbrRoughnessFactorExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_MATERIAL_PBR_ROUGHNESS_FACTOR_EXT,
        MaterialPbrBaseColorTextureExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_MATERIAL_PBR_BASE_COLOR_TEXTURE_EXT,
        MaterialPbrMetallicRoughnessTextureExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_MATERIAL_PBR_METALLIC_ROUGHNESS_TEXTURE_EXT,
        MaterialNormalTextureExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_MATERIAL_NORMAL_TEXTURE_EXT,
        MaterialOcclusionTextureExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_MATERIAL_OCCLUSION_TEXTURE_EXT,
        MaterialEmissiveTextureExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_MATERIAL_EMISSIVE_TEXTURE_EXT,
        VolumeContentActualScaleExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_VOLUME_CONTENT_ACTUAL_SCALE_EXT,
        VolumeContentActualSizeExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_VOLUME_CONTENT_ACTUAL_SIZE_EXT,
        VolumeContentActualPositionExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_VOLUME_CONTENT_ACTUAL_POSITION_EXT,
        TextureImageUriExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_TEXTURE_IMAGE_URI_EXT,
        TextureNormalScaleExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_TEXTURE_NORMAL_SCALE_EXT,
        TextureOcclusionStrengthExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_TEXTURE_OCCLUSION_STRENGTH_EXT,
        VolumeContainerModeCapabilitiesExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_MODE_CAPABILITIES_EXT,
        VolumeContainerThumbnailModelUriExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_THUMBNAIL_MODEL_URI_EXT,
        VolumeContainerThumbnailIconUriExt = Api.VaElementProperty.VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_THUMBNAIL_ICON_URI_EXT,
    }

    public enum VaVolumeUpdateMode : Int32
    {
        OnDemand = Api.VaVolumeUpdateMode.VA_VOLUME_UPDATE_MODE_ON_DEMAND,
        FullFramerate = Api.VaVolumeUpdateMode.VA_VOLUME_UPDATE_MODE_FULL_FRAMERATE,
        HalfFramerate = Api.VaVolumeUpdateMode.VA_VOLUME_UPDATE_MODE_HALF_FRAMERATE,
        ThirdFramerate = Api.VaVolumeUpdateMode.VA_VOLUME_UPDATE_MODE_THIRD_FRAMERATE,
        QuarterFramerate = Api.VaVolumeUpdateMode.VA_VOLUME_UPDATE_MODE_QUARTER_FRAMERATE,
    }

    public enum VaElementAsyncState : Int32
    {
        Ready = Api.VaElementAsyncState.VA_ELEMENT_ASYNC_STATE_READY,
        Pending = Api.VaElementAsyncState.VA_ELEMENT_ASYNC_STATE_PENDING,
        Error = Api.VaElementAsyncState.VA_ELEMENT_ASYNC_STATE_ERROR,
    }

    public enum VaElementAsyncError : Int32
    {
        NoMore = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_NO_MORE,
        UserCanceled = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_USER_CANCELED,
        PlatformFailure = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_PLATFORM_FAILURE,
        LimitReached = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_LIMIT_REACHED,
        Gltf2ModelUriInvalidExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_GLTF2_MODEL_URI_INVALID_EXT,
        Gltf2ModelUriNotFoundExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_GLTF2_MODEL_URI_NOT_FOUND_EXT,
        Gltf2ExtensionUnsupportedExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_GLTF2_EXTENSION_UNSUPPORTED_EXT,
        Gltf2FileContentInvalidExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_GLTF2_FILE_CONTENT_INVALID_EXT,
        Gltf2NodeNameNotFoundExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_GLTF2_NODE_NAME_NOT_FOUND_EXT,
        Gltf2MeshIndexNotFoundExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_GLTF2_MESH_INDEX_NOT_FOUND_EXT,
        Gltf2MeshPrimitiveIndexNotFoundExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_GLTF2_MESH_PRIMITIVE_INDEX_NOT_FOUND_EXT,
        Gltf2MaterialNameNotFoundExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_GLTF2_MATERIAL_NAME_NOT_FOUND_EXT,
        Gltf2MeshParentModelInvalidExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_GLTF2_MESH_PARENT_MODEL_INVALID_EXT,
        Gltf2MeshDataInitAfterLoadExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_GLTF2_MESH_DATA_INIT_AFTER_LOAD_EXT,
        AdaptiveCardTemplateInvalidExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_ADAPTIVE_CARD_TEMPLATE_INVALID_EXT,
        AdaptiveCardDataInvalidExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_ADAPTIVE_CARD_DATA_INVALID_EXT,
        MeshBufferDescriptorUnsupportedExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_MESH_BUFFER_DESCRIPTOR_UNSUPPORTED_EXT,
        ImageModelUriInvalidExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_IMAGE_MODEL_URI_INVALID_EXT,
        ImageModelUriNotFoundExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_IMAGE_MODEL_URI_NOT_FOUND_EXT,
        ImageExtensionUnsupportedExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_IMAGE_EXTENSION_UNSUPPORTED_EXT,
        ImageFileContentInvalidExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_IMAGE_FILE_CONTENT_INVALID_EXT,
        ContainerThumbnailModelUriInvalidExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_CONTAINER_THUMBNAIL_MODEL_URI_INVALID_EXT,
        ContainerThumbnailModelInvalidExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_CONTAINER_THUMBNAIL_MODEL_INVALID_EXT,
        ContainerThumbnailModelTooLargeExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_CONTAINER_THUMBNAIL_MODEL_TOO_LARGE_EXT,
        ContainerThumbnailIconUriInvalidExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_CONTAINER_THUMBNAIL_ICON_URI_INVALID_EXT,
        ContainerThumbnailIconSizeInvalidExt = Api.VaElementAsyncError.VA_ELEMENT_ASYNC_ERROR_CONTAINER_THUMBNAIL_ICON_SIZE_INVALID_EXT,
    }

    public enum VaVolumeSizeBehavior : Int32
    {
        AutoSize = Api.VaVolumeSizeBehavior.VA_VOLUME_SIZE_BEHAVIOR_AUTO_SIZE,
        Fixed = Api.VaVolumeSizeBehavior.VA_VOLUME_SIZE_BEHAVIOR_FIXED,
    }

    public enum VaSessionWaitForSystemBehavior : Int32
    {
        RetryWithUserCancel = Api.VaSessionWaitForSystemBehavior.VA_SESSION_WAIT_FOR_SYSTEM_BEHAVIOR_RETRY_WITH_USER_CANCEL,
        RetrySilently = Api.VaSessionWaitForSystemBehavior.VA_SESSION_WAIT_FOR_SYSTEM_BEHAVIOR_RETRY_SILENTLY,
        NoWait = Api.VaSessionWaitForSystemBehavior.VA_SESSION_WAIT_FOR_SYSTEM_BEHAVIOR_NO_WAIT,
    }

    public enum VaSpaceTypeExt : Int32
    {
        VolumeContainer = Api.VaSpaceTypeExt.VA_SPACE_TYPE_VOLUME_CONTAINER_EXT,
        VolumeContent = Api.VaSpaceTypeExt.VA_SPACE_TYPE_VOLUME_CONTENT_EXT,
        Viewer = Api.VaSpaceTypeExt.VA_SPACE_TYPE_VIEWER_EXT,
        Local = Api.VaSpaceTypeExt.VA_SPACE_TYPE_LOCAL_EXT,
    }

    public enum VaJointSetExt : Int32
    {
        Hand = Api.VaJointSetExt.VA_JOINT_SET_HAND_EXT,
    }

    public enum VaSideExt : Int32
    {
        None = Api.VaSideExt.VA_SIDE_NONE_EXT,
        Left = Api.VaSideExt.VA_SIDE_LEFT_EXT,
        Right = Api.VaSideExt.VA_SIDE_RIGHT_EXT,
    }

    public enum VaHandJointExt : Int32
    {
        Palm = Api.VaHandJointExt.VA_HAND_JOINT_PALM_EXT,
        Wrist = Api.VaHandJointExt.VA_HAND_JOINT_WRIST_EXT,
        ThumbMetacarpal = Api.VaHandJointExt.VA_HAND_JOINT_THUMB_METACARPAL_EXT,
        ThumbProximal = Api.VaHandJointExt.VA_HAND_JOINT_THUMB_PROXIMAL_EXT,
        ThumbDistal = Api.VaHandJointExt.VA_HAND_JOINT_THUMB_DISTAL_EXT,
        ThumbTip = Api.VaHandJointExt.VA_HAND_JOINT_THUMB_TIP_EXT,
        IndexMetacarpal = Api.VaHandJointExt.VA_HAND_JOINT_INDEX_METACARPAL_EXT,
        IndexProximal = Api.VaHandJointExt.VA_HAND_JOINT_INDEX_PROXIMAL_EXT,
        IndexIntermediate = Api.VaHandJointExt.VA_HAND_JOINT_INDEX_INTERMEDIATE_EXT,
        IndexDistal = Api.VaHandJointExt.VA_HAND_JOINT_INDEX_DISTAL_EXT,
        IndexTip = Api.VaHandJointExt.VA_HAND_JOINT_INDEX_TIP_EXT,
        MiddleMetacarpal = Api.VaHandJointExt.VA_HAND_JOINT_MIDDLE_METACARPAL_EXT,
        MiddleProximal = Api.VaHandJointExt.VA_HAND_JOINT_MIDDLE_PROXIMAL_EXT,
        MiddleIntermediate = Api.VaHandJointExt.VA_HAND_JOINT_MIDDLE_INTERMEDIATE_EXT,
        MiddleDistal = Api.VaHandJointExt.VA_HAND_JOINT_MIDDLE_DISTAL_EXT,
        MiddleTip = Api.VaHandJointExt.VA_HAND_JOINT_MIDDLE_TIP_EXT,
        RingMetacarpal = Api.VaHandJointExt.VA_HAND_JOINT_RING_METACARPAL_EXT,
        RingProximal = Api.VaHandJointExt.VA_HAND_JOINT_RING_PROXIMAL_EXT,
        RingIntermediate = Api.VaHandJointExt.VA_HAND_JOINT_RING_INTERMEDIATE_EXT,
        RingDistal = Api.VaHandJointExt.VA_HAND_JOINT_RING_DISTAL_EXT,
        RingTip = Api.VaHandJointExt.VA_HAND_JOINT_RING_TIP_EXT,
        LittleMetacarpal = Api.VaHandJointExt.VA_HAND_JOINT_LITTLE_METACARPAL_EXT,
        LittleProximal = Api.VaHandJointExt.VA_HAND_JOINT_LITTLE_PROXIMAL_EXT,
        LittleIntermediate = Api.VaHandJointExt.VA_HAND_JOINT_LITTLE_INTERMEDIATE_EXT,
        LittleDistal = Api.VaHandJointExt.VA_HAND_JOINT_LITTLE_DISTAL_EXT,
        LittleTip = Api.VaHandJointExt.VA_HAND_JOINT_LITTLE_TIP_EXT,
    }

    public enum VaHandTrackingDataSourceExt : Int32
    {
        Unavailable = Api.VaHandTrackingDataSourceExt.VA_HAND_TRACKING_DATA_SOURCE_UNAVAILABLE_EXT,
        HandTracking = Api.VaHandTrackingDataSourceExt.VA_HAND_TRACKING_DATA_SOURCE_HAND_TRACKING_EXT,
        ControllerSimulated = Api.VaHandTrackingDataSourceExt.VA_HAND_TRACKING_DATA_SOURCE_CONTROLLER_SIMULATED_EXT,
    }

    public enum VaVolumeRestoreBehaviorExt : Int32
    {
        NoRestore = Api.VaVolumeRestoreBehaviorExt.VA_VOLUME_RESTORE_BEHAVIOR_NO_RESTORE_EXT,
        ByApp = Api.VaVolumeRestoreBehaviorExt.VA_VOLUME_RESTORE_BEHAVIOR_BY_APP_EXT,
        ByPlatformMultipleVolumes = Api.VaVolumeRestoreBehaviorExt.VA_VOLUME_RESTORE_BEHAVIOR_BY_PLATFORM_MULTIPLE_VOLUMES_EXT,
    }

    public enum VaVolumeRestoredResultExt : Int32
    {
        Success = Api.VaVolumeRestoredResultExt.VA_VOLUME_RESTORED_RESULT_SUCCESS_EXT,
        InvalidRestoreId = Api.VaVolumeRestoredResultExt.VA_VOLUME_RESTORED_RESULT_INVALID_RESTORE_ID_EXT,
        RestoreFailed = Api.VaVolumeRestoredResultExt.VA_VOLUME_RESTORED_RESULT_RESTORE_FAILED_EXT,
    }

    public enum VaMeshBufferTypeExt : Int32
    {
        Index = Api.VaMeshBufferTypeExt.VA_MESH_BUFFER_TYPE_INDEX_EXT,
        VertexPosition = Api.VaMeshBufferTypeExt.VA_MESH_BUFFER_TYPE_VERTEX_POSITION_EXT,
        VertexNormal = Api.VaMeshBufferTypeExt.VA_MESH_BUFFER_TYPE_VERTEX_NORMAL_EXT,
        VertexTangent = Api.VaMeshBufferTypeExt.VA_MESH_BUFFER_TYPE_VERTEX_TANGENT_EXT,
        VertexColor = Api.VaMeshBufferTypeExt.VA_MESH_BUFFER_TYPE_VERTEX_COLOR_EXT,
        VertexTexcoord0 = Api.VaMeshBufferTypeExt.VA_MESH_BUFFER_TYPE_VERTEX_TEXCOORD_0_EXT,
        VertexTexcoord1 = Api.VaMeshBufferTypeExt.VA_MESH_BUFFER_TYPE_VERTEX_TEXCOORD_1_EXT,
    }

#pragma warning disable CA1720 // Some enum values are type names, and they are by design.
    public enum VaMeshBufferFormatExt : Int32
    {
        Uint16 = Api.VaMeshBufferFormatExt.VA_MESH_BUFFER_FORMAT_UINT16_EXT,
        Uint32 = Api.VaMeshBufferFormatExt.VA_MESH_BUFFER_FORMAT_UINT32_EXT,
        Float = Api.VaMeshBufferFormatExt.VA_MESH_BUFFER_FORMAT_FLOAT_EXT,
        Float2 = Api.VaMeshBufferFormatExt.VA_MESH_BUFFER_FORMAT_FLOAT2_EXT,
        Float3 = Api.VaMeshBufferFormatExt.VA_MESH_BUFFER_FORMAT_FLOAT3_EXT,
        Float4 = Api.VaMeshBufferFormatExt.VA_MESH_BUFFER_FORMAT_FLOAT4_EXT,
    }
#pragma warning restore CA1720

    public enum VaMaterialTypeExt : Int32
    {
        Pbr = Api.VaMaterialTypeExt.VA_MATERIAL_TYPE_PBR_EXT,
    }

#pragma warning disable CA1711 // The 'Flags' in the name is by design.
    [Flags]
    public enum VaVolumeRotationLockFlags : UInt32
    {
        None = Api.VaVolumeRotationLockFlags.VA_VOLUME_ROTATION_LOCK_NONE,
        X = Api.VaVolumeRotationLockFlags.VA_VOLUME_ROTATION_LOCK_X,
        Y = Api.VaVolumeRotationLockFlags.VA_VOLUME_ROTATION_LOCK_Y,
        Z = Api.VaVolumeRotationLockFlags.VA_VOLUME_ROTATION_LOCK_Z,
    }
#pragma warning restore CA1711

#pragma warning disable CA1711 // The 'Flags' in the name is by design.
    [Flags]
    public enum VaElementChangeFilterFlags : UInt32
    {
        None = Api.VaElementChangeFilterFlags.VA_ELEMENT_CHANGE_FILTER_NONE,
        AsyncState = Api.VaElementChangeFilterFlags.VA_ELEMENT_CHANGE_FILTER_ASYNC_STATE,
    }
#pragma warning restore CA1711

#pragma warning disable CA1711 // The 'Flags' in the name is by design.
    [Flags]
    public enum VaVolumeContainerModeFlagsExt : UInt32
    {
        None = Api.VaVolumeContainerModeFlagsExt.VA_VOLUME_CONTAINER_MODE_NONE_EXT,
        InteractiveMode = Api.VaVolumeContainerModeFlagsExt.VA_VOLUME_CONTAINER_MODE_INTERACTIVE_MODE_EXT,
        OneToOneMode = Api.VaVolumeContainerModeFlagsExt.VA_VOLUME_CONTAINER_MODE_ONE_TO_ONE_MODE_EXT,
        ShareableInTeams = Api.VaVolumeContainerModeFlagsExt.VA_VOLUME_CONTAINER_MODE_SHAREABLE_IN_TEAMS_EXT,
        UnboundedMode = Api.VaVolumeContainerModeFlagsExt.VA_VOLUME_CONTAINER_MODE_UNBOUNDED_MODE_EXT,
        SubpartMode = Api.VaVolumeContainerModeFlagsExt.VA_VOLUME_CONTAINER_MODE_SUBPART_MODE_EXT,
        DefaultAllowed = Api.VaVolumeContainerModeFlagsExt.VA_VOLUME_CONTAINER_MODE_DEFAULT_ALLOWED_EXT,
    }
#pragma warning restore CA1711

#pragma warning disable CA1707 // Member names here contains `_` to match the convention in ABI, and they are by design.
    public static class Extensions
    {
        public const string VA_EXT_locate_spaces = "VA_EXT_locate_spaces";
        public const string VA_EXT_locate_joints = "VA_EXT_locate_joints";
        public const string VA_EXT_volume_restore = "VA_EXT_volume_restore";
        public const string VA_EXT_gltf2_model_resource = "VA_EXT_gltf2_model_resource";
        public const string VA_EXT_adaptive_card_element = "VA_EXT_adaptive_card_element";
        public const string VA_EXT_mesh_edit = "VA_EXT_mesh_edit";
        public const string VA_EXT_material_resource = "VA_EXT_material_resource";
        public const string VA_EXT_volume_content_container = "VA_EXT_volume_content_container";
        public const string VA_EXT_texture_resource = "VA_EXT_texture_resource";
        public const string VA_EXT_volume_container_modes = "VA_EXT_volume_container_modes";
        public const string VA_EXT_volume_container_thumbnail = "VA_EXT_volume_container_thumbnail";
    }
#pragma warning restore CA1707

}
