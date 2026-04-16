#
#  Copyright (c) Microsoft Corporation.  All rights reserved.
#

set(TARGET cpp_clock)

add_executable(
    ${TARGET}
        WIN32
        main.cpp
        ${VOLUMETRIC_SDK_ASSETS_DIR}/clock.glb
        ${VOLUMETRIC_SDK_ASSETS_DIR}/ClockThumbnail.glb
        ${VOLUMETRIC_SDK_ASSETS_DIR}/ClockThumbnail.png
)

target_link_libraries(
    ${TARGET}
        VolumetricSDK::VolumetricCppLibrary
)

add_file_to_target(${TARGET} ${VOLUMETRIC_SDK_ASSETS_DIR}/clock.glb)
add_file_to_target(${TARGET} ${VOLUMETRIC_SDK_ASSETS_DIR}/ClockThumbnail.glb)
add_file_to_target(${TARGET} ${VOLUMETRIC_SDK_ASSETS_DIR}/ClockThumbnail.png)

