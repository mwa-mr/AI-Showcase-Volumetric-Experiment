#
#  Copyright (c) Microsoft Corporation.  All rights reserved.
#

set(TARGET VolumetricCppLibrary)

add_library(
    ${TARGET}
    INTERFACE
        VolumetricApp.h

        api/vaApp.h
        api/vaElement.h
        api/vaInput.h
        api/vaVolume.h

        detail/components/vaAppContext.h
        detail/components/vaElementList.h

        detail/elements/vaAdaptiveCard.inl
        detail/elements/vaElement.inl
        detail/elements/vaInput.inl
        detail/elements/vaMaterialResource.inl
        detail/elements/vaMeshResource.inl
        detail/elements/vaModelResource.inl
        detail/elements/vaTextureResource.inl
        detail/elements/vaVisualElement.inl

        detail/vaApp.inl
        detail/vaSession.h
        detail/vaSession.inl
        detail/vaVolume.inl
)

target_include_directories(
    ${TARGET}
    INTERFACE
        ${CMAKE_CURRENT_SOURCE_DIR}
)

target_link_libraries(
    ${TARGET}
    INTERFACE
        VolumetricSDK::VaAbi
        VolumetricSDK::VaUtility
)

add_library(${PROJECT_NAME}::${TARGET} ALIAS ${TARGET})
