#ifndef VOLUMETRIC_H_
#define VOLUMETRIC_H_ 1

#ifdef __cplusplus
extern "C" {
#endif

#include "volumetric_platform_defines.h"

#define VA_MAKE_VERSION(major, minor, patch) ((((major) & 0xffffULL) << 48) | (((minor) & 0xffffULL) << 32) | ((patch) & 0xffffffffULL))

#define VA_VERSION_MAJOR(version) (uint16_t)(((uint64_t)(version) >> 48) & 0xffffULL)
#define VA_VERSION_MINOR(version) (uint16_t)(((uint64_t)(version) >> 32) & 0xffffULL)
#define VA_VERSION_PATCH(version) (uint32_t)((uint64_t)(version) & 0xffffffffULL)

#if !defined(VA_DEFINE_HANDLE)
#define VA_DEFINE_HANDLE(object) typedef struct object##_T* object;
#endif

#if !defined(VA_DEFINE_ATOM)
#define VA_DEFINE_ATOM(object) typedef uint64_t object;
#endif

#if !defined(VA_NULL_HANDLE)
#if VA_CPP_NULLPTR_SUPPORTED
#define VA_NULL_HANDLE nullptr
#else
#define VA_NULL_HANDLE 0
#endif
#endif

#define VA_SUCCEEDED(result) ((result) >= 0)

#define VA_FAILED(result) ((result) < 0)

#define VA_UNQUALIFIED_SUCCESS(result) ((result) == 0)

#define VA_CURRENT_API_VERSION VA_MAKE_VERSION(0, 3, 25)

typedef uint64_t VaVersion;
typedef uint32_t VaBool32;
typedef int64_t VaTime;
typedef int64_t VaDuration;
typedef uint32_t VaSystemId;
typedef uint32_t VaFrameId;
typedef void(VA_API_PTR* PFN_vaVoidFunction)(void);

VA_DEFINE_HANDLE(VaSession);
VA_DEFINE_HANDLE(VaVolume);
VA_DEFINE_HANDLE(VaElement);

#define VA_MAX_EXTENSION_NAME_SIZE 128
#define VA_MAX_APPLICATION_NAME_SIZE 128
#define VA_MAX_LIBRARY_NAME_SIZE 128
#define VA_MAX_ERROR_STRING_SIZE 256
#define VA_NO_DURATION 0
#define VA_INFINITE_DURATION -1
#define VA_TRUE 1
#define VA_FALSE 0

typedef enum VaVolumeRotationLockFlags {
    VA_VOLUME_ROTATION_LOCK_NONE = 0,
    VA_VOLUME_ROTATION_LOCK_X = 1,
    VA_VOLUME_ROTATION_LOCK_Y = 2,
    VA_VOLUME_ROTATION_LOCK_Z = 4,

    VA_VOLUME_ROTATION_LOCK_FLAGS_MAX_ENUM = 0xFFFFFFFF,
} VaVolumeRotationLockFlags;

typedef enum VaElementChangeFilterFlags {
    VA_ELEMENT_CHANGE_FILTER_NONE = 0,
    VA_ELEMENT_CHANGE_FILTER_ASYNC_STATE = 1,

    VA_ELEMENT_CHANGE_FILTER_FLAGS_MAX_ENUM = 0xFFFFFFFF,
} VaElementChangeFilterFlags;

typedef enum VaObjectType {
    VA_OBJECT_TYPE_SESSION = 1,
    VA_OBJECT_TYPE_VOLUME = 2,
    VA_OBJECT_TYPE_ELEMENT = 3,

    VA_OBJECT_TYPE_MAX_ENUM = 0x7FFFFFFF,
} VaObjectType;

typedef enum VaResult {
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

    VA_RESULT_MAX_ENUM = 0x7FFFFFFF,
} VaResult;

typedef enum VaStructureType {
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

    VA_STRUCTURE_TYPE_MAX_ENUM = 0x7FFFFFFF,
} VaStructureType;

typedef enum VaVolumeState {
    VA_VOLUME_STATE_IDLE = 1,
    VA_VOLUME_STATE_RUNNING = 2,
    VA_VOLUME_STATE_CLOSED = 3,

    VA_VOLUME_STATE_MAX_ENUM = 0x7FFFFFFF,
} VaVolumeState;

typedef enum VaVolumeStateAction {
    VA_VOLUME_STATE_ACTION_ON_READY = 1,
    VA_VOLUME_STATE_ACTION_ON_CLOSE = 2,
    VA_VOLUME_STATE_ACTION_ON_PAUSE = 3,
    VA_VOLUME_STATE_ACTION_ON_RESUME = 4,

    VA_VOLUME_STATE_ACTION_MAX_ENUM = 0x7FFFFFFF,
} VaVolumeStateAction;

typedef enum VaElementType {
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

    VA_ELEMENT_TYPE_MAX_ENUM = 0x7FFFFFFF,
} VaElementType;

typedef enum VaElementProperty {
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

    VA_ELEMENT_PROPERTY_MAX_ENUM = 0x7FFFFFFF,
} VaElementProperty;

typedef enum VaVolumeUpdateMode {
    VA_VOLUME_UPDATE_MODE_ON_DEMAND = 1,
    VA_VOLUME_UPDATE_MODE_FULL_FRAMERATE = 2,
    VA_VOLUME_UPDATE_MODE_HALF_FRAMERATE = 3,
    VA_VOLUME_UPDATE_MODE_THIRD_FRAMERATE = 4,
    VA_VOLUME_UPDATE_MODE_QUARTER_FRAMERATE = 5,

    VA_VOLUME_UPDATE_MODE_MAX_ENUM = 0x7FFFFFFF,
} VaVolumeUpdateMode;

typedef enum VaElementAsyncState {
    VA_ELEMENT_ASYNC_STATE_READY = 1,
    VA_ELEMENT_ASYNC_STATE_PENDING = 2,
    VA_ELEMENT_ASYNC_STATE_ERROR = 3,

    VA_ELEMENT_ASYNC_STATE_MAX_ENUM = 0x7FFFFFFF,
} VaElementAsyncState;

typedef enum VaElementAsyncError {
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

    VA_ELEMENT_ASYNC_ERROR_MAX_ENUM = 0x7FFFFFFF,
} VaElementAsyncError;

typedef enum VaVolumeSizeBehavior {
    VA_VOLUME_SIZE_BEHAVIOR_AUTO_SIZE = 1,
    VA_VOLUME_SIZE_BEHAVIOR_FIXED = 2,

    VA_VOLUME_SIZE_BEHAVIOR_MAX_ENUM = 0x7FFFFFFF,
} VaVolumeSizeBehavior;

typedef enum VaSessionWaitForSystemBehavior {
    VA_SESSION_WAIT_FOR_SYSTEM_BEHAVIOR_RETRY_WITH_USER_CANCEL = 1,
    VA_SESSION_WAIT_FOR_SYSTEM_BEHAVIOR_RETRY_SILENTLY = 2,
    VA_SESSION_WAIT_FOR_SYSTEM_BEHAVIOR_NO_WAIT = 3,

    VA_SESSION_WAIT_FOR_SYSTEM_BEHAVIOR_MAX_ENUM = 0x7FFFFFFF,
} VaSessionWaitForSystemBehavior;

typedef enum VaSpaceTypeExt {
    VA_SPACE_TYPE_VOLUME_CONTAINER_EXT = 1,
    VA_SPACE_TYPE_VOLUME_CONTENT_EXT = 2,
    VA_SPACE_TYPE_VIEWER_EXT = 3,
    VA_SPACE_TYPE_LOCAL_EXT = 4,

    VA_SPACE_TYPE_EXT_MAX_ENUM = 0x7FFFFFFF,
} VaSpaceTypeExt;

typedef enum VaJointSetExt {
    VA_JOINT_SET_HAND_EXT = 1,

    VA_JOINT_SET_EXT_MAX_ENUM = 0x7FFFFFFF,
} VaJointSetExt;

typedef enum VaSideExt {
    VA_SIDE_NONE_EXT = 0,
    VA_SIDE_LEFT_EXT = 1,
    VA_SIDE_RIGHT_EXT = 2,

    VA_SIDE_EXT_MAX_ENUM = 0x7FFFFFFF,
} VaSideExt;

typedef enum VaHandJointExt {
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

    VA_HAND_JOINT_EXT_MAX_ENUM = 0x7FFFFFFF,
} VaHandJointExt;

typedef enum VaHandTrackingDataSourceExt {
    VA_HAND_TRACKING_DATA_SOURCE_UNAVAILABLE_EXT = 0,
    VA_HAND_TRACKING_DATA_SOURCE_HAND_TRACKING_EXT = 1,
    VA_HAND_TRACKING_DATA_SOURCE_CONTROLLER_SIMULATED_EXT = 2,

    VA_HAND_TRACKING_DATA_SOURCE_EXT_MAX_ENUM = 0x7FFFFFFF,
} VaHandTrackingDataSourceExt;

typedef enum VaVolumeRestoreBehaviorExt {
    VA_VOLUME_RESTORE_BEHAVIOR_NO_RESTORE_EXT = 1,
    VA_VOLUME_RESTORE_BEHAVIOR_BY_APP_EXT = 2,
    VA_VOLUME_RESTORE_BEHAVIOR_BY_PLATFORM_MULTIPLE_VOLUMES_EXT = 3,

    VA_VOLUME_RESTORE_BEHAVIOR_EXT_MAX_ENUM = 0x7FFFFFFF,
} VaVolumeRestoreBehaviorExt;

typedef enum VaVolumeRestoredResultExt {
    VA_VOLUME_RESTORED_RESULT_SUCCESS_EXT = 0,
    VA_VOLUME_RESTORED_RESULT_INVALID_RESTORE_ID_EXT = -1,
    VA_VOLUME_RESTORED_RESULT_RESTORE_FAILED_EXT = -2,

    VA_VOLUME_RESTORED_RESULT_EXT_MAX_ENUM = 0x7FFFFFFF,
} VaVolumeRestoredResultExt;

typedef enum VaMeshBufferTypeExt {
    VA_MESH_BUFFER_TYPE_INDEX_EXT = 1,
    VA_MESH_BUFFER_TYPE_VERTEX_POSITION_EXT = 2,
    VA_MESH_BUFFER_TYPE_VERTEX_NORMAL_EXT = 3,
    VA_MESH_BUFFER_TYPE_VERTEX_TANGENT_EXT = 4,
    VA_MESH_BUFFER_TYPE_VERTEX_COLOR_EXT = 5,
    VA_MESH_BUFFER_TYPE_VERTEX_TEXCOORD_0_EXT = 6,
    VA_MESH_BUFFER_TYPE_VERTEX_TEXCOORD_1_EXT = 7,

    VA_MESH_BUFFER_TYPE_EXT_MAX_ENUM = 0x7FFFFFFF,
} VaMeshBufferTypeExt;

typedef enum VaMeshBufferFormatExt {
    VA_MESH_BUFFER_FORMAT_UINT16_EXT = 1,
    VA_MESH_BUFFER_FORMAT_UINT32_EXT = 2,
    VA_MESH_BUFFER_FORMAT_FLOAT_EXT = 3,
    VA_MESH_BUFFER_FORMAT_FLOAT2_EXT = 4,
    VA_MESH_BUFFER_FORMAT_FLOAT3_EXT = 5,
    VA_MESH_BUFFER_FORMAT_FLOAT4_EXT = 6,

    VA_MESH_BUFFER_FORMAT_EXT_MAX_ENUM = 0x7FFFFFFF,
} VaMeshBufferFormatExt;

typedef enum VaMaterialTypeExt {
    VA_MATERIAL_TYPE_PBR_EXT = 1,

    VA_MATERIAL_TYPE_EXT_MAX_ENUM = 0x7FFFFFFF,
} VaMaterialTypeExt;

typedef struct VaVector2f {
    float x;
    float y;
} VaVector2f;

typedef struct VaVector3f {
    float x;
    float y;
    float z;
} VaVector3f;

typedef struct VaVector4f {
    float x;
    float y;
    float z;
    float w;
} VaVector4f;

typedef struct VaQuaternionf {
    float x;
    float y;
    float z;
    float w;
} VaQuaternionf;

typedef struct VaExtent3Df {
    float width;
    float height;
    float depth;
} VaExtent3Df;

typedef struct VaPosef {
    VaQuaternionf orientation;
    VaVector3f position;
} VaPosef;

typedef struct VaColor3f {
    float r;
    float g;
    float b;
} VaColor3f;

typedef struct VaColor4f {
    float r;
    float g;
    float b;
    float a;
} VaColor4f;

typedef struct VaUuid {
    uint8_t data[16];
} VaUuid;

typedef struct VaBaseInStructure {
    VaStructureType type;
    const struct VaBaseInStructure* next;
} VaBaseInStructure;

typedef struct VaBaseOutStructure {
    VaStructureType type;
    struct VaBaseOutStructure* next;
} VaBaseOutStructure;

typedef struct VaApplicationInfo {
    char applicationName[VA_MAX_APPLICATION_NAME_SIZE];
    char libraryName[VA_MAX_LIBRARY_NAME_SIZE];
    uint32_t applicationVersion;
    VaVersion apiVersion;
} VaApplicationInfo;

typedef struct VaSessionCreateInfo {
    VaStructureType type;
    const void* next;
    VaApplicationInfo applicationInfo;
    VaSessionWaitForSystemBehavior waitForSystemBehavior;
    uint32_t enabledExtensionCount;
    const char* const* enabledExtensionNames;
} VaSessionCreateInfo;

typedef struct VaVolumeCreateInfo {
    VaStructureType type;
    const void* next;
    VaSystemId systemId;
} VaVolumeCreateInfo;

typedef struct VaElementCreateInfo {
    VaStructureType type;
    const void* next;
    VaElementType elementType;
} VaElementCreateInfo;

typedef struct VaExtensionProperties {
    VaStructureType type;
    void* next;
    char extensionName[VA_MAX_EXTENSION_NAME_SIZE];
    uint32_t extensionVersion;
} VaExtensionProperties;

typedef struct VaEventDataBuffer {
    VaStructureType type;
    void* next;
    uint8_t varying[4000];
} VaEventDataBuffer;

typedef struct VaEventDataBaseHeader {
    VaStructureType type;
    void* next;
} VaEventDataBaseHeader;

typedef struct VaEventConnectedSystemChanged {
    VaStructureType type;
    void* next;
    VaSystemId systemId;
} VaEventConnectedSystemChanged;

typedef struct VaEventSessionStopped {
    VaStructureType type;
    void* next;
} VaEventSessionStopped;

typedef struct VaEventVolumeStateChanged {
    VaStructureType type;
    void* next;
    VaVolume volume;
    VaVolumeState state;
    VaVolumeStateAction action;
} VaEventVolumeStateChanged;

typedef struct VaEventUpdateVolume {
    VaStructureType type;
    void* next;
    VaVolume volume;
    VaFrameId frameId;
} VaEventUpdateVolume;

typedef struct VaUpdateVolumeRequestInfo {
    VaStructureType type;
    const void* next;
    VaVolumeUpdateMode updateMode;
    VaDuration onDemandUpdateDelay;
} VaUpdateVolumeRequestInfo;

typedef struct VaUpdateVolumeRequestResult {
    VaStructureType type;
    void* next;
} VaUpdateVolumeRequestResult;

typedef struct VaUpdateVolumeBeginInfo {
    VaStructureType type;
    const void* next;
    VaFrameId frameId;
} VaUpdateVolumeBeginInfo;

typedef struct VaUpdateVolumeEndInfo {
    VaStructureType type;
    const void* next;
    VaFrameId frameId;
} VaUpdateVolumeEndInfo;

typedef struct VaUpdateVolumeFrameState {
    VaStructureType type;
    void* next;
    VaTime time;
    VaDuration duration;
} VaUpdateVolumeFrameState;

typedef struct VaElementAsyncErrorData {
    VaStructureType type;
    void* next;
    VaElementAsyncError error;
    char errorMessage[VA_MAX_ERROR_STRING_SIZE];
} VaElementAsyncErrorData;

typedef struct VaEventElementAsyncStateChanged {
    VaStructureType type;
    void* next;
    VaVolume volume;
} VaEventElementAsyncStateChanged;

typedef struct VaChangedElementsGetInfo {
    VaStructureType type;
    const void* next;
    VaElementChangeFilterFlags filterFlags;
} VaChangedElementsGetInfo;

typedef struct VaChangedElements {
    VaStructureType type;
    void* next;
    uint32_t elementCapacityInput;
    uint32_t elementCountOutput;
    VaElement* elements;
} VaChangedElements;

typedef struct VaEventWaitInfo {
    VaStructureType type;
    const void* next;
    VaDuration timeout;
} VaEventWaitInfo;

typedef VaResult(VA_API_PTR* PFN_vaGetFunctionPointer)(VaSession session, const char* name, PFN_vaVoidFunction* function);
typedef VaResult(VA_API_PTR* PFN_vaEnumerateExtensions)(uint32_t propertyCapacityInput, uint32_t* propertyCountOutput, VaExtensionProperties* properties);
typedef VaResult(VA_API_PTR* PFN_vaCreateSession)(const VaSessionCreateInfo* createInfo, VaSession* session);
typedef VaResult(VA_API_PTR* PFN_vaDestroySession)(VaSession session);
typedef VaResult(VA_API_PTR* PFN_vaRequestStopSession)(VaSession session);
typedef VaResult(VA_API_PTR* PFN_vaPollEvent)(VaSession session, VaEventDataBuffer* eventData);
typedef VaResult(VA_API_PTR* PFN_vaWaitForNextEvent)(VaSession session, VaEventDataBuffer* eventData);
typedef VaResult(VA_API_PTR* PFN_vaWaitEvent)(VaSession session, const VaEventWaitInfo* waitInfo, VaEventDataBuffer* eventData);
typedef VaResult(VA_API_PTR* PFN_vaCreateVolume)(VaSession session, const VaVolumeCreateInfo* createInfo, VaVolume* volume);
typedef VaResult(VA_API_PTR* PFN_vaDestroyVolume)(VaVolume volume);
typedef VaResult(VA_API_PTR* PFN_vaRequestCloseVolume)(VaVolume volume);
typedef VaResult(VA_API_PTR* PFN_vaRequestUpdateVolume)(VaVolume volume, const VaUpdateVolumeRequestInfo* requestInfo, VaUpdateVolumeRequestResult* requestResult);
typedef VaResult(VA_API_PTR* PFN_vaBeginUpdateVolume)(VaVolume volume, const VaUpdateVolumeBeginInfo* beginInfo, VaUpdateVolumeFrameState* frameState);
typedef VaResult(VA_API_PTR* PFN_vaEndUpdateVolume)(VaVolume volume, const VaUpdateVolumeEndInfo* endInfo);
typedef VaResult(VA_API_PTR* PFN_vaCreateElement)(VaVolume volume, const VaElementCreateInfo* createInfo, VaElement* element);
typedef VaResult(VA_API_PTR* PFN_vaDestroyElement)(VaElement element);
typedef VaResult(VA_API_PTR* PFN_vaGetElementPropertyBool)(VaElement element, VaElementProperty property, VaBool32* value);
typedef VaResult(VA_API_PTR* PFN_vaGetElementPropertyEnum)(VaElement element, VaElementProperty property, int32_t* value);
typedef VaResult(VA_API_PTR* PFN_vaGetElementPropertyFloat)(VaElement element, VaElementProperty property, float* value);
typedef VaResult(VA_API_PTR* PFN_vaGetElementPropertyVector3f)(VaElement element, VaElementProperty property, VaVector3f* value);
typedef VaResult(VA_API_PTR* PFN_vaGetElementPropertyQuaternionf)(VaElement element, VaElementProperty property, VaQuaternionf* value);
typedef VaResult(VA_API_PTR* PFN_vaGetElementPropertyExtent3Df)(VaElement element, VaElementProperty property, VaExtent3Df* value);
typedef VaResult(VA_API_PTR* PFN_vaSetElementPropertyBool)(VaElement element, VaElementProperty property, VaBool32 value);
typedef VaResult(VA_API_PTR* PFN_vaSetElementPropertyEnum)(VaElement element, VaElementProperty property, int32_t value);
typedef VaResult(VA_API_PTR* PFN_vaSetElementPropertyFlags)(VaElement element, VaElementProperty property, uint32_t value);
typedef VaResult(VA_API_PTR* PFN_vaSetElementPropertyString)(VaElement element, VaElementProperty property, const char* value);
typedef VaResult(VA_API_PTR* PFN_vaSetElementPropertyUInt32)(VaElement element, VaElementProperty property, uint32_t value);
typedef VaResult(VA_API_PTR* PFN_vaSetElementPropertyInt32)(VaElement element, VaElementProperty property, int32_t value);
typedef VaResult(VA_API_PTR* PFN_vaSetElementPropertyHandle)(VaElement element, VaElementProperty property, VaElement value);
typedef VaResult(VA_API_PTR* PFN_vaSetElementPropertyFloat)(VaElement element, VaElementProperty property, float value);
typedef VaResult(VA_API_PTR* PFN_vaSetElementPropertyVector3f)(VaElement element, VaElementProperty property, const VaVector3f* value);
typedef VaResult(VA_API_PTR* PFN_vaSetElementPropertyColor3f)(VaElement element, VaElementProperty property, const VaColor3f* value);
typedef VaResult(VA_API_PTR* PFN_vaSetElementPropertyColor4f)(VaElement element, VaElementProperty property, const VaColor4f* value);
typedef VaResult(VA_API_PTR* PFN_vaSetElementPropertyQuaternionf)(VaElement element, VaElementProperty property, const VaQuaternionf* value);
typedef VaResult(VA_API_PTR* PFN_vaSetElementPropertyExtent3Df)(VaElement element, VaElementProperty property, const VaExtent3Df* value);
typedef VaResult(VA_API_PTR* PFN_vaGetNextElementAsyncError)(VaElement element, VaElementAsyncErrorData* errorData);
typedef VaResult(VA_API_PTR* PFN_vaGetChangedElements)(VaVolume volume, const VaChangedElementsGetInfo* getInfo, VaChangedElements* changedElements);

#ifdef VA_PROTOTYPES
VaResult VA_API_CALL vaGetFunctionPointer(VaSession session, const char* name, PFN_vaVoidFunction* function);
VaResult VA_API_CALL vaEnumerateExtensions(uint32_t propertyCapacityInput, uint32_t* propertyCountOutput, VaExtensionProperties* properties);
VaResult VA_API_CALL vaCreateSession(const VaSessionCreateInfo* createInfo, VaSession* session);
VaResult VA_API_CALL vaDestroySession(VaSession session);
VaResult VA_API_CALL vaRequestStopSession(VaSession session);
VaResult VA_API_CALL vaPollEvent(VaSession session, VaEventDataBuffer* eventData);
VaResult VA_API_CALL vaWaitForNextEvent(VaSession session, VaEventDataBuffer* eventData);
VaResult VA_API_CALL vaWaitEvent(VaSession session, const VaEventWaitInfo* waitInfo, VaEventDataBuffer* eventData);
VaResult VA_API_CALL vaCreateVolume(VaSession session, const VaVolumeCreateInfo* createInfo, VaVolume* volume);
VaResult VA_API_CALL vaDestroyVolume(VaVolume volume);
VaResult VA_API_CALL vaRequestCloseVolume(VaVolume volume);
VaResult VA_API_CALL vaRequestUpdateVolume(VaVolume volume, const VaUpdateVolumeRequestInfo* requestInfo, VaUpdateVolumeRequestResult* requestResult);
VaResult VA_API_CALL vaBeginUpdateVolume(VaVolume volume, const VaUpdateVolumeBeginInfo* beginInfo, VaUpdateVolumeFrameState* frameState);
VaResult VA_API_CALL vaEndUpdateVolume(VaVolume volume, const VaUpdateVolumeEndInfo* endInfo);
VaResult VA_API_CALL vaCreateElement(VaVolume volume, const VaElementCreateInfo* createInfo, VaElement* element);
VaResult VA_API_CALL vaDestroyElement(VaElement element);
VaResult VA_API_CALL vaGetElementPropertyBool(VaElement element, VaElementProperty property, VaBool32* value);
VaResult VA_API_CALL vaGetElementPropertyEnum(VaElement element, VaElementProperty property, int32_t* value);
VaResult VA_API_CALL vaGetElementPropertyFloat(VaElement element, VaElementProperty property, float* value);
VaResult VA_API_CALL vaGetElementPropertyVector3f(VaElement element, VaElementProperty property, VaVector3f* value);
VaResult VA_API_CALL vaGetElementPropertyQuaternionf(VaElement element, VaElementProperty property, VaQuaternionf* value);
VaResult VA_API_CALL vaGetElementPropertyExtent3Df(VaElement element, VaElementProperty property, VaExtent3Df* value);
VaResult VA_API_CALL vaSetElementPropertyBool(VaElement element, VaElementProperty property, VaBool32 value);
VaResult VA_API_CALL vaSetElementPropertyEnum(VaElement element, VaElementProperty property, int32_t value);
VaResult VA_API_CALL vaSetElementPropertyFlags(VaElement element, VaElementProperty property, uint32_t value);
VaResult VA_API_CALL vaSetElementPropertyString(VaElement element, VaElementProperty property, const char* value);
VaResult VA_API_CALL vaSetElementPropertyUInt32(VaElement element, VaElementProperty property, uint32_t value);
VaResult VA_API_CALL vaSetElementPropertyInt32(VaElement element, VaElementProperty property, int32_t value);
VaResult VA_API_CALL vaSetElementPropertyHandle(VaElement element, VaElementProperty property, VaElement value);
VaResult VA_API_CALL vaSetElementPropertyFloat(VaElement element, VaElementProperty property, float value);
VaResult VA_API_CALL vaSetElementPropertyVector3f(VaElement element, VaElementProperty property, const VaVector3f* value);
VaResult VA_API_CALL vaSetElementPropertyColor3f(VaElement element, VaElementProperty property, const VaColor3f* value);
VaResult VA_API_CALL vaSetElementPropertyColor4f(VaElement element, VaElementProperty property, const VaColor4f* value);
VaResult VA_API_CALL vaSetElementPropertyQuaternionf(VaElement element, VaElementProperty property, const VaQuaternionf* value);
VaResult VA_API_CALL vaSetElementPropertyExtent3Df(VaElement element, VaElementProperty property, const VaExtent3Df* value);
VaResult VA_API_CALL vaGetNextElementAsyncError(VaElement element, VaElementAsyncErrorData* errorData);
VaResult VA_API_CALL vaGetChangedElements(VaVolume volume, const VaChangedElementsGetInfo* getInfo, VaChangedElements* changedElements);
#endif /* !VA_PROTOTYPES */

#define VA_EXT_LOCATE_SPACES 1
#define VA_EXT_locate_spaces_SPEC_VERSION 1
#define VA_EXT_LOCATE_SPACES_EXTENSION_NAME "VA_EXT_locate_spaces"

typedef struct VaSpaceLocationExt {
    VaPosef pose;
    VaBool32 isTracked;
    VaTime time;
} VaSpaceLocationExt;

typedef struct VaSpaceLocateInfoExt {
    VaStructureType type;
    const void* next;
    VaSpaceTypeExt baseSpace;
    uint32_t spaceCount;
    const VaSpaceTypeExt* spaces;
} VaSpaceLocateInfoExt;

typedef struct VaSpaceLocationsExt {
    VaStructureType type;
    void* next;
    uint32_t locationCount;
    VaSpaceLocationExt* locations;
} VaSpaceLocationsExt;

typedef VaResult(VA_API_PTR* PFN_vaLocateSpacesExt)(VaElement spatialInputElement, const VaSpaceLocateInfoExt* locateInfo, VaSpaceLocationsExt* locations);

#ifdef VA_PROTOTYPES
VaResult VA_API_CALL vaLocateSpacesExt(VaElement spatialInputElement, const VaSpaceLocateInfoExt* locateInfo, VaSpaceLocationsExt* locations);
#endif /* !VA_PROTOTYPES */

#define VA_EXT_LOCATE_JOINTS 1
#define VA_EXT_locate_joints_SPEC_VERSION 1
#define VA_EXT_LOCATE_JOINTS_EXTENSION_NAME "VA_EXT_locate_joints"

#define VA_HAND_JOINT_COUNT_EXT 26

typedef struct VaJointLocateInfoExt {
    VaStructureType type;
    const void* next;
    VaSpaceTypeExt baseSpace;
    VaJointSetExt jointSet;
    VaSideExt side;
} VaJointLocateInfoExt;

typedef struct VaJointLocationsExt {
    VaStructureType type;
    void* next;
    VaBool32 hasDataSource;
    VaBool32 isTracked;
    VaTime time;
    uint32_t jointCount;
    VaPosef* jointPoses;
    float* jointRadii;
    VaHandTrackingDataSourceExt dataSource;
} VaJointLocationsExt;

typedef VaResult(VA_API_PTR* PFN_vaLocateJointsExt)(VaElement handTrackingElement, const VaJointLocateInfoExt* locateInfo, VaJointLocationsExt* locations);

#ifdef VA_PROTOTYPES
VaResult VA_API_CALL vaLocateJointsExt(VaElement handTrackingElement, const VaJointLocateInfoExt* locateInfo, VaJointLocationsExt* locations);
#endif /* !VA_PROTOTYPES */

#define VA_EXT_VOLUME_RESTORE 1
#define VA_EXT_volume_restore_SPEC_VERSION 1
#define VA_EXT_VOLUME_RESTORE_EXTENSION_NAME "VA_EXT_volume_restore"

typedef struct VaSessionCreateWithVolumeRestoreBehaviorExt { // extend VaSessionCreateInfo
    VaStructureType type;
    const void* next;
    VaVolumeRestoreBehaviorExt restoreBehavior;
} VaSessionCreateWithVolumeRestoreBehaviorExt;

typedef struct VaEventVolumeRestoreRequestExt {
    VaStructureType type;
    void* next;
    VaUuid volumeRestoreId;
} VaEventVolumeRestoreRequestExt;

typedef struct VaEventVolumeRestoreResultExt {
    VaStructureType type;
    void* next;
    VaVolume volume;
    VaVolumeRestoredResultExt volumeRestoreResult;
} VaEventVolumeRestoreResultExt;

typedef struct VaEventVolumeRestoreIdInvalidatedExt {
    VaStructureType type;
    void* next;
    VaUuid volumeRestoreId;
} VaEventVolumeRestoreIdInvalidatedExt;

typedef struct VaVolumeCreateWithRestoreConfigExt { // extend VaVolumeCreateInfo
    VaStructureType type;
    const void* next;
    VaBool32 restorable;
} VaVolumeCreateWithRestoreConfigExt;

typedef struct VaVolumeCreateWithRestoreIdExt { // extend VaVolumeCreateInfo
    VaStructureType type;
    const void* next;
    VaUuid volumeRestoreId;
} VaVolumeCreateWithRestoreIdExt;

typedef VaResult(VA_API_PTR* PFN_vaGetVolumeRestoreIdExt)(VaVolume volume, VaUuid* volumeRestoreId);
typedef VaResult(VA_API_PTR* PFN_vaRemoveRestorableVolumeExt)(VaSession session, const VaUuid* volumeRestoreId);

#ifdef VA_PROTOTYPES
VaResult VA_API_CALL vaGetVolumeRestoreIdExt(VaVolume volume, VaUuid* volumeRestoreId);
VaResult VA_API_CALL vaRemoveRestorableVolumeExt(VaSession session, const VaUuid* volumeRestoreId);
#endif /* !VA_PROTOTYPES */

#define VA_EXT_GLTF2_MODEL_RESOURCE 1
#define VA_EXT_gltf2_model_resource_SPEC_VERSION 1
#define VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME "VA_EXT_gltf2_model_resource"

typedef struct VaGltf2MeshResourceIndexInfoExt { // extend VaElementCreateInfo
    VaStructureType type;
    const void* next;
    VaElement modelResource;
    const char* nodeName;
    uint32_t meshIndex;
    uint32_t meshPrimitiveIndex;
    VaBool32 decoupleAccessors;
} VaGltf2MeshResourceIndexInfoExt;

#ifdef VA_PROTOTYPES
#endif /* !VA_PROTOTYPES */

#define VA_EXT_ADAPTIVE_CARD_ELEMENT 1
#define VA_EXT_adaptive_card_element_SPEC_VERSION 1
#define VA_EXT_ADAPTIVE_CARD_ELEMENT_EXTENSION_NAME "VA_EXT_adaptive_card_element"

typedef struct VaEventAdaptiveCardActionInvokedExt {
    VaStructureType type;
    void* next;
    VaVolume volume;
    VaElement element;
} VaEventAdaptiveCardActionInvokedExt;

typedef struct VaAdaptiveCardActionInvokedDataExt {
    VaStructureType type;
    void* next;
    VaBool32 hasData;
    uint32_t verbCapacityInput;
    uint32_t verbCountOutput;
    char* verb;
    uint32_t dataCapacityInput;
    uint32_t dataCountOutput;
    char* data;
} VaAdaptiveCardActionInvokedDataExt;

typedef VaResult(VA_API_PTR* PFN_vaGetNextAdaptiveCardActionInvokedDataExt)(VaElement element, VaAdaptiveCardActionInvokedDataExt* actionInvokedData);

#ifdef VA_PROTOTYPES
VaResult VA_API_CALL vaGetNextAdaptiveCardActionInvokedDataExt(VaElement element, VaAdaptiveCardActionInvokedDataExt* actionInvokedData);
#endif /* !VA_PROTOTYPES */

#define VA_EXT_MESH_EDIT 1
#define VA_EXT_mesh_edit_SPEC_VERSION 1
#define VA_EXT_MESH_EDIT_EXTENSION_NAME "VA_EXT_mesh_edit"

typedef struct VaMeshBufferDescriptorExt {
    VaMeshBufferTypeExt bufferType;
    VaMeshBufferFormatExt bufferFormat;
} VaMeshBufferDescriptorExt;

typedef struct VaMeshResourceInitBuffersInfoExt { // extend VaElementCreateInfo
    VaStructureType type;
    const void* next;
    VaBool32 initializeData;
    uint32_t bufferDescriptorCount;
    const VaMeshBufferDescriptorExt* bufferDescriptors;
} VaMeshResourceInitBuffersInfoExt;

typedef struct VaMeshBufferAcquireInfoExt {
    VaStructureType type;
    const void* next;
    uint32_t bufferTypeCount;
    VaMeshBufferTypeExt* bufferTypes;
} VaMeshBufferAcquireInfoExt;

typedef struct VaMeshBufferResizeInfoExt { // extend VaMeshBufferAcquireInfoExt
    VaStructureType type;
    const void* next;
    uint32_t indexCount;
    uint32_t vertexCount;
} VaMeshBufferResizeInfoExt;

typedef struct VaMeshBufferDataExt {
    VaMeshBufferDescriptorExt bufferDescriptor;
    uint64_t bufferByteSize;
    uint8_t* buffer;
} VaMeshBufferDataExt;

typedef struct VaMeshBufferAcquireResultExt {
    VaStructureType type;
    void* next;
    uint32_t bufferCount;
    VaMeshBufferDataExt* buffers;
} VaMeshBufferAcquireResultExt;

typedef struct VaMeshBufferReleaseInfoExt {
    VaStructureType type;
    const void* next;
} VaMeshBufferReleaseInfoExt;

typedef VaResult(VA_API_PTR* PFN_vaAcquireMeshBufferExt)(VaElement meshElement, const VaMeshBufferAcquireInfoExt* acquireInfo, VaMeshBufferAcquireResultExt* acquireResult);
typedef VaResult(VA_API_PTR* PFN_vaReleaseMeshBufferExt)(VaElement meshElement, const VaMeshBufferReleaseInfoExt* releaseInfo);

#ifdef VA_PROTOTYPES
VaResult VA_API_CALL vaAcquireMeshBufferExt(VaElement meshElement, const VaMeshBufferAcquireInfoExt* acquireInfo, VaMeshBufferAcquireResultExt* acquireResult);
VaResult VA_API_CALL vaReleaseMeshBufferExt(VaElement meshElement, const VaMeshBufferReleaseInfoExt* releaseInfo);
#endif /* !VA_PROTOTYPES */

#define VA_EXT_MATERIAL_RESOURCE 1
#define VA_EXT_material_resource_SPEC_VERSION 1
#define VA_EXT_MATERIAL_RESOURCE_EXTENSION_NAME "VA_EXT_material_resource"

#ifdef VA_PROTOTYPES
#endif /* !VA_PROTOTYPES */

#define VA_EXT_VOLUME_CONTENT_CONTAINER 1
#define VA_EXT_volume_content_container_SPEC_VERSION 1
#define VA_EXT_VOLUME_CONTENT_CONTAINER_EXTENSION_NAME "VA_EXT_volume_content_container"

#ifdef VA_PROTOTYPES
#endif /* !VA_PROTOTYPES */

#define VA_EXT_TEXTURE_RESOURCE 1
#define VA_EXT_texture_resource_SPEC_VERSION 1
#define VA_EXT_TEXTURE_RESOURCE_EXTENSION_NAME "VA_EXT_texture_resource"

#ifdef VA_PROTOTYPES
#endif /* !VA_PROTOTYPES */

#define VA_EXT_VOLUME_CONTAINER_MODES 1
#define VA_EXT_volume_container_modes_SPEC_VERSION 1
#define VA_EXT_VOLUME_CONTAINER_MODES_EXTENSION_NAME "VA_EXT_volume_container_modes"

typedef enum VaVolumeContainerModeFlagsExt {
    VA_VOLUME_CONTAINER_MODE_NONE_EXT = 0,
    VA_VOLUME_CONTAINER_MODE_INTERACTIVE_MODE_EXT = 1,
    VA_VOLUME_CONTAINER_MODE_ONE_TO_ONE_MODE_EXT = 2,
    VA_VOLUME_CONTAINER_MODE_SHAREABLE_IN_TEAMS_EXT = 4,
    VA_VOLUME_CONTAINER_MODE_UNBOUNDED_MODE_EXT = 8,
    VA_VOLUME_CONTAINER_MODE_SUBPART_MODE_EXT = 16,
    VA_VOLUME_CONTAINER_MODE_DEFAULT_ALLOWED_EXT = 30,

    VA_VOLUME_CONTAINER_MODE_FLAGS_EXT_MAX_ENUM = 0xFFFFFFFF,
} VaVolumeContainerModeFlagsExt;

typedef struct VaEventVolumeContainerModeChangedExt {
    VaStructureType type;
    void* next;
    VaVolume volume;
    VaElement element;
    VaVolumeContainerModeFlagsExt currentModes;
} VaEventVolumeContainerModeChangedExt;

#ifdef VA_PROTOTYPES
#endif /* !VA_PROTOTYPES */

#define VA_EXT_VOLUME_CONTAINER_THUMBNAIL 1
#define VA_EXT_volume_container_thumbnail_SPEC_VERSION 1
#define VA_EXT_VOLUME_CONTAINER_THUMBNAIL_EXTENSION_NAME "VA_EXT_volume_container_thumbnail"

#ifdef VA_PROTOTYPES
#endif /* !VA_PROTOTYPES */

#ifdef __cplusplus
}
#endif

#endif /* VOLUMETRIC_H_ */
