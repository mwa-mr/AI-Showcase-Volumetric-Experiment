#include "MeshGenerator.h"

namespace {
    using namespace sample;

    void GenerateSphereMesh(MeshData& data, uint32_t size) {
        const float PI = 3.14159265358979323846f;

        data.vertexCount = (size + 1) * (size + 1);
        data.indexCount = 6 * size * size;

        data.indexBuffer.resize(data.indexCount);
        data.positionBuffer.resize(data.vertexCount);
        data.normalBuffer.resize(data.vertexCount);
        data.tangentBuffer.resize(data.vertexCount);
        data.colorBuffer.resize(data.vertexCount);
        data.uvBuffer.resize(data.vertexCount);

        uint32_t vertexIndex = 0;
        uint32_t indexIndex = 0;

        for (uint32_t y = 0; y <= size; ++y) {
            for (uint32_t x = 0; x <= size; ++x) {
                float xSegment = (float)x / (float)size;
                float ySegment = (float)y / (float)size;
                float xPos = std::cos(xSegment * 2.0f * PI) * std::sin(ySegment * PI);
                float yPos = std::cos(ySegment * PI);
                float zPos = std::sin(xSegment * 2.0f * PI) * std::sin(ySegment * PI);

                data.positionBuffer[vertexIndex] = {xPos, yPos, zPos};
                data.normalBuffer[vertexIndex] = {xPos, yPos, zPos};
                data.tangentBuffer[vertexIndex] = {-std::sin(xSegment * 2.0f * PI), 0.0f, std::cos(xSegment * 2.0f * PI), 1.0f};
                data.colorBuffer[vertexIndex] = {std::sin(ySegment * PI), std::cos(ySegment * PI), 1.0f, 1.0f};
                data.uvBuffer[vertexIndex] = {xSegment, ySegment};
                ++vertexIndex;
            }
        }

        for (uint32_t y = 0; y < size; ++y) {
            for (uint32_t x = 0; x < size; ++x) {
                data.indexBuffer[indexIndex++] = (y + 1) * (size + 1) + x;
                data.indexBuffer[indexIndex++] = y * (size + 1) + x;
                data.indexBuffer[indexIndex++] = y * (size + 1) + x + 1;

                data.indexBuffer[indexIndex++] = (y + 1) * (size + 1) + x;
                data.indexBuffer[indexIndex++] = y * (size + 1) + x + 1;
                data.indexBuffer[indexIndex++] = (y + 1) * (size + 1) + x + 1;
            }
        }
    }

    struct PulseMeshGenerator : MeshGenerator {
    public:
        PulseMeshGenerator()
            : MeshGenerator(100, /*doubleSided*/ false) {}

    private:
        void InitializeMeshData(MeshData& data) override {
            GenerateSphereMesh(data, VertexCountPerSide());
        }

        void EstimateIndexAndVertexCount(uint32_t& indexCount, uint32_t& vertexCount) const override {
            indexCount = 6 * VertexCountPerSide() * VertexCountPerSide();
            vertexCount = (VertexCountPerSide() + 1) * (VertexCountPerSide() + 1);
        }

        void ModifyMeshData(MeshDataRef& meshRef, float timeInSeconds) override {
            constexpr float PI = 3.14159265358979323846f;
            constexpr float frequency = 3.0f; // Frequency of the ripple
            constexpr float amplitude = 0.2f; // Amplitude of the ripple
            constexpr float duration = 0.6f;  // Duration of the impact effect
            constexpr float northPoleY = 1.0f;
            constexpr float cycleDuration = 2.0f;        // Time for one full cycle of the ripple effect
            constexpr float oscillationFrequency = 2.0f; // Frequency of the bouncing effect
            constexpr float oscillationAmplitude = 0.1f; // Amplitude of the bouncing effect
            constexpr float waveScrollSpeed = 0.1f;      // Speed of the wave texture scrolling

            const float timeSinceImpact = fmod(timeInSeconds, cycleDuration);

            uint32_t size = VertexCountPerSide();

            uint32_t vertexIndex = 0;
            for (uint32_t y = 0; y <= size; ++y) {
                for (uint32_t x = 0; x <= size; ++x) {
                    const float xSegment = (float)x / (float)size;
                    const float ySegment = (float)y / (float)size;
                    const float xPos = std::cos(xSegment * 2.0f * PI) * std::sin(ySegment * PI);
                    const float yPos = std::cos(ySegment * PI);
                    const float zPos = std::sin(xSegment * 2.0f * PI) * std::sin(ySegment * PI);

                    VaVector3f position = {xPos, yPos, zPos};
                    VaVector3f normal = {xPos, yPos, zPos};

                    const float distanceFromPole = std::sqrt((position.x * position.x) + (position.z * position.z) + ((position.y - northPoleY) * (position.y - northPoleY)));

                    float phase = timeSinceImpact * PI * 2.0f / duration; // Calculate the phase shift over time
                    float ripple = std::sin(distanceFromPole * frequency - phase);

                    // Apply exponential decay and oscillatory bounce effect
                    float decay = std::exp(-timeSinceImpact / duration * 3.0f);
                    float bounce = oscillationAmplitude * decay * std::sin(oscillationFrequency * timeSinceImpact * PI * 2.0f);
                    float damping = decay + bounce;

                    const float displacement = ripple * damping * amplitude;
                    const float colorChange = damping * (ripple + 1.0f) / 2.0f; // value must in range of [0, 1]

                    position.x += normal.x * displacement;
                    position.y += normal.y * displacement;
                    position.z += normal.z * displacement;

                    meshRef.positionBuffer[vertexIndex] = position;
                    if (meshRef.colorBuffer.size() > 0) { // color buffer is optional
                        meshRef.colorBuffer[vertexIndex] = {std::sin(ySegment * PI), std::cos(ySegment * PI), colorChange, 1.0f};
                    }
                    if (meshRef.uvBuffer.size() > 0) { // uv buffer is optional
                        meshRef.uvBuffer[vertexIndex] = {xSegment, std::fmod(ySegment + timeInSeconds * waveScrollSpeed, 1.0f)};
                    }
                    ++vertexIndex;
                }
            }
        }
    };

} // namespace

namespace sample {
    std::unique_ptr<MeshGenerator> CreatePulseMeshGenerator() {
        return std::make_unique<PulseMeshGenerator>();
    }
} // namespace sample
