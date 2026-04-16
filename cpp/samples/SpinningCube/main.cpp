#include <chrono>
#include <VolumetricApp.h>

namespace {
    const std::vector<VaMeshBufferDescriptorExt> BufferDescriptors{
        VaMeshBufferDescriptorExt{VA_MESH_BUFFER_TYPE_INDEX_EXT, VA_MESH_BUFFER_FORMAT_UINT32_EXT},
        VaMeshBufferDescriptorExt{VA_MESH_BUFFER_TYPE_VERTEX_POSITION_EXT, VA_MESH_BUFFER_FORMAT_FLOAT3_EXT},
        VaMeshBufferDescriptorExt{VA_MESH_BUFFER_TYPE_VERTEX_NORMAL_EXT, VA_MESH_BUFFER_FORMAT_FLOAT3_EXT},
    };

    class SpinningVolume : public va::Volume {
    public:
        SpinningVolume(va::VolumetricApp& app)
            : va::Volume(app) {
            onReady = std::bind(&SpinningVolume::OnReady, this);
            onUpdate = std::bind(&SpinningVolume::OnUpdate, this);
            onClose = std::bind(&SpinningVolume::OnClose, this);
        }

    private:
        // Called once on the first update after volume is created.
        void OnReady() {
            auto uri = va::windows::GetLocalAssetUri("BoxTextured.glb");
            m_model = CreateElement<va::ModelResource>(uri);
            m_visual = CreateElement<va::VisualElement>(*m_model);
            m_mesh = CreateElement<va::MeshResource>(*m_model, 0, 0, BufferDescriptors, false /*decoupleAccessors*/, true /*initializeData*/);

            RequestUpdate(VA_VOLUME_UPDATE_MODE_FULL_FRAMERATE);
        }

        // Called once per frame update.
        void OnUpdate() {
            if (m_model && m_model->IsReady()) {
                using namespace std::chrono;
                const va::FrameState& frameState = FrameState();
                const float seconds = frameState.frameTime * 1e-9f;
                VaQuaternionf quaternion = va::quaternion::from_axis_rotation(va::vector::up, va::degrees_to_radians(90 * seconds));
                m_visual->SetOrientation(quaternion);        // Spin on Y axis
                m_visual->SetScale((cosf(seconds) + 3) / 4); // Pulse the scale between 0.5 to 1

                // Update mesh vertices, while keep the index buffer intact.  Let platform handle normal buffer automatically.
                std::vector<va::MeshBufferData> buffers = {
                    va::MeshBufferData({VA_MESH_BUFFER_TYPE_VERTEX_POSITION_EXT, VA_MESH_BUFFER_FORMAT_FLOAT3_EXT}), // buffers[0] is position buffer
                };
                m_mesh->WriteMeshBuffers(buffers, 0, 0, [&](auto&) {
                    auto t = -3 * seconds;
                    auto ct = cosf(t);
                    auto st = sinf(t);
                    auto r = 0.5f; // 1m cube, so the radius is 0.5

                    // Modify the top 12 vertices while keeping the bottom 24 vertices intact.
                    // clang-format off
                    // x' = x cos(t) - y sin(t)
                    // y' = y cos(t) + x sin(t)
                    float vertices[] = {
                        (float)(-r * ct - -r * st), (float)(-r * ct + -r * st), r,  // v0 -> (-r, -r)
                        (float)( r * ct - -r * st), (float)(-r * ct +  r * st), r,  // v1 -> ( r, -r)
                        (float)(-r * ct -  r * st), (float)( r * ct + -r * st), r,  // v2 -> (-r,  r)
                        (float)( r * ct -  r * st), (float)( r * ct +  r * st), r };// v3 -> ( r,  r)
                    // clang-format on

                    memcpy(buffers[0].Ptr<float>(), vertices, sizeof(vertices));
                });
            }
        }

        void OnClose() {
            m_visual = nullptr;
            m_model = nullptr;
            m_mesh = nullptr;
            App().RequestExit();
        }

    private:
        va::VisualElement* m_visual{};
        va::ModelResource* m_model{};
        va::MeshResource* m_mesh{};
    };

} // namespace

int main() {
    auto app = va::CreateVolumetricApp({"spinning_cube_cpp",
                                        {
                                            VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME,
                                            VA_EXT_MESH_EDIT_EXTENSION_NAME,
                                        }});

    return app->Run([](va::VolumetricApp& app) { app.CreateVolume<SpinningVolume>(); });
}
