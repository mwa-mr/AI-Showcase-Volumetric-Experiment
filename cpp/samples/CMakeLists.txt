#
#  Copyright (c) Microsoft Corporation.  All rights reserved.
#

set(VOLUMETRIC_SDK_ASSETS_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../../assets")

function(add_file_to_target targetName source)
    cmake_parse_arguments(A "" "DESTINATION" "" "${ARGN}")
    if (DEFINED A_DESTINATION) # If DESTINATION is provided, use it
        set(destination "${A_DESTINATION}")
    else() # Otherwise, use the source file name
        get_filename_component(destination ${source} NAME)
    endif()

    # A relative path is treated with respect to the value of CMAKE_CURRENT_BINARY_DIR
    configure_file("${source}" "${destination}" COPYONLY)
endfunction()

add_subdirectory(Clock)
add_subdirectory(Externals)
add_subdirectory(HelloWorld)
add_subdirectory(MeshEdit)
add_subdirectory(ModelViewer)
add_subdirectory(MultipleVolumes)
add_subdirectory(NamedNodes)
add_subdirectory(SampleShared)
add_subdirectory(SpatialInputs)
add_subdirectory(SpinningCube)
