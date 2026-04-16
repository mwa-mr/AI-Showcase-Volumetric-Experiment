#pragma once

#include <vector>
#include <span>
#include <volumetric/volumetric.h>

namespace sample {

    /// <summary>
    /// This structure is used to store the mesh data that is used to generate the GLTF file.
    /// </summary>
    struct MeshData {
        uint32_t vertexCount = 0;
        uint32_t indexCount = 0;
        std::vector<uint32_t> indexBuffer;
        std::vector<VaVector3f> positionBuffer;
        std::vector<VaVector3f> normalBuffer;
        std::vector<VaVector4f> tangentBuffer;
        std::vector<VaColor4f> colorBuffer;
        std::vector<VaVector2f> uvBuffer;
    };

    /// <summary>
    /// This structure is used to reference the mesh data from another memory location.
    /// </summary>
    struct MeshDataRef {
        uint32_t vertexCount = 0;
        uint32_t indexCount = 0;
        std::span<uint32_t> indexBuffer;
        std::span<VaVector3f> positionBuffer;
        std::span<VaVector3f> normalBuffer;
        std::span<VaVector4f> tangentBuffer;
        std::span<VaColor4f> colorBuffer;
        std::span<VaVector2f> uvBuffer;
    };

    inline MeshDataRef CreateMeshDataReference(MeshData& data) {
        MeshDataRef meshRef{};
        meshRef.indexCount = data.indexCount;
        meshRef.vertexCount = data.vertexCount;
        meshRef.indexBuffer = std::span<uint32_t>(data.indexBuffer);
        meshRef.positionBuffer = std::span<VaVector3f>(data.positionBuffer);
        meshRef.normalBuffer = std::span<VaVector3f>(data.normalBuffer);
        meshRef.tangentBuffer = std::span<VaVector4f>(data.tangentBuffer);
        meshRef.colorBuffer = std::span<VaColor4f>(data.colorBuffer);
        meshRef.uvBuffer = std::span<VaVector2f>(data.uvBuffer);
        return meshRef;
    }
} // namespace sample
