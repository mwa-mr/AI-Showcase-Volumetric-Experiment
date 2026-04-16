#
#  Copyright (c) Microsoft Corporation.  All rights reserved.
#

set(TARGET cpp_model_viewer)

set(MODEL_FILES
    ${VOLUMETRIC_SDK_ASSETS_DIR}/brass_vase.glb
    ${VOLUMETRIC_SDK_ASSETS_DIR}/Duck.glb
    ${VOLUMETRIC_SDK_ASSETS_DIR}/TeaSet.glb
    ${VOLUMETRIC_SDK_ASSETS_DIR}/Chess_Set.glb
)

add_executable(
    ${TARGET} WIN32
        main.cpp
        FileCollection.h
        FileCollection.cpp
        ${MODEL_FILES}
        ${VOLUMETRIC_SDK_ASSETS_DIR}/ModelViewerThumbnail.png
)

target_link_libraries(
    ${TARGET}
        VolumetricSDK::VolumetricCppLibrary
        Userenv.lib
)

foreach(MODEL_FILE ${MODEL_FILES})
    get_filename_component(MODEL_NAME ${MODEL_FILE} NAME_WE)
    add_file_to_target(${TARGET} ${MODEL_FILE} DESTINATION models/${MODEL_NAME}.glb)
endforeach()

add_file_to_target(${TARGET} ${VOLUMETRIC_SDK_ASSETS_DIR}/ModelViewerThumbnail.png)

