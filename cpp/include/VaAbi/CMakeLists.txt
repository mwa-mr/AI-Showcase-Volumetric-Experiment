#
#  Copyright (c) Microsoft Corporation.  All rights reserved.
#

project(VolumetricSDK LANGUAGES C CXX)

set(TARGET VaAbi)

add_library(
    ${TARGET}
    INTERFACE
        volumetric/volumetric.h
        volumetric/volumetric_loader_default.h
        volumetric/volumetric_loader_interface.h
        volumetric/volumetric_platform_defines.h
        volumetric/volumetric_reflection.h
)

target_include_directories(
    ${TARGET}
    INTERFACE
        ${CMAKE_CURRENT_SOURCE_DIR}
)

add_library(${PROJECT_NAME}::${TARGET} ALIAS ${TARGET})
