#
#  Copyright (c) Microsoft Corporation.  All rights reserved.
#

set(TARGET cpp_spinning_cube)

# This sample demos compatibility with c++14 standard
set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

add_executable(
    ${TARGET}
        main.cpp
        ${VOLUMETRIC_SDK_ASSETS_DIR}/BoxTextured.glb
)

target_link_libraries(
    ${TARGET}
        VolumetricSDK::VolumetricCppLibrary
)

add_file_to_target(${TARGET} ${VOLUMETRIC_SDK_ASSETS_DIR}/BoxTextured.glb)

