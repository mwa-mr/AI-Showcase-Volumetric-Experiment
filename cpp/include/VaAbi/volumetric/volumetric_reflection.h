#ifndef VOLUMETRIC_REFLECTION_H
#define VOLUMETRIC_REFLECTION_H

#include "volumetric.h"

#define LIST_ENUM_VaObjectType(_) \
    _(VA_OBJECT_TYPE_SESSION, 1)  \
    _(VA_OBJECT_TYPE_VOLUME, 2)   \
    _(VA_OBJECT_TYPE_ELEMENT, 3)

#define LIST_ENUM_VaResult(_)                    \
    _(VA_SUCCESS, 0)                             \
    _(VA_TIMEOUT_EXPIRED, 1)                     \
    _(VA_EVENT_UNAVAILABLE, 2)                   \
    _(VA_ERROR_VALIDATION_FAILURE, -1)           \
    _(VA_ERROR_RUNTIME_FAILURE, -2)              \
    _(VA_ERROR_OUT_OF_MEMORY, -3)                \
    _(VA_ERROR_API_VERSION_UNSUPPORTED, -4)      \
    _(VA_ERROR_INITIALIZATION_FAILED, -6)        \
    _(VA_ERROR_FUNCTION_UNSUPPORTED, -7)         \
    _(VA_ERROR_FEATURE_UNSUPPORTED, -8)          \
    _(VA_ERROR_EXTENSION_NOT_PRESENT, -9)        \
    _(VA_ERROR_LIMIT_REACHED, -10)               \
    _(VA_ERROR_SIZE_INSUFFICIENT, -11)           \
    _(VA_ERROR_HANDLE_INVALID, -12)              \
    _(VA_ERROR_ELEMENT_INVALID, -14)             \
    _(VA_ERROR_ELEMENT_TYPE_INVALID, -15)        \
    _(VA_ERROR_SYSTEM_DISCONNECTED, -16)         \
    _(VA_ERROR_SYSTEM_ID_INVALID, -17)           \
    _(VA_ERROR_SESSION_STOPPED, -18)             \
    _(VA_ERROR_VOLUME_INVALID, -20)              \
    _(VA_ERROR_VOLUME_NOT_READY, -21)            \
    _(VA_ERROR_VOLUME_NOT_RUNNING, -22)          \
    _(VA_ERROR_VOLUME_UPDATE_OUT_OF_SCOPE, -23)  \
    _(VA_ERROR_TIME_INVALID, -30)                \
    _(VA_ERROR_DURATION_INVALID, -31)            \
    _(VA_ERROR_INDEX_OUT_OF_RANGE, -40)          \
    _(VA_ERROR_NAME_INVALID, -45)                \
    _(VA_ERROR_RUNTIME_UNAVAILABLE, -51)         \
    _(VA_ERROR_PROPERTY_INVALID, -60)            \
    _(VA_ERROR_PROPERTY_VALUE_TYPE_INVALID, -61) \
    _(VA_ERROR_VOLUME_DUPLICATE_ID, -62)         \
    _(VA_ERROR_CALL_ORDER_INVALID, -63)

#define LIST_ENUM_VaStructureType(_)                                 \
    _(VA_TYPE_UNKNOWN, 0)                                            \
    _(VA_TYPE_EXTENSION_PROPERTIES, 1)                               \
    _(VA_TYPE_SESSION_CREATE_INFO, 2)                                \
    _(VA_TYPE_EVENT_DATA_BUFFER, 4)                                  \
    _(VA_TYPE_EVENT_VOLUME_STATE_CHANGED, 6)                         \
    _(VA_TYPE_VOLUME_CREATE_INFO, 12)                                \
    _(VA_TYPE_ELEMENT_CREATE_INFO, 13)                               \
    _(VA_TYPE_EVENT_CONNECTED_SYSTEM_CHANGED, 15)                    \
    _(VA_TYPE_EVENT_UPDATE_VOLUME, 16)                               \
    _(VA_TYPE_UPDATE_VOLUME_REQUEST_INFO, 19)                        \
    _(VA_TYPE_UPDATE_VOLUME_REQUEST_RESULT, 20)                      \
    _(VA_TYPE_UPDATE_VOLUME_BEGIN_INFO, 21)                          \
    _(VA_TYPE_UPDATE_VOLUME_END_INFO, 22)                            \
    _(VA_TYPE_UPDATE_VOLUME_FRAME_STATE, 23)                         \
    _(VA_TYPE_EVENT_SESSION_STOPPED, 25)                             \
    _(VA_TYPE_ELEMENT_ASYNC_ERROR_DATA, 30)                          \
    _(VA_TYPE_EVENT_ELEMENT_ASYNC_STATE_CHANGED, 32)                 \
    _(VA_TYPE_CHANGED_ELEMENTS_GET_INFO, 33)                         \
    _(VA_TYPE_CHANGED_ELEMENTS, 31)                                  \
    _(VA_TYPE_EVENT_WAIT_INFO, 34)                                   \
    _(VA_TYPE_SPACE_LOCATE_INFO_EXT, 7001)                           \
    _(VA_TYPE_SPACE_LOCATIONS_EXT, 7002)                             \
    _(VA_TYPE_JOINT_LOCATE_INFO_EXT, 8001)                           \
    _(VA_TYPE_JOINT_LOCATIONS_EXT, 8002)                             \
    _(VA_TYPE_SESSION_CREATE_WITH_VOLUME_RESTORE_BEHAVIOR_EXT, 9002) \
    _(VA_TYPE_VOLUME_CREATE_WITH_RESTORE_ID_EXT, 9003)               \
    _(VA_TYPE_VOLUME_CREATE_WITH_RESTORE_CONFIG_EXT, 9004)           \
    _(VA_TYPE_EVENT_VOLUME_RESTORE_REQUEST_EXT, 9005)                \
    _(VA_TYPE_EVENT_VOLUME_RESTORE_RESULT_EXT, 9006)                 \
    _(VA_TYPE_EVENT_VOLUME_RESTORE_ID_INVALIDATED_EXT, 9007)         \
    _(VA_TYPE_GLTF2_MESH_RESOURCE_INDEX_INFO_EXT, 20001)             \
    _(VA_TYPE_EVENT_ADAPTIVE_CARD_ACTION_INVOKED_EXT, 21001)         \
    _(VA_TYPE_ADAPTIVE_CARD_ACTION_INVOKED_DATA_EXT, 21002)          \
    _(VA_TYPE_MESH_BUFFER_ACQUIRE_INFO_EXT, 22001)                   \
    _(VA_TYPE_MESH_BUFFER_ACQUIRE_RESULT_EXT, 22002)                 \
    _(VA_TYPE_MESH_BUFFER_RELEASE_INFO_EXT, 22003)                   \
    _(VA_TYPE_MESH_RESOURCE_INIT_BUFFERS_INFO_EXT, 22004)            \
    _(VA_TYPE_MESH_BUFFER_RESIZE_INFO_EXT, 22005)                    \
    _(VA_TYPE_EVENT_VOLUME_CONTAINER_MODE_CHANGED_EXT, 26001)

#define LIST_ENUM_VaVolumeState(_) \
    _(VA_VOLUME_STATE_IDLE, 1)     \
    _(VA_VOLUME_STATE_RUNNING, 2)  \
    _(VA_VOLUME_STATE_CLOSED, 3)

#define LIST_ENUM_VaVolumeStateAction(_)  \
    _(VA_VOLUME_STATE_ACTION_ON_READY, 1) \
    _(VA_VOLUME_STATE_ACTION_ON_CLOSE, 2) \
    _(VA_VOLUME_STATE_ACTION_ON_PAUSE, 3) \
    _(VA_VOLUME_STATE_ACTION_ON_RESUME, 4)

#define LIST_ENUM_VaElementType(_)                  \
    _(VA_ELEMENT_TYPE_INVALID, 0)                   \
    _(VA_ELEMENT_TYPE_VISUAL, 1)                    \
    _(VA_ELEMENT_TYPE_MODEL_RESOURCE, 2)            \
    _(VA_ELEMENT_TYPE_VOLUME_CONTENT, 3)            \
    _(VA_ELEMENT_TYPE_VOLUME_CONTAINER, 4)          \
    _(VA_ELEMENT_TYPE_SPACE_LOCATOR_EXT, 7001)      \
    _(VA_ELEMENT_TYPE_HAND_TRACKER_EXT, 8001)       \
    _(VA_ELEMENT_TYPE_ADAPTIVE_CARD_EXT, 21001)     \
    _(VA_ELEMENT_TYPE_MESH_RESOURCE_EXT, 22001)     \
    _(VA_ELEMENT_TYPE_MATERIAL_RESOURCE_EXT, 23001) \
    _(VA_ELEMENT_TYPE_TEXTURE_RESOURCE_EXT, 25001)

#define LIST_ENUM_VaElementProperty(_)                                        \
    _(VA_ELEMENT_PROPERTY_POSITION, 1)                                        \
    _(VA_ELEMENT_PROPERTY_ORIENTATION, 2)                                     \
    _(VA_ELEMENT_PROPERTY_SCALE, 3)                                           \
    _(VA_ELEMENT_PROPERTY_VISIBLE, 4)                                         \
    _(VA_ELEMENT_PROPERTY_ASYNC_STATE, 5)                                     \
    _(VA_ELEMENT_PROPERTY_VISUAL_RESOURCE, 6)                                 \
    _(VA_ELEMENT_PROPERTY_VISUAL_PARENT, 7)                                   \
    _(VA_ELEMENT_PROPERTY_VISUAL_REFERENCE, 8)                                \
    _(VA_ELEMENT_PROPERTY_MODEL_REFERENCE, 9)                                 \
    _(VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_DISPLAY_NAME, 10)                  \
    _(VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_ROTATION_LOCK, 11)                 \
    _(VA_ELEMENT_PROPERTY_VOLUME_CONTENT_POSITION, 12)                        \
    _(VA_ELEMENT_PROPERTY_VOLUME_CONTENT_ORIENTATION, 13)                     \
    _(VA_ELEMENT_PROPERTY_VOLUME_CONTENT_SIZE, 14)                            \
    _(VA_ELEMENT_PROPERTY_VOLUME_CONTENT_SIZE_BEHAVIOR, 15)                   \
    _(VA_ELEMENT_PROPERTY_GLTF2_MODEL_URI_EXT, 20001)                         \
    _(VA_ELEMENT_PROPERTY_GLTF2_NODE_NAME_EXT, 20002)                         \
    _(VA_ELEMENT_PROPERTY_GLTF2_MATERIAL_NAME_EXT, 20003)                     \
    _(VA_ELEMENT_PROPERTY_ADAPTIVE_CARD_TEMPLATE_EXT, 21001)                  \
    _(VA_ELEMENT_PROPERTY_ADAPTIVE_CARD_DATA_EXT, 21002)                      \
    _(VA_ELEMENT_PROPERTY_MATERIAL_TYPE_EXT, 23001)                           \
    _(VA_ELEMENT_PROPERTY_MATERIAL_PBR_BASE_COLOR_FACTOR_EXT, 23002)          \
    _(VA_ELEMENT_PROPERTY_MATERIAL_PBR_METALLIC_FACTOR_EXT, 23003)            \
    _(VA_ELEMENT_PROPERTY_MATERIAL_PBR_ROUGHNESS_FACTOR_EXT, 23004)           \
    _(VA_ELEMENT_PROPERTY_MATERIAL_PBR_BASE_COLOR_TEXTURE_EXT, 23005)         \
    _(VA_ELEMENT_PROPERTY_MATERIAL_PBR_METALLIC_ROUGHNESS_TEXTURE_EXT, 23006) \
    _(VA_ELEMENT_PROPERTY_MATERIAL_NORMAL_TEXTURE_EXT, 23007)                 \
    _(VA_ELEMENT_PROPERTY_MATERIAL_OCCLUSION_TEXTURE_EXT, 23008)              \
    _(VA_ELEMENT_PROPERTY_MATERIAL_EMISSIVE_TEXTURE_EXT, 23009)               \
    _(VA_ELEMENT_PROPERTY_VOLUME_CONTENT_ACTUAL_SCALE_EXT, 24001)             \
    _(VA_ELEMENT_PROPERTY_VOLUME_CONTENT_ACTUAL_SIZE_EXT, 24002)              \
    _(VA_ELEMENT_PROPERTY_VOLUME_CONTENT_ACTUAL_POSITION_EXT, 24003)          \
    _(VA_ELEMENT_PROPERTY_TEXTURE_IMAGE_URI_EXT, 25001)                       \
    _(VA_ELEMENT_PROPERTY_TEXTURE_NORMAL_SCALE_EXT, 25002)                    \
    _(VA_ELEMENT_PROPERTY_TEXTURE_OCCLUSION_STRENGTH_EXT, 25003)              \
    _(VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_MODE_CAPABILITIES_EXT, 26001)      \
    _(VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_THUMBNAIL_MODEL_URI_EXT, 27001)    \
    _(VA_ELEMENT_PROPERTY_VOLUME_CONTAINER_THUMBNAIL_ICON_URI_EXT, 27002)

#define LIST_ENUM_VaVolumeUpdateMode(_)         \
    _(VA_VOLUME_UPDATE_MODE_ON_DEMAND, 1)       \
    _(VA_VOLUME_UPDATE_MODE_FULL_FRAMERATE, 2)  \
    _(VA_VOLUME_UPDATE_MODE_HALF_FRAMERATE, 3)  \
    _(VA_VOLUME_UPDATE_MODE_THIRD_FRAMERATE, 4) \
    _(VA_VOLUME_UPDATE_MODE_QUARTER_FRAMERATE, 5)

#define LIST_ENUM_VaElementAsyncState(_) \
    _(VA_ELEMENT_ASYNC_STATE_READY, 1)   \
    _(VA_ELEMENT_ASYNC_STATE_PENDING, 2) \
    _(VA_ELEMENT_ASYNC_STATE_ERROR, 3)

#define LIST_ENUM_VaElementAsyncError(_)                                        \
    _(VA_ELEMENT_ASYNC_ERROR_NO_MORE, 1)                                        \
    _(VA_ELEMENT_ASYNC_ERROR_USER_CANCELED, -1)                                 \
    _(VA_ELEMENT_ASYNC_ERROR_PLATFORM_FAILURE, -2)                              \
    _(VA_ELEMENT_ASYNC_ERROR_LIMIT_REACHED, -3)                                 \
    _(VA_ELEMENT_ASYNC_ERROR_GLTF2_MODEL_URI_INVALID_EXT, -20001)               \
    _(VA_ELEMENT_ASYNC_ERROR_GLTF2_MODEL_URI_NOT_FOUND_EXT, -20002)             \
    _(VA_ELEMENT_ASYNC_ERROR_GLTF2_EXTENSION_UNSUPPORTED_EXT, -20003)           \
    _(VA_ELEMENT_ASYNC_ERROR_GLTF2_FILE_CONTENT_INVALID_EXT, -20004)            \
    _(VA_ELEMENT_ASYNC_ERROR_GLTF2_NODE_NAME_NOT_FOUND_EXT, -20005)             \
    _(VA_ELEMENT_ASYNC_ERROR_GLTF2_MESH_INDEX_NOT_FOUND_EXT, -20006)            \
    _(VA_ELEMENT_ASYNC_ERROR_GLTF2_MESH_PRIMITIVE_INDEX_NOT_FOUND_EXT, -20007)  \
    _(VA_ELEMENT_ASYNC_ERROR_GLTF2_MATERIAL_NAME_NOT_FOUND_EXT, -20008)         \
    _(VA_ELEMENT_ASYNC_ERROR_GLTF2_MESH_PARENT_MODEL_INVALID_EXT, -20009)       \
    _(VA_ELEMENT_ASYNC_ERROR_GLTF2_MESH_DATA_INIT_AFTER_LOAD_EXT, -20010)       \
    _(VA_ELEMENT_ASYNC_ERROR_ADAPTIVE_CARD_TEMPLATE_INVALID_EXT, -21001)        \
    _(VA_ELEMENT_ASYNC_ERROR_ADAPTIVE_CARD_DATA_INVALID_EXT, -21002)            \
    _(VA_ELEMENT_ASYNC_ERROR_MESH_BUFFER_DESCRIPTOR_UNSUPPORTED_EXT, -22001)    \
    _(VA_ELEMENT_ASYNC_ERROR_IMAGE_MODEL_URI_INVALID_EXT, -25001)               \
    _(VA_ELEMENT_ASYNC_ERROR_IMAGE_MODEL_URI_NOT_FOUND_EXT, -25002)             \
    _(VA_ELEMENT_ASYNC_ERROR_IMAGE_EXTENSION_UNSUPPORTED_EXT, -25003)           \
    _(VA_ELEMENT_ASYNC_ERROR_IMAGE_FILE_CONTENT_INVALID_EXT, -25004)            \
    _(VA_ELEMENT_ASYNC_ERROR_CONTAINER_THUMBNAIL_MODEL_URI_INVALID_EXT, -27001) \
    _(VA_ELEMENT_ASYNC_ERROR_CONTAINER_THUMBNAIL_MODEL_INVALID_EXT, -27002)     \
    _(VA_ELEMENT_ASYNC_ERROR_CONTAINER_THUMBNAIL_MODEL_TOO_LARGE_EXT, -27003)   \
    _(VA_ELEMENT_ASYNC_ERROR_CONTAINER_THUMBNAIL_ICON_URI_INVALID_EXT, -27004)  \
    _(VA_ELEMENT_ASYNC_ERROR_CONTAINER_THUMBNAIL_ICON_SIZE_INVALID_EXT, -27005)

#define LIST_ENUM_VaVolumeSizeBehavior(_)   \
    _(VA_VOLUME_SIZE_BEHAVIOR_AUTO_SIZE, 1) \
    _(VA_VOLUME_SIZE_BEHAVIOR_FIXED, 2)

#define LIST_ENUM_VaSessionWaitForSystemBehavior(_)                  \
    _(VA_SESSION_WAIT_FOR_SYSTEM_BEHAVIOR_RETRY_WITH_USER_CANCEL, 1) \
    _(VA_SESSION_WAIT_FOR_SYSTEM_BEHAVIOR_RETRY_SILENTLY, 2)         \
    _(VA_SESSION_WAIT_FOR_SYSTEM_BEHAVIOR_NO_WAIT, 3)

#define LIST_ENUM_VaSpaceTypeExt(_)          \
    _(VA_SPACE_TYPE_VOLUME_CONTAINER_EXT, 1) \
    _(VA_SPACE_TYPE_VOLUME_CONTENT_EXT, 2)   \
    _(VA_SPACE_TYPE_VIEWER_EXT, 3)           \
    _(VA_SPACE_TYPE_LOCAL_EXT, 4)

#define LIST_ENUM_VaJointSetExt(_) _(VA_JOINT_SET_HAND_EXT, 1)

#define LIST_ENUM_VaSideExt(_) \
    _(VA_SIDE_NONE_EXT, 0)     \
    _(VA_SIDE_LEFT_EXT, 1)     \
    _(VA_SIDE_RIGHT_EXT, 2)

#define LIST_ENUM_VaHandJointExt(_)              \
    _(VA_HAND_JOINT_PALM_EXT, 0)                 \
    _(VA_HAND_JOINT_WRIST_EXT, 1)                \
    _(VA_HAND_JOINT_THUMB_METACARPAL_EXT, 2)     \
    _(VA_HAND_JOINT_THUMB_PROXIMAL_EXT, 3)       \
    _(VA_HAND_JOINT_THUMB_DISTAL_EXT, 4)         \
    _(VA_HAND_JOINT_THUMB_TIP_EXT, 5)            \
    _(VA_HAND_JOINT_INDEX_METACARPAL_EXT, 6)     \
    _(VA_HAND_JOINT_INDEX_PROXIMAL_EXT, 7)       \
    _(VA_HAND_JOINT_INDEX_INTERMEDIATE_EXT, 8)   \
    _(VA_HAND_JOINT_INDEX_DISTAL_EXT, 9)         \
    _(VA_HAND_JOINT_INDEX_TIP_EXT, 10)           \
    _(VA_HAND_JOINT_MIDDLE_METACARPAL_EXT, 11)   \
    _(VA_HAND_JOINT_MIDDLE_PROXIMAL_EXT, 12)     \
    _(VA_HAND_JOINT_MIDDLE_INTERMEDIATE_EXT, 13) \
    _(VA_HAND_JOINT_MIDDLE_DISTAL_EXT, 14)       \
    _(VA_HAND_JOINT_MIDDLE_TIP_EXT, 15)          \
    _(VA_HAND_JOINT_RING_METACARPAL_EXT, 16)     \
    _(VA_HAND_JOINT_RING_PROXIMAL_EXT, 17)       \
    _(VA_HAND_JOINT_RING_INTERMEDIATE_EXT, 18)   \
    _(VA_HAND_JOINT_RING_DISTAL_EXT, 19)         \
    _(VA_HAND_JOINT_RING_TIP_EXT, 20)            \
    _(VA_HAND_JOINT_LITTLE_METACARPAL_EXT, 21)   \
    _(VA_HAND_JOINT_LITTLE_PROXIMAL_EXT, 22)     \
    _(VA_HAND_JOINT_LITTLE_INTERMEDIATE_EXT, 23) \
    _(VA_HAND_JOINT_LITTLE_DISTAL_EXT, 24)       \
    _(VA_HAND_JOINT_LITTLE_TIP_EXT, 25)

#define LIST_ENUM_VaHandTrackingDataSourceExt(_)         \
    _(VA_HAND_TRACKING_DATA_SOURCE_UNAVAILABLE_EXT, 0)   \
    _(VA_HAND_TRACKING_DATA_SOURCE_HAND_TRACKING_EXT, 1) \
    _(VA_HAND_TRACKING_DATA_SOURCE_CONTROLLER_SIMULATED_EXT, 2)

#define LIST_ENUM_VaVolumeRestoreBehaviorExt(_)     \
    _(VA_VOLUME_RESTORE_BEHAVIOR_NO_RESTORE_EXT, 1) \
    _(VA_VOLUME_RESTORE_BEHAVIOR_BY_APP_EXT, 2)     \
    _(VA_VOLUME_RESTORE_BEHAVIOR_BY_PLATFORM_MULTIPLE_VOLUMES_EXT, 3)

#define LIST_ENUM_VaVolumeRestoredResultExt(_)              \
    _(VA_VOLUME_RESTORED_RESULT_SUCCESS_EXT, 0)             \
    _(VA_VOLUME_RESTORED_RESULT_INVALID_RESTORE_ID_EXT, -1) \
    _(VA_VOLUME_RESTORED_RESULT_RESTORE_FAILED_EXT, -2)

#define LIST_ENUM_VaMeshBufferTypeExt(_)            \
    _(VA_MESH_BUFFER_TYPE_INDEX_EXT, 1)             \
    _(VA_MESH_BUFFER_TYPE_VERTEX_POSITION_EXT, 2)   \
    _(VA_MESH_BUFFER_TYPE_VERTEX_NORMAL_EXT, 3)     \
    _(VA_MESH_BUFFER_TYPE_VERTEX_TANGENT_EXT, 4)    \
    _(VA_MESH_BUFFER_TYPE_VERTEX_COLOR_EXT, 5)      \
    _(VA_MESH_BUFFER_TYPE_VERTEX_TEXCOORD_0_EXT, 6) \
    _(VA_MESH_BUFFER_TYPE_VERTEX_TEXCOORD_1_EXT, 7)

#define LIST_ENUM_VaMeshBufferFormatExt(_) \
    _(VA_MESH_BUFFER_FORMAT_UINT16_EXT, 1) \
    _(VA_MESH_BUFFER_FORMAT_UINT32_EXT, 2) \
    _(VA_MESH_BUFFER_FORMAT_FLOAT_EXT, 3)  \
    _(VA_MESH_BUFFER_FORMAT_FLOAT2_EXT, 4) \
    _(VA_MESH_BUFFER_FORMAT_FLOAT3_EXT, 5) \
    _(VA_MESH_BUFFER_FORMAT_FLOAT4_EXT, 6)

#define LIST_ENUM_VaMaterialTypeExt(_) _(VA_MATERIAL_TYPE_PBR_EXT, 1)

#define LIST_ENUM_TYPES(_)            \
    _(VaObjectType)                   \
    _(VaResult)                       \
    _(VaStructureType)                \
    _(VaVolumeState)                  \
    _(VaVolumeStateAction)            \
    _(VaElementType)                  \
    _(VaElementProperty)              \
    _(VaVolumeUpdateMode)             \
    _(VaElementAsyncState)            \
    _(VaElementAsyncError)            \
    _(VaVolumeSizeBehavior)           \
    _(VaSessionWaitForSystemBehavior) \
    _(VaSpaceTypeExt)                 \
    _(VaJointSetExt)                  \
    _(VaSideExt)                      \
    _(VaHandJointExt)                 \
    _(VaHandTrackingDataSourceExt)    \
    _(VaVolumeRestoreBehaviorExt)     \
    _(VaVolumeRestoredResultExt)      \
    _(VaMeshBufferTypeExt)            \
    _(VaMeshBufferFormatExt)          \
    _(VaMaterialTypeExt)

#define LIST_STRUCTURE_TYPES_VA_CORE(_)                                           \
    _(VaSessionCreateInfo, VA_TYPE_SESSION_CREATE_INFO)                           \
    _(VaVolumeCreateInfo, VA_TYPE_VOLUME_CREATE_INFO)                             \
    _(VaElementCreateInfo, VA_TYPE_ELEMENT_CREATE_INFO)                           \
    _(VaExtensionProperties, VA_TYPE_EXTENSION_PROPERTIES)                        \
    _(VaEventDataBuffer, VA_TYPE_EVENT_DATA_BUFFER)                               \
    _(VaEventConnectedSystemChanged, VA_TYPE_EVENT_CONNECTED_SYSTEM_CHANGED)      \
    _(VaEventSessionStopped, VA_TYPE_EVENT_SESSION_STOPPED)                       \
    _(VaEventVolumeStateChanged, VA_TYPE_EVENT_VOLUME_STATE_CHANGED)              \
    _(VaEventUpdateVolume, VA_TYPE_EVENT_UPDATE_VOLUME)                           \
    _(VaUpdateVolumeRequestInfo, VA_TYPE_UPDATE_VOLUME_REQUEST_INFO)              \
    _(VaUpdateVolumeRequestResult, VA_TYPE_UPDATE_VOLUME_REQUEST_RESULT)          \
    _(VaUpdateVolumeBeginInfo, VA_TYPE_UPDATE_VOLUME_BEGIN_INFO)                  \
    _(VaUpdateVolumeEndInfo, VA_TYPE_UPDATE_VOLUME_END_INFO)                      \
    _(VaUpdateVolumeFrameState, VA_TYPE_UPDATE_VOLUME_FRAME_STATE)                \
    _(VaElementAsyncErrorData, VA_TYPE_ELEMENT_ASYNC_ERROR_DATA)                  \
    _(VaEventElementAsyncStateChanged, VA_TYPE_EVENT_ELEMENT_ASYNC_STATE_CHANGED) \
    _(VaChangedElementsGetInfo, VA_TYPE_CHANGED_ELEMENTS_GET_INFO)                \
    _(VaChangedElements, VA_TYPE_CHANGED_ELEMENTS)                                \
    _(VaEventWaitInfo, VA_TYPE_EVENT_WAIT_INFO)

#define LIST_STRUCTURE_TYPES_VA_EXT_locate_spaces(_)       \
    _(VaSpaceLocateInfoExt, VA_TYPE_SPACE_LOCATE_INFO_EXT) \
    _(VaSpaceLocationsExt, VA_TYPE_SPACE_LOCATIONS_EXT)

#define LIST_STRUCTURE_TYPES_VA_EXT_locate_joints(_)       \
    _(VaJointLocateInfoExt, VA_TYPE_JOINT_LOCATE_INFO_EXT) \
    _(VaJointLocationsExt, VA_TYPE_JOINT_LOCATIONS_EXT)

#define LIST_STRUCTURE_TYPES_VA_EXT_volume_restore(_)                                                       \
    _(VaSessionCreateWithVolumeRestoreBehaviorExt, VA_TYPE_SESSION_CREATE_WITH_VOLUME_RESTORE_BEHAVIOR_EXT) \
    _(VaEventVolumeRestoreRequestExt, VA_TYPE_EVENT_VOLUME_RESTORE_REQUEST_EXT)                             \
    _(VaEventVolumeRestoreResultExt, VA_TYPE_EVENT_VOLUME_RESTORE_RESULT_EXT)                               \
    _(VaEventVolumeRestoreIdInvalidatedExt, VA_TYPE_EVENT_VOLUME_RESTORE_ID_INVALIDATED_EXT)                \
    _(VaVolumeCreateWithRestoreConfigExt, VA_TYPE_VOLUME_CREATE_WITH_RESTORE_CONFIG_EXT)                    \
    _(VaVolumeCreateWithRestoreIdExt, VA_TYPE_VOLUME_CREATE_WITH_RESTORE_ID_EXT)

#define LIST_STRUCTURE_TYPES_VA_EXT_gltf2_model_resource(_) _(VaGltf2MeshResourceIndexInfoExt, VA_TYPE_GLTF2_MESH_RESOURCE_INDEX_INFO_EXT)

#define LIST_STRUCTURE_TYPES_VA_EXT_adaptive_card_element(_)                               \
    _(VaEventAdaptiveCardActionInvokedExt, VA_TYPE_EVENT_ADAPTIVE_CARD_ACTION_INVOKED_EXT) \
    _(VaAdaptiveCardActionInvokedDataExt, VA_TYPE_ADAPTIVE_CARD_ACTION_INVOKED_DATA_EXT)

#define LIST_STRUCTURE_TYPES_VA_EXT_mesh_edit(_)                                     \
    _(VaMeshResourceInitBuffersInfoExt, VA_TYPE_MESH_RESOURCE_INIT_BUFFERS_INFO_EXT) \
    _(VaMeshBufferAcquireInfoExt, VA_TYPE_MESH_BUFFER_ACQUIRE_INFO_EXT)              \
    _(VaMeshBufferResizeInfoExt, VA_TYPE_MESH_BUFFER_RESIZE_INFO_EXT)                \
    _(VaMeshBufferAcquireResultExt, VA_TYPE_MESH_BUFFER_ACQUIRE_RESULT_EXT)          \
    _(VaMeshBufferReleaseInfoExt, VA_TYPE_MESH_BUFFER_RELEASE_INFO_EXT)

#define LIST_STRUCTURE_TYPES_VA_EXT_material_resource(_)

#define LIST_STRUCTURE_TYPES_VA_EXT_volume_content_container(_)

#define LIST_STRUCTURE_TYPES_VA_EXT_texture_resource(_)

#define LIST_STRUCTURE_TYPES_VA_EXT_volume_container_modes(_) _(VaEventVolumeContainerModeChangedExt, VA_TYPE_EVENT_VOLUME_CONTAINER_MODE_CHANGED_EXT)

#define LIST_STRUCTURE_TYPES_VA_EXT_volume_container_thumbnail(_)

// clang-format off
#define LIST_STRUCTURE_TYPES_VA_EXTENSIONS(_) \
    LIST_STRUCTURE_TYPES_VA_EXT_locate_spaces(_)   \
    LIST_STRUCTURE_TYPES_VA_EXT_locate_joints(_)   \
    LIST_STRUCTURE_TYPES_VA_EXT_volume_restore(_)   \
    LIST_STRUCTURE_TYPES_VA_EXT_gltf2_model_resource(_)   \
    LIST_STRUCTURE_TYPES_VA_EXT_adaptive_card_element(_)   \
    LIST_STRUCTURE_TYPES_VA_EXT_mesh_edit(_)   \
    LIST_STRUCTURE_TYPES_VA_EXT_material_resource(_)   \
    LIST_STRUCTURE_TYPES_VA_EXT_volume_content_container(_)   \
    LIST_STRUCTURE_TYPES_VA_EXT_texture_resource(_)   \
    LIST_STRUCTURE_TYPES_VA_EXT_volume_container_modes(_)   \
    LIST_STRUCTURE_TYPES_VA_EXT_volume_container_thumbnail(_)
// clang-format on

#define LIST_EXTENSIONS(_)                \
    _(VA_EXT_locate_spaces, 3)            \
    _(VA_EXT_locate_joints, 1)            \
    _(VA_EXT_volume_restore, 2)           \
    _(VA_EXT_gltf2_model_resource, 1)     \
    _(VA_EXT_adaptive_card_element, 1)    \
    _(VA_EXT_mesh_edit, 1)                \
    _(VA_EXT_material_resource, 2)        \
    _(VA_EXT_volume_content_container, 1) \
    _(VA_EXT_texture_resource, 1)         \
    _(VA_EXT_volume_container_modes, 1)   \
    _(VA_EXT_volume_container_thumbnail, 1)

#define LIST_FUNCTIONS_VA_CORE(_)      \
    _(vaGetFunctionPointer)            \
    _(vaEnumerateExtensions)           \
    _(vaCreateSession)                 \
    _(vaDestroySession)                \
    _(vaRequestStopSession)            \
    _(vaPollEvent)                     \
    _(vaWaitForNextEvent)              \
    _(vaWaitEvent)                     \
    _(vaCreateVolume)                  \
    _(vaDestroyVolume)                 \
    _(vaRequestCloseVolume)            \
    _(vaRequestUpdateVolume)           \
    _(vaBeginUpdateVolume)             \
    _(vaEndUpdateVolume)               \
    _(vaCreateElement)                 \
    _(vaDestroyElement)                \
    _(vaGetElementPropertyBool)        \
    _(vaGetElementPropertyEnum)        \
    _(vaGetElementPropertyFloat)       \
    _(vaGetElementPropertyVector3f)    \
    _(vaGetElementPropertyQuaternionf) \
    _(vaGetElementPropertyExtent3Df)   \
    _(vaSetElementPropertyBool)        \
    _(vaSetElementPropertyEnum)        \
    _(vaSetElementPropertyFlags)       \
    _(vaSetElementPropertyString)      \
    _(vaSetElementPropertyUInt32)      \
    _(vaSetElementPropertyInt32)       \
    _(vaSetElementPropertyHandle)      \
    _(vaSetElementPropertyFloat)       \
    _(vaSetElementPropertyVector3f)    \
    _(vaSetElementPropertyColor3f)     \
    _(vaSetElementPropertyColor4f)     \
    _(vaSetElementPropertyQuaternionf) \
    _(vaSetElementPropertyExtent3Df)   \
    _(vaGetNextElementAsyncError)      \
    _(vaGetChangedElements)

#define LIST_FUNCTIONS_VA_EXT_locate_spaces(_) _(vaLocateSpacesExt)

#define LIST_FUNCTIONS_VA_EXT_locate_joints(_) _(vaLocateJointsExt)

#define LIST_FUNCTIONS_VA_EXT_volume_restore(_) \
    _(vaGetVolumeRestoreIdExt)                  \
    _(vaRemoveRestorableVolumeExt)

#define LIST_FUNCTIONS_VA_EXT_gltf2_model_resource(_)

#define LIST_FUNCTIONS_VA_EXT_adaptive_card_element(_) _(vaGetNextAdaptiveCardActionInvokedDataExt)

#define LIST_FUNCTIONS_VA_EXT_mesh_edit(_) \
    _(vaAcquireMeshBufferExt)              \
    _(vaReleaseMeshBufferExt)

#define LIST_FUNCTIONS_VA_EXT_material_resource(_)

#define LIST_FUNCTIONS_VA_EXT_volume_content_container(_)

#define LIST_FUNCTIONS_VA_EXT_texture_resource(_)

#define LIST_FUNCTIONS_VA_EXT_volume_container_modes(_)

#define LIST_FUNCTIONS_VA_EXT_volume_container_thumbnail(_)

#ifdef VA_EXT_LOCATE_SPACES
#define VA_EXT_LOCATE_SPACES_DEFINED(_, defined, undefined) _(defined)
#else
#define VA_EXT_LOCATE_SPACES_DEFINED(_, defined, undefined) _(undefined)
#endif

#ifdef VA_EXT_LOCATE_JOINTS
#define VA_EXT_LOCATE_JOINTS_DEFINED(_, defined, undefined) _(defined)
#else
#define VA_EXT_LOCATE_JOINTS_DEFINED(_, defined, undefined) _(undefined)
#endif

#ifdef VA_EXT_VOLUME_RESTORE
#define VA_EXT_VOLUME_RESTORE_DEFINED(_, defined, undefined) _(defined)
#else
#define VA_EXT_VOLUME_RESTORE_DEFINED(_, defined, undefined) _(undefined)
#endif

#ifdef VA_EXT_GLTF2_MODEL_RESOURCE
#define VA_EXT_GLTF2_MODEL_RESOURCE_DEFINED(_, defined, undefined) _(defined)
#else
#define VA_EXT_GLTF2_MODEL_RESOURCE_DEFINED(_, defined, undefined) _(undefined)
#endif

#ifdef VA_EXT_ADAPTIVE_CARD_ELEMENT
#define VA_EXT_ADAPTIVE_CARD_ELEMENT_DEFINED(_, defined, undefined) _(defined)
#else
#define VA_EXT_ADAPTIVE_CARD_ELEMENT_DEFINED(_, defined, undefined) _(undefined)
#endif

#ifdef VA_EXT_MESH_EDIT
#define VA_EXT_MESH_EDIT_DEFINED(_, defined, undefined) _(defined)
#else
#define VA_EXT_MESH_EDIT_DEFINED(_, defined, undefined) _(undefined)
#endif

#ifdef VA_EXT_MATERIAL_RESOURCE
#define VA_EXT_MATERIAL_RESOURCE_DEFINED(_, defined, undefined) _(defined)
#else
#define VA_EXT_MATERIAL_RESOURCE_DEFINED(_, defined, undefined) _(undefined)
#endif

#ifdef VA_EXT_VOLUME_CONTENT_CONTAINER
#define VA_EXT_VOLUME_CONTENT_CONTAINER_DEFINED(_, defined, undefined) _(defined)
#else
#define VA_EXT_VOLUME_CONTENT_CONTAINER_DEFINED(_, defined, undefined) _(undefined)
#endif

#ifdef VA_EXT_TEXTURE_RESOURCE
#define VA_EXT_TEXTURE_RESOURCE_DEFINED(_, defined, undefined) _(defined)
#else
#define VA_EXT_TEXTURE_RESOURCE_DEFINED(_, defined, undefined) _(undefined)
#endif

#ifdef VA_EXT_VOLUME_CONTAINER_MODES
#define VA_EXT_VOLUME_CONTAINER_MODES_DEFINED(_, defined, undefined) _(defined)
#else
#define VA_EXT_VOLUME_CONTAINER_MODES_DEFINED(_, defined, undefined) _(undefined)
#endif

#ifdef VA_EXT_VOLUME_CONTAINER_THUMBNAIL
#define VA_EXT_VOLUME_CONTAINER_THUMBNAIL_DEFINED(_, defined, undefined) _(defined)
#else
#define VA_EXT_VOLUME_CONTAINER_THUMBNAIL_DEFINED(_, defined, undefined) _(undefined)
#endif

#define LIST_FUNCTIONS_VA_EXTENSIONS(_, __)                                                        \
    VA_EXT_LOCATE_SPACES_DEFINED(LIST_FUNCTIONS_VA_EXT_locate_spaces, _, __)                       \
    VA_EXT_LOCATE_JOINTS_DEFINED(LIST_FUNCTIONS_VA_EXT_locate_joints, _, __)                       \
    VA_EXT_VOLUME_RESTORE_DEFINED(LIST_FUNCTIONS_VA_EXT_volume_restore, _, __)                     \
    VA_EXT_GLTF2_MODEL_RESOURCE_DEFINED(LIST_FUNCTIONS_VA_EXT_gltf2_model_resource, _, __)         \
    VA_EXT_ADAPTIVE_CARD_ELEMENT_DEFINED(LIST_FUNCTIONS_VA_EXT_adaptive_card_element, _, __)       \
    VA_EXT_MESH_EDIT_DEFINED(LIST_FUNCTIONS_VA_EXT_mesh_edit, _, __)                               \
    VA_EXT_MATERIAL_RESOURCE_DEFINED(LIST_FUNCTIONS_VA_EXT_material_resource, _, __)               \
    VA_EXT_VOLUME_CONTENT_CONTAINER_DEFINED(LIST_FUNCTIONS_VA_EXT_volume_content_container, _, __) \
    VA_EXT_TEXTURE_RESOURCE_DEFINED(LIST_FUNCTIONS_VA_EXT_texture_resource, _, __)                 \
    VA_EXT_VOLUME_CONTAINER_MODES_DEFINED(LIST_FUNCTIONS_VA_EXT_volume_container_modes, _, __)     \
    VA_EXT_VOLUME_CONTAINER_THUMBNAIL_DEFINED(LIST_FUNCTIONS_VA_EXT_volume_container_thumbnail, _, __)
#endif // VOLUMETRIC_REFLECTION_H
