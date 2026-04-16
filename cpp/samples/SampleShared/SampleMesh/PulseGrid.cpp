#include <cmath>
#include <algorithm>
#include "MeshGenerator.h"

namespace {
    using namespace sample;

    // Generate an arbitrary-sized grid in the XY plane
    void GenerateGridMesh(MeshData& data, uint32_t width, uint32_t height) {
        data.vertexCount = width * height;
        data.indexCount = (width - 1u) * (height - 1u) * 6u;

        data.positionBuffer.resize(data.vertexCount);
        data.normalBuffer.resize(data.vertexCount);
        data.tangentBuffer.resize(data.vertexCount);
        data.colorBuffer.resize(data.vertexCount);
        data.uvBuffer.resize(data.vertexCount);

        for (uint32_t y = 0u; y < height; ++y) {
            for (uint32_t x = 0u; x < width; ++x) {
                const uint32_t idx = y * width + x;
                const float fx = static_cast<float>(x) / static_cast<float>(width - 1u) - 0.5f;
                const float fy = static_cast<float>(y) / static_cast<float>(height - 1u) - 0.5f;

                data.positionBuffer[idx] = {fx, fy, 0.0f};
                data.normalBuffer[idx] = {0.0f, 0.0f, 1.0f};
                data.tangentBuffer[idx] = {1.0f, 0.0f, 0.0f, 1.0f};
                data.colorBuffer[idx] = {1.0f, 1.0f, 1.0f, 1.0f};
                data.uvBuffer[idx] = {fx + 0.5f, fy + 0.5f};
            }
        }

        data.indexBuffer.clear();
        data.indexBuffer.reserve(data.indexCount);

        for (uint32_t y = 0u; y < height - 1u; ++y) {
            for (uint32_t x = 0u; x < width - 1u; ++x) {
                const uint32_t bl = y * width + x;
                const uint32_t br = bl + 1u;
                const uint32_t tl = (y + 1u) * width + x;
                const uint32_t tr = tl + 1u;

                data.indexBuffer.push_back(bl);
                data.indexBuffer.push_back(br);
                data.indexBuffer.push_back(tl);

                data.indexBuffer.push_back(br);
                data.indexBuffer.push_back(tr);
                data.indexBuffer.push_back(tl);
            }
        }
    }

    class PulseGridGenerator final : public MeshGenerator {
    public:
        explicit PulseGridGenerator(uint32_t totalVertexCount)
            : MeshGenerator(1u, false /*doubleSided*/)
            , m_totalVertexCount(totalVertexCount) {}

    private:
        void InitializeMeshData(MeshData& data) override {
            const std::pair<uint32_t, uint32_t> dim = ComputeDimensions();
            GenerateGridMesh(data, dim.first, dim.second);
        }

        void EstimateIndexAndVertexCount(uint32_t& out_IndexCount, uint32_t& out_VertexCount) const override {
            const std::pair<uint32_t, uint32_t> dim = ComputeDimensions();
            out_VertexCount = dim.first * dim.second;
            out_IndexCount = (dim.first - 1u) * (dim.second - 1u) * 6u;
        }

        void ModifyMeshData(MeshDataRef& meshRef, float timeInSeconds) override {
            const std::pair<uint32_t, uint32_t> dim = ComputeDimensions();
            const uint32_t width = dim.first;
            const uint32_t height = dim.second;
            const float maxRadius = std::min(width, height) * 0.5f / static_cast<float>(std::max(width, height) - 1u);

            constexpr float amplitude = 0.20f;
            constexpr float frequency = 4.0f;

            const uint32_t count = static_cast<uint32_t>(meshRef.positionBuffer.size());

            for (uint32_t i = 0u; i < count; ++i) {
                const float vx = meshRef.positionBuffer[i].x;
                const float vy = meshRef.positionBuffer[i].y;

                const float distNorm = std::sqrt(vx * vx + vy * vy) / maxRadius;

                const float disp = std::sin(distNorm * frequency + timeInSeconds) * amplitude * (1.0f - distNorm);

                meshRef.positionBuffer[i].z = disp;
                meshRef.normalBuffer[i] = {0.0f, 0.0f, 1.0f};

                if (!meshRef.tangentBuffer.empty()) {
                    meshRef.tangentBuffer[i] = {1.0f, 0.0f, 0.0f, 1.0f};
                }

                if (!meshRef.colorBuffer.empty()) {
                    const float c = (disp + amplitude) / (2.0f * amplitude);
                    meshRef.colorBuffer[i] = {c, 1.0f - c, 1.0f, 1.0f};
                }
            }
        }

        void SetVertexCountPerSide(uint32_t) {
            throw std::runtime_error("PulseGridGenerator does not support SetVertexCountPerSide.");
        }

        uint32_t VertexCountPerSide() const override {
            throw std::runtime_error("PulseGridGenerator does not support VertexCountPerSide.");
        }

        void SetTotalVertexCount(uint32_t totalVertexCount) override {
            if (m_totalVertexCount != totalVertexCount) {
                m_totalVertexCount = totalVertexCount;
            }
        }

        [[nodiscard]] std::pair<uint32_t, uint32_t> ComputeDimensions() const {
            const uint32_t width = static_cast<uint32_t>(std::sqrt(static_cast<float>(m_totalVertexCount)));
            const uint32_t height = (m_totalVertexCount + width - 1u) / width;
            return {width, height};
        }

        uint32_t m_totalVertexCount = 0u;
    };
} // namespace

namespace sample {
    std::unique_ptr<MeshGenerator> CreatePulseGridGenerator() {
        constexpr uint32_t defaultVertexCount = 10'000u;
        return std::make_unique<PulseGridGenerator>(defaultVertexCount);
    }
} // namespace sample
