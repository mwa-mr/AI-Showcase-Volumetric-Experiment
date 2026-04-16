#
#  Copyright (c) Microsoft Corporation.  All rights reserved.
#

set(TARGET cpp_hello_world)

add_executable(
    ${TARGET}
        main.cpp
        ${VOLUMETRIC_SDK_ASSETS_DIR}/world.glb
)

target_link_libraries(
    ${TARGET}
        VolumetricSDK::VolumetricCppLibrary
)

add_file_to_target(${TARGET} ${VOLUMETRIC_SDK_ASSETS_DIR}/world.glb)

