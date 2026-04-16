#
#  Copyright (c) Microsoft Corporation.  All rights reserved.
#

set(TARGET VaUtility)

add_library(
    ${TARGET}
    INTERFACE
        vaCtor.h
        vaEnums.h
        vaError.h
        vaExtensions.h
        vaFlags.h
        vaFunctions.h
        vaMath.h
        vaString.h
        vaUuid.h
        vaVersion.h
        vaWindows.h
)

target_include_directories(
    ${TARGET}
    INTERFACE
        ${CMAKE_CURRENT_SOURCE_DIR}
)

add_library(${PROJECT_NAME}::${TARGET} ALIAS ${TARGET})