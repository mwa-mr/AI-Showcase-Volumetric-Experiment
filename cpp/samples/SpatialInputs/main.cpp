#include <VolumetricApp.h>

#include <DirectXMath.h>

class SpatialInputVolume : public va::Volume {
public:
    SpatialInputVolume(va::VolumetricApp& app)
        : va::Volume(app) {
        onReady = std::bind(&SpatialInputVolume::OnReady, this);
        onUpdate = std::bind(&SpatialInputVolume::OnUpdate, this);
        onClose = std::bind(&SpatialInputVolume::OnClose, this);
        Container().AllowInteractiveMode(true);
    }

    void OnReady() {
        RequestUpdate(VA_VOLUME_UPDATE_MODE_FULL_FRAMERATE);

        Content().SetSize(1.0f); // Scale to meters
        Content().SetSizeBehavior(VA_VOLUME_SIZE_BEHAVIOR_FIXED);

        m_elements.spaceLocator = CreateElement<va::SpaceLocator>();
        m_elements.handTracker = CreateElement<va::HandTracker>();

        auto uri = va::windows::GetLocalAssetUri("axis_xyz_rub.glb");
        m_elements.model = CreateElement<va::ModelResource>(uri);
        m_elements.volumeContainer = CreateElement<va::VisualElement>(*m_elements.model);
        m_elements.volumeCentent = CreateElement<va::VisualElement>(*m_elements.model);
        m_elements.viewer = CreateElement<va::VisualElement>(*m_elements.model);
        m_elements.local = CreateElement<va::VisualElement>(*m_elements.model);

        for (auto side : {va::left_side, va::right_side}) {
            m_elements.handJoints[side].resize(VA_HAND_JOINT_COUNT_EXT, {});
            for (size_t i = 0; i < VA_HAND_JOINT_COUNT_EXT; i++) {
                m_elements.handJoints[side][i] = CreateElement<va::VisualElement>(*m_elements.model);
            }
        }
    }

    void OnUpdate() {
        const float volumeScale = Content().GetActualScale();
        const auto sizeInMeters = [volumeScale](float sizeInVolume) { return sizeInVolume / volumeScale; };

        if (m_elements.spaceLocator && m_elements.spaceLocator->IsReady()) {
            m_elements.spaceLocator->Update();

            const va::SpaceLocations& locations = m_elements.spaceLocator->Locations();
            static const auto updateSpaceElement = [](va::VisualElement* elem, const VaSpaceLocationExt& loc, float radius) {
                // Since the lenght of each axis in glb is 1 meters long, the requested radius is the scale of model.
                if (loc.isTracked) {
                    elem->SetPosition(loc.pose.position);
                    elem->SetOrientation(loc.pose.orientation);
                    elem->SetScale(radius);
                    elem->SetVisible(true);
                } else {
                    elem->SetVisible(false);
                }
            };

            // Set the volume visual to be 1x1x1 meters, so that it expands volume bounds when volume is
            // automatically sized using VA_VOLUME_SIZE_BEHAVIOR_AUTO_SIZE
            updateSpaceElement(m_elements.volumeContainer, locations.volumeContainer, 0.3f);
            updateSpaceElement(m_elements.volumeCentent, locations.volumeContent, 0.5f);

            // Use a fixed position and viewer's pose to demostrate a billboard rotation that always faces the user
            updateSpaceElement(m_elements.viewer, locations.viewer, sizeInMeters(0.1f));
            m_elements.viewer->SetPosition(VaVector3f(0.1f, 0.2f, 0.3f));

            updateSpaceElement(m_elements.local, locations.local, 0.2f);
        }

        if (m_elements.handTracker && m_elements.handTracker->IsReady()) {
            m_elements.handTracker->Update();

            for (auto side : {va::left_side, va::right_side}) {
                const va::JointLocations& locations = m_elements.handTracker->JointLocations(side);
                if (locations.isTracked) {
                    for (size_t i = 0; i < VA_HAND_JOINT_COUNT_EXT; i++) {
                        auto joint = m_elements.handJoints[side][i];
                        if (joint) {
                            joint->SetVisible(true);
                            joint->SetPosition(locations.poses[i].position);
                            joint->SetOrientation(locations.poses[i].orientation);
                            joint->SetScale(locations.radii[i]);
                        }
                    }
                } else {
                    // If the hand tracking is not active, hide the hand joints
                    for (size_t i = 0; i < VA_HAND_JOINT_COUNT_EXT; i++) {
                        auto joint = m_elements.handJoints[side][i];
                        if (joint) {
                            joint->SetVisible(false);
                        }
                    }
                }
            }
        }
    }

    void OnClose() {
        App().RequestExit();
        m_elements = {};
    }

private:
    VaVector3f m_randomPosition[VA_HAND_JOINT_COUNT_EXT][va::side_count];

    struct {
        va::ModelResource* model;
        va::SpaceLocator* spaceLocator;
        va::HandTracker* handTracker;
        va::VisualElement* volumeContainer;
        va::VisualElement* volumeCentent;
        va::VisualElement* viewer;
        va::VisualElement* local;
        std::vector<va::VisualElement*> handJoints[va::side_count];
    } m_elements = {};
};

int main() {
    auto app = va::CreateVolumetricApp({"cpp_spatial_input",
                                        {
                                            VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME,
                                            VA_EXT_LOCATE_SPACES_EXTENSION_NAME,
                                            VA_EXT_LOCATE_JOINTS_EXTENSION_NAME,
                                            VA_EXT_VOLUME_CONTAINER_MODES_EXTENSION_NAME,
                                        }});

    return app->Run([&](va::VolumetricApp& app) { app.CreateVolume<SpatialInputVolume>(); });
}
