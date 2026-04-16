#pragma once

#include <vector>
#include <chrono>
#include <memory>
#include "MeshData.h"

namespace sample {

    struct MeshGenerator {
    public:
        MeshGenerator(uint32_t defaultVertexCountPerSide, bool defaultDoubleSided);
        virtual ~MeshGenerator() = default;

        // Generate mesh data on given data struct and return as mesh data reference.
        MeshDataRef GenerateMesh(MeshData& data, float timeInSeconds);

        // Update mesh data on existing mesh data reference, using the given time as animation tick.
        void UpdateMeshData(struct MeshDataRef& meshRef, float timeInSeconds, bool initData);

        virtual uint32_t VertexCountPerSide() const {
            return m_vertexCountPerSide;
        }

        virtual void SetVertexCountPerSide(uint32_t vertexCountPerSide) {
            if (m_vertexCountPerSide != vertexCountPerSide) {
                m_vertexCountPerSide = vertexCountPerSide;
            }
        }

        virtual void SetTotalVertexCount(uint32_t) {
            throw std::runtime_error("SetTotalVertexCount is not implemented for this mesh generator.");
        }

        void GetMeshBufferSize(uint32_t& indexCount, uint32_t& vertexCount) const {
            EstimateIndexAndVertexCount(indexCount, vertexCount);
        }

        bool IsDoubleSided() const {
            return m_doubleSided;
        }

        void SetDoubleSided(bool doubleSided) {
            m_doubleSided = doubleSided;
        }

    protected:
        virtual void InitializeMeshData(MeshData& data) = 0;
        virtual void ModifyMeshData(MeshDataRef& meshRef, float timeInSeconds) = 0;
        virtual void EstimateIndexAndVertexCount(uint32_t& indexCount, uint32_t& vertexCount) const = 0;

    private:
        void PrepareBufferAfterResize(MeshDataRef& meshRef);

    private:
        const std::chrono::steady_clock::time_point m_startTime = std::chrono::steady_clock::now();
        uint32_t m_vertexCountPerSide = 100;
        bool m_doubleSided = false;
    };

    std::unique_ptr<MeshGenerator> CreateWaveMeshGenerator();
    std::unique_ptr<MeshGenerator> CreatePulseMeshGenerator();
    std::unique_ptr<MeshGenerator> CreatePulseGridGenerator();

} // namespace sample
