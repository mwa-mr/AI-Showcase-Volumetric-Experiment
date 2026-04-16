#
#  Copyright (c) Microsoft Corporation.  All rights reserved.
#

set(TARGET cpp_sample_shared)

add_library(${TARGET} STATIC
    SampleMesh/GlbEmitter.cpp
    SampleMesh/GlbEmitter.h
    SampleMesh/MeshData.h
    SampleMesh/MeshGenerator.cpp
    SampleMesh/MeshGenerator.h
    SampleMesh/PulseImpactSphere.cpp
    SampleMesh/WaveformMesh.cpp
    SampleMesh/PulseGrid.cpp

    SampleMathHelpers.h

    SampleTraceHelpers.h
    SampleTraceHelpers.cpp
)

target_include_directories(${TARGET} PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})

target_link_libraries(
    ${TARGET}
        VolumetricSamples::Externals
        VolumetricSDK::VaAbi
        VolumetricSDK::VaUtility
)

add_library(${PROJECT_NAME}::${TARGET} ALIAS ${TARGET})
