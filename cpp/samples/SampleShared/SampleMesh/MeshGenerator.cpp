#include <volumetric/volumetric.h>
#include <vaError.h>
#include "MeshGenerator.h"

namespace sample {
    MeshGenerator::MeshGenerator(uint32_t defaultVertexCountPerSide, bool defaultDoubleSided)
        : m_vertexCountPerSide(defaultVertexCountPerSide)
        , m_doubleSided(defaultDoubleSided) {}

    MeshDataRef MeshGenerator::GenerateMesh(MeshData& data, float timeInSeconds) {
        InitializeMeshData(data);
        MeshDataRef meshRef = CreateMeshDataReference(data);
        ModifyMeshData(meshRef, timeInSeconds);
        return meshRef;
    }

    void MeshGenerator::UpdateMeshData(MeshDataRef& meshRef, float timeInSeconds, bool initData) {
        uint32_t indexCount, vertexCount;
        EstimateIndexAndVertexCount(indexCount, vertexCount);
        if (meshRef.indexCount != indexCount || meshRef.vertexCount != vertexCount) {
            TRACE("ERROR: Mesh data size mismatch: expected %d indices and %d vertices, actually get %d and %d instead.",
                  indexCount,
                  vertexCount,
                  meshRef.indexCount,
                  meshRef.vertexCount);
            return;
        }

        if (initData) {
            PrepareBufferAfterResize(meshRef);
        }

        using namespace std::chrono;
        milliseconds ms = duration_cast<milliseconds>(steady_clock::now() - m_startTime);
        ModifyMeshData(meshRef, ms.count() / 1000.0f + timeInSeconds);
    }

    void MeshGenerator::PrepareBufferAfterResize(MeshDataRef& meshRef) {
        // For this sample, we simply reinitialize the mesh data
        // by creating a new copy of data and copy into the given meshRef.
        MeshData data;
        InitializeMeshData(data);

        std::copy(data.indexBuffer.begin(), data.indexBuffer.end(), meshRef.indexBuffer.begin());
        std::copy(data.positionBuffer.begin(), data.positionBuffer.end(), meshRef.positionBuffer.begin());
        std::copy(data.normalBuffer.begin(), data.normalBuffer.end(), meshRef.normalBuffer.begin());
        if (meshRef.tangentBuffer.size() > 0) { // tangent buffer is optional
            std::copy(data.tangentBuffer.begin(), data.tangentBuffer.end(), meshRef.tangentBuffer.begin());
        }
        if (meshRef.colorBuffer.size() > 0) { // color buffer is optional
            std::copy(data.colorBuffer.begin(), data.colorBuffer.end(), meshRef.colorBuffer.begin());
        }
        if (meshRef.uvBuffer.size() > 0) { // uv buffer is optional
            std::copy(data.uvBuffer.begin(), data.uvBuffer.end(), meshRef.uvBuffer.begin());
        }
    }
} // namespace sample
