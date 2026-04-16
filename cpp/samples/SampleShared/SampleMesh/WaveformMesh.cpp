#include "MeshGenerator.h"

namespace {
    using namespace sample;

    // Generate a square mesh patch om the XY plane
    void GenerateSquareMesh(MeshData& data, uint32_t size) {
        data.vertexCount = size * size;
        data.indexCount = (size - 1) * (size - 1) * 6;

        // Generate vertices
        data.positionBuffer.resize(size * size);
        data.normalBuffer.resize(size * size);
        data.tangentBuffer.resize(size * size);
        data.colorBuffer.resize(size * size);
        data.uvBuffer.resize(size * size);
        for (uint32_t y = 0; y < size; ++y) {
            for (uint32_t x = 0; x < size; ++x) {
                data.positionBuffer[y * size + x] = {static_cast<float>(x) / (size - 1), static_cast<float>(y) / (size - 1), 0.0f};
                data.normalBuffer[y * size + x] = {0.0f, 0.0f, 1.0f};
                data.tangentBuffer[y * size + x] = {1.0f, 0.0f, 0.0f, 1.0f};
                data.colorBuffer[y * size + x] = {(float)x / size, (float)y / size, 1.0f, 1.0f};
                data.uvBuffer[y * size + x] = {static_cast<float>(x) / (size - 1), static_cast<float>(y) / (size - 1)};
            }
        }

        // Generate indices
        data.indexBuffer.clear();
        data.indexBuffer.reserve((size - 1) * (size - 1) * 6); // 2 triangles = 6 vertices per quad
        for (uint32_t y = 0; y < size - 1; ++y) {
            for (uint32_t x = 0; x < size - 1; ++x) {
                // The "top, right, bottom, left" here are named when looking towards the surface.
                // Triangles are counter-clockwise in right-handed coordinate system.
                uint32_t bottomLeft = y * size + x;
                uint32_t bottomRight = bottomLeft + 1;
                uint32_t topLeft = (y + 1) * size + x;
                uint32_t topRight = topLeft + 1;

                // First triangle
                data.indexBuffer.push_back(bottomLeft);
                data.indexBuffer.push_back(bottomRight);
                data.indexBuffer.push_back(topLeft);

                // Second triangle
                data.indexBuffer.push_back(bottomRight);
                data.indexBuffer.push_back(topRight);
                data.indexBuffer.push_back(topLeft);
            }
        }
    }

    struct WaveMeshGenerator : MeshGenerator {
    public:
        WaveMeshGenerator()
            : MeshGenerator(100, /*doubleSided*/ true) {}

    private:
        void InitializeMeshData(MeshData& data) override {
            GenerateSquareMesh(data, VertexCountPerSide());
        }

        void EstimateIndexAndVertexCount(uint32_t& indexCount, uint32_t& vertexCount) const override {
            indexCount = (VertexCountPerSide() - 1) * (VertexCountPerSide() - 1) * 6;
            vertexCount = VertexCountPerSide() * VertexCountPerSide();
        }

        void ModifyMeshData(MeshDataRef& meshRef, float timeInSeconds) override {
            const uint32_t size = VertexCountPerSide();

            // Amplitude and frequency of the wave motion
            float amplitude = 0.25f;             // Adjust this value to control the amplitude of the wave
            float frequency = 2.0f;              // Adjust this value to control the frequency of the wave
            float waveTextureScrollSpeed = 0.1f; // Adjust this value to control the speed of the wave texture scrolling

            // Calculate the center of the mesh
            float radius = (size - 1) / 2.0f;
            float centerX = radius;
            float centerY = radius;

            // Iterate through each vertex and modify its position based on a circular wave
            for (uint32_t y = 0; y < size; ++y) {
                for (uint32_t x = 0; x < size; ++x) {
                    // Calculate index in the position buffer
                    uint32_t index = y * size + x;

                    // Calculate distance from the center of the mesh
                    float dx = x - centerX;
                    float dy = y - centerY;

                    // Distance is normalized to the range [0, 1]
                    float distance = sqrt(dx * dx + dy * dy) / radius;

                    // Calculate wave displacement based on time and distance from the center
                    float displacement = sin(timeInSeconds * frequency - distance * frequency);

                    // Update vertex position
                    meshRef.positionBuffer[index].z = amplitude * displacement;

                    // Calculate the normal
                    // For simplicity, assume the wave is only modifying the z-coordinate
                    // Normal is perpendicular to the mesh surface; here we assume a simple upward normal.
                    meshRef.normalBuffer[index] = {0.0f, 0.0f, 1.0f}; // Normal pointing upwards

                    // Calculate the tangent
                    // For a simple planar mesh, we assume tangents are in the x and y directions
                    if (meshRef.tangentBuffer.size() > 0) {                      // tangent buffer is optional
                        meshRef.tangentBuffer[index] = {1.0f, 0.0f, 0.0f, 1.0f}; // Tangent along the x-axis
                    }

                    if (meshRef.colorBuffer.size() > 0) {        // color buffer is optional
                        float color = (displacement + 1) / 2.0f; // this value must be in range of [0, 1]
                        meshRef.colorBuffer[index] = {(float)x / size, (float)y / size, color, 1.0f};
                    }

                    if (meshRef.uvBuffer.size() > 0) { // uv buffer is optional
                        float uvx = static_cast<float>(x) / (size - 1);
                        float uvy = static_cast<float>(y) / (size - 1);
                        uvy = std::fmod(uvy + timeInSeconds * waveTextureScrollSpeed, uvy);
                        meshRef.uvBuffer[index] = {uvx, uvy};
                    }
                }
            }
        }
    };

} // namespace

namespace sample {
    std::unique_ptr<MeshGenerator> CreateWaveMeshGenerator() {
        return std::make_unique<WaveMeshGenerator>();
    }
} // namespace sample
