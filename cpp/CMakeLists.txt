#
#  Copyright (c) Microsoft Corporation.  All rights reserved.
#

cmake_minimum_required(VERSION 3.26)

project(VolumetricSamples LANGUAGES C CXX)

set(CMAKE_C_STANDARD 99)
set(CMAKE_C_STANDARD_REQUIRED ON)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

add_subdirectory(include)
add_subdirectory(samples)
