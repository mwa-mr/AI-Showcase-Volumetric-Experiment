#
#  Copyright (c) Microsoft Corporation.  All rights reserved.
#

set(TARGET cpp_multiple_volumes)

add_executable(
    ${TARGET}
        main.cpp
        ${VOLUMETRIC_SDK_ASSETS_DIR}/Duck.glb
        ${VOLUMETRIC_SDK_ASSETS_DIR}/BoomBox.glb
)

target_link_libraries(
    ${TARGET}
        VolumetricSDK::VolumetricCppLibrary
)

add_file_to_target(${TARGET} ${VOLUMETRIC_SDK_ASSETS_DIR}/Duck.glb)
add_file_to_target(${TARGET} ${VOLUMETRIC_SDK_ASSETS_DIR}/BoomBox.glb)
add_file_to_target(${TARGET} ${VOLUMETRIC_SDK_ASSETS_DIR}/axis_xyz_rub.glb)

