#include <VolumetricApp.h>

class SpinningModel : public va::Volume {
public:
    SpinningModel(va::VolumetricApp& app)
        : va::Volume(app) {
        onReady = std::bind(&SpinningModel::OnReady, this);
        onUpdate = std::bind(&SpinningModel::OnUpdate, this);
        onClose = std::bind(&SpinningModel::OnClose, this);
    }

private:
    void OnReady() {
        RequestUpdate(VA_VOLUME_UPDATE_MODE_FULL_FRAMERATE);

        // Reference to the Gltf file on GitHub for node name and scene structures.
        // https://github.com/KhronosGroup/glTF-Sample-Models/blob/main/2.0/AntiqueCamera/glTF/AntiqueCamera.gltf
        auto uri = va::windows::GetLocalAssetUri("AntiqueCamera.glb");

        m_elements.model = CreateElement<va::ModelResource>(uri);
        m_elements.visual = CreateElement<va::VisualElement>(*m_elements.model);
        m_elements.camera = CreateElement<va::VisualElement>(*m_elements.visual, "camera");
        m_elements.tripod = CreateElement<va::VisualElement>(*m_elements.visual, "tripod");
    }

    void OnUpdate() {
        if (m_elements.model->IsReady()) {
            const va::FrameState& frameState = FrameState();
            const float seconds = frameState.frameTime * 1e-9f;
            VaQuaternionf quaternion = va::quaternion::from_axis_rotation(va::vector::up, va::degrees_to_radians(90 * seconds));
            m_elements.camera->SetOrientation(quaternion);

            quaternion.y *= -1; // Rotate the tripod in the opposite direction
            m_elements.tripod->SetOrientation(quaternion);
        }
    }

    void OnClose() {
        m_elements = {};
        App().RequestExit();
    }

private:
    struct {
        va::ModelResource* model{};
        va::VisualElement* visual{};
        va::VisualElement* camera{};
        va::VisualElement* tripod{};
    } m_elements{};
};

int main() {
    auto app = va::CreateVolumetricApp({"cpp_named_nodes",
                                        {
                                            VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME,
                                        }});
    return app->Run([](va::VolumetricApp& app) { app.CreateVolume<SpinningModel>(); });
}
