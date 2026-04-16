#pragma once

#include <SampleMesh/MeshData.h>
#include <SampleMesh/MeshGenerator.h>

std::string GenerateTempGLBFilePath();
void EmitGLB(const std::vector<sample::MeshDataRef>& meshRefs, const std::string& outputPath, bool doubleSided, bool shareAttributeAccessors = false);
