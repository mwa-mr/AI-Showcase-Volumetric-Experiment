#
#  Copyright (c) Microsoft Corporation.  All rights reserved.
#


set(TARGET_NAME ${PROJECT_NAME}::Externals)
add_library(${TARGET_NAME} INTERFACE IMPORTED GLOBAL)
target_include_directories(${TARGET_NAME} INTERFACE
    ${CMAKE_CURRENT_SOURCE_DIR}
    ${CMAKE_CURRENT_SOURCE_DIR}/stb
)
