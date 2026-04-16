#
#  Copyright (c) Microsoft Corporation.  All rights reserved.
#

set(TARGET cpp_mesh_edit)

add_executable(
    ${TARGET}
        main.cpp
        ${VOLUMETRIC_SDK_ASSETS_DIR}/Simple.gltf
)

target_include_directories(
    ${TARGET}
    PRIVATE
        ${CMAKE_SOURCE_DIR}
)

target_link_libraries(
    ${TARGET}
        VolumetricSDK::VolumetricCppLibrary
        VolumetricSamples::cpp_sample_shared
)

add_file_to_target(${TARGET} ${VOLUMETRIC_SDK_ASSETS_DIR}/Simple.gltf)

