#include <VolumetricApp.h>
#include <SampleMesh/MeshData.h>
#include <SampleMesh/MeshGenerator.h>
#include <SampleMathHelpers.h>
#include <algorithm>
#include <cmath>
#include <string>   // Added for std::stoi
#include <vector>   // Added for std::vector
#include <iostream> // Added for error messages

// In this sample, we use the "Simple.gltf" model and modify its mesh in real-time.
// This gltf model contains a single mesh with just position and normal attributes.
// The order of these buffers here matches the helper structs in MeshData.h.
const std::vector<VaMeshBufferDescriptorExt> BufferDesciptors{
    {VA_MESH_BUFFER_TYPE_INDEX_EXT, VA_MESH_BUFFER_FORMAT_UINT32_EXT},
    {VA_MESH_BUFFER_TYPE_VERTEX_POSITION_EXT, VA_MESH_BUFFER_FORMAT_FLOAT3_EXT},
    {VA_MESH_BUFFER_TYPE_VERTEX_NORMAL_EXT, VA_MESH_BUFFER_FORMAT_FLOAT3_EXT},
};

class MeshEditVolume : public va::Volume {
private:
    struct TessellatedSphere {
        va::VisualElement* visual = nullptr;
        va::ModelResource* model = nullptr;
        va::MeshResource* mesh = nullptr;
        va::MaterialResource* material = nullptr;
    };

    // The base model is a unit cube in range [-1,1] that is going to be morphed into a sphere.
    // From PulseImpactSphere.cpp, max displacement is amplitude * (decay + bounce)
    // amplitude = 0.2, decay_max = 1, bounce_max = 0.1 * decay_max = 0.1.
    // damping_max = 1.1. ripple_max = 1. displacement_max = 0.2 * 1.1 * 1 = 0.22.
    // So, max radius is 1.0 + 0.22 = 1.22.
    static constexpr float s_baseRadius = 1.22f;
    static constexpr float s_spacing = 3.0f;
    static constexpr float s_scaleFactor = 0.8f;

    void SetContentSize() {
        Content().SetSizeBehavior(VA_VOLUME_SIZE_BEHAVIOR_FIXED);

        float chainLength = 0.0f;
        float lastSphereRadius = s_baseRadius;
        float currentScale = 1.0f;
        for (int i = 1; i < m_count; ++i) {
            chainLength += s_spacing * currentScale;
            currentScale *= s_scaleFactor;
        }
        lastSphereRadius *= currentScale;

        // The chain of spheres rotates around the origin (center of the first sphere).
        // The maximum extent from the origin is the length of the chain plus the radius of the last sphere.
        const float maxExtent = chainLength + lastSphereRadius;

        // The content size must enclose a sphere with radius `maxExtent` centered at the origin,
        // as well as the first sphere. The height is determined by the largest sphere (the first one).
        const float requiredSize = std::max(maxExtent, s_baseRadius) * 2.0f;
        const VaExtent3Df contentSizeExtent{requiredSize, 2.0f * s_baseRadius, requiredSize};
        Content().SetSize(contentSizeExtent);
    }

public:
    MeshEditVolume(va::VolumetricApp& app, int count)
        : va::Volume(app)
        , m_count(count) { // Initialize m_count
        onReady = [this](auto&) { OnReady(); };
        onUpdate = [this](auto&) { OnUpdate(); };
        onClose = [this](auto&) { OnClose(); };
        m_meshGenerator = sample::CreatePulseMeshGenerator();
    }

    void OnReady() {
        RequestUpdate(VA_VOLUME_UPDATE_MODE_FULL_FRAMERATE);

        SetContentSize();

        m_sphere.resize(m_count);

        for (int i = 0; i < m_count; ++i) {
            m_sphere[i].model = CreateElement<va::ModelResource>(va::windows::GetLocalAssetUri("Simple.gltf"));
            m_sphere[i].visual = CreateElement<va::VisualElement>(*m_sphere[i].model);

            if (i > 0) {
                m_sphere[i].visual->SetVisualParent(*m_sphere[i - 1].visual); // Set the parent to the previous visual
            }

            m_sphere[i].mesh = CreateElement<va::MeshResource>(*m_sphere[i].model, "Node0", 0, BufferDesciptors, true /*decoupleAccessors*/, false /*initializeData*/);
            m_sphere[i].material = CreateElement<va::MaterialResource>(*m_sphere[i].model, "Material0");

            // Apply spacing relative to the sphere on the left
            m_sphere[i].visual->SetPosition(VaVector3f{i == 0 ? 0.0f : s_spacing, 0.0f, 0.0f});

            // Each sphere is 80% of the size of the previous one
            m_sphere[i].visual->SetScale(i == 0 ? 1.0f : s_scaleFactor);

            // BUG: setting invisible also impact auto-size behavior when loading the model
            // Temporarily disable this line until the bug is fixed in platform.
            // m_elements[i].visual->SetVisible(false); // Hide the model until the mesh is ready to be edited.
        }
    }

    void OnUpdate() {
        using namespace std::chrono;
        const va::FrameState& frameState = FrameState();
        const float seconds = frameState.frameTime * 1e-9f;
        const VaQuaternionf quaternion = va::quaternion::from_axis_rotation(va::vector::up, va::degrees_to_radians(90 * seconds));

        for (int i = 0; i < m_count; ++i) {
            if (m_sphere[i].model->IsReady()) {
                m_sphere[i].visual->SetVisible(true);
                UpdateMesh(i);
                UpdateMaterial(i);
                m_sphere[i].visual->SetOrientation(quaternion);
            }
        }
    }

    void UpdateMesh(int index) {
        if (!m_sphere[index].mesh || !m_sphere[index].mesh->IsReady()) {
            return;
        }

        std::vector<va::MeshBufferData> buffers = {
            va::MeshBufferData(BufferDesciptors[0]), // index
            va::MeshBufferData(BufferDesciptors[1]), // position
            va::MeshBufferData(BufferDesciptors[2]), // normal
        };

        using namespace std::chrono;
        const float seconds = duration_cast<duration<float>>(steady_clock::now().time_since_epoch()).count();

        // Vary properties slightly per instance using the index
        const float timeOffset = static_cast<float>(index) * 0.5f;
        const uint32_t vertexCountPerSide = static_cast<uint32_t>(5 + 15 * sample::PositiveSinWave(seconds + timeOffset, 10));
        m_meshGenerator->SetVertexCountPerSide(vertexCountPerSide);

        uint32_t indexCount, vertexCount;
        m_meshGenerator->GetMeshBufferSize(indexCount, vertexCount);

        m_sphere[index].mesh->WriteMeshBuffers(buffers, indexCount, vertexCount, [&](auto&) {
            sample::MeshDataRef meshRef{};
            meshRef.indexCount = buffers[0].Count();
            meshRef.vertexCount = buffers[1].Count();
            meshRef.indexBuffer = buffers[0].AsSpan<uint32_t>();
            meshRef.positionBuffer = buffers[1].AsSpan<VaVector3f>();
            meshRef.normalBuffer = buffers[2].AsSpan<VaVector3f>();

            m_meshGenerator->UpdateMeshData(meshRef, seconds + timeOffset, true);
        });
    }

    void UpdateMaterial(int index) { // Added index parameter
        using namespace std::chrono;
        const float seconds = duration_cast<duration<float>>(steady_clock::now().time_since_epoch()).count();

        if (m_sphere[index].material && m_sphere[index].material->IsReady()) {
            // This sample doesn't work yet for non-PBR materials.
            auto materialType = m_sphere[index].material->MaterialType();
            CHECK(materialType == VA_MATERIAL_TYPE_PBR_EXT);

            // Vary properties slightly per instance using the index
            const float timeOffset = static_cast<float>(index) * 0.5f;
            const VaColor4f srgbColor = sample::HSVtoRGB(sample::PositiveSinWave(seconds + timeOffset, 5), 1, 1);
            const VaColor4f linearColor = SRGBToLinear(srgbColor);
            m_sphere[index].material->SetPbrBaseColorFactor(linearColor);
            m_sphere[index].material->SetPbrRoughnessFactor(sample::PositiveSinWave(seconds + timeOffset, 10));
            m_sphere[index].material->SetPbrMetallicFactor(sample::PositiveSinWave(seconds + timeOffset, 20));
        }
    }

    void OnClose() {
        m_sphere.clear(); // Clear the vector
        App().RequestExit();
    }

private:
    VaColor4f SRGBToLinear(const VaColor4f& color) const {
        const auto convertChannel = [](float channel) noexcept -> float {
            const float clampedChannel = std::clamp(channel, 0.0f, 1.0f);
            if (clampedChannel <= 0.04045f) {
                return clampedChannel / 12.92f;
            }

            return static_cast<float>(std::pow((clampedChannel + 0.055f) / 1.055f, 2.4f));
        };

        return VaColor4f{convertChannel(color.r), convertChannel(color.g), convertChannel(color.b), color.a};
    }

    int m_count;
    std::vector<TessellatedSphere> m_sphere;
    std::unique_ptr<sample::MeshGenerator> m_meshGenerator;
};

int main(int argc, char* argv[]) { // Added argc, argv
    int count = 1;                 // Default count

    // Simple command-line argument parsing for --count
    for (int i = 1; i < argc; ++i) {
        std::string arg = argv[i];
        if (arg == "--count" && i + 1 < argc) {
            try {
                count = std::stoi(argv[i + 1]);
                if (count <= 0) {
                    std::cerr << "Warning: --count must be positive. Using default count=1." << std::endl;
                    count = 1;
                }
                break;
            } catch (const std::invalid_argument& e) {
                std::cerr << "Warning: Invalid argument for --count. Using default count=1. Error: " << e.what() << std::endl;
                count = 1;
            } catch (const std::out_of_range& e) {
                std::cerr << "Warning: --count value out of range. Using default count=1. Error: " << e.what() << std::endl;
                count = 1;
            }
        }
    }

    auto app = va::CreateVolumetricApp({"cpp_mesh_edit",
                                        {
                                            VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME,
                                            VA_EXT_MATERIAL_RESOURCE_EXTENSION_NAME,
                                            VA_EXT_MESH_EDIT_EXTENSION_NAME,
                                        }});
    return app->Run([&](auto& app) { app.CreateVolume<MeshEditVolume>(count); });
}
