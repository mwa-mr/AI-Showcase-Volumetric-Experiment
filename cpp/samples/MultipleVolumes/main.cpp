#include <chrono>
#include <iostream>
#include <queue>
#include <optional>

// This sample demos compatibility with non-unicode projects
#undef UNICODE

#define VA_ENABLE_DEBUG_PRINTF 1
#include <vaError.h>
#include <vaMath.h>
#include <VolumetricApp.h>

namespace {
    // This sample class contains exactly one gltf model in a volume.
    // Once the model is successfully loaded, it starts spinning automatically.
    class SpinningModel : public va::Volume {
    public:
        SpinningModel(va::VolumetricApp& app,
                      std::string uri,
                      VaVolumeUpdateMode volumeUpdateMode = VA_VOLUME_UPDATE_MODE_FULL_FRAMERATE,
                      float scale = 1.0,
                      bool axisModelAsVisualParent = false)
            : va::Volume(app)
            , m_uri(uri)
            , m_modelScale(scale)
            , m_volumeUpdateMode(volumeUpdateMode)
            , m_axisModelAsVisualParent(axisModelAsVisualParent) {
            onReady = std::bind(&SpinningModel::OnReady, this);
            onUpdate = std::bind(&SpinningModel::OnUpdate, this);
            onClose = std::bind(&SpinningModel::OnClose, this);
        }

        void AddNewModel(const std::string& newModelUri, float scale = 1.0f) {
            std::lock_guard lock(m_newModelUriLock);
            m_newModelUri = newModelUri;
            m_newModelScale = scale;
        }

    private:
        void OnReady() {
            RequestUpdate(m_volumeUpdateMode);

            m_model = CreateElement<va::ModelResource>();
            m_model->SetModelUri(m_uri);

            m_visual = CreateElement<va::VisualElement>();
            m_visual->SetVisualResource(*m_model);
            m_visual->SetScale(m_modelScale);

            const std::string axis_uri_path = va::windows::GetLocalAssetUri("axis_xyz_rub.glb");
            m_axis_model = CreateElement<va::ModelResource>();
            m_axis_model->SetModelUri(axis_uri_path);

            m_axis_visual = CreateElement<va::VisualElement>();
            m_axis_visual->SetVisualResource(*m_axis_model);

            if (m_axisModelAsVisualParent) {
                m_visual->SetVisualParent(*m_axis_visual);
            }
        }

        void OnUpdate() {
            using namespace std::chrono;
            const va::FrameState& frameState = FrameState();
            const float seconds = frameState.frameTime * 1e-9f;
            VaQuaternionf quaternion = va::quaternion::from_axis_rotation(va::vector::up, va::degrees_to_radians(90 * seconds));

            if (m_visual && m_visual->IsReady() && !m_axisModelAsVisualParent) {
                m_visual->SetOrientation(quaternion);
            }
            if (m_axis_visual && m_axis_visual->IsReady()) {
                m_axis_visual->SetOrientation(quaternion);
            }

            {
                std::lock_guard lock(m_newModelUriLock);
                if (!m_newModelUri.empty()) {
                    va::ModelResource* model = CreateElement<va::ModelResource>();
                    model->SetModelUri(m_newModelUri);

                    va::VisualElement* visual = CreateElement<va::VisualElement>();
                    m_newModelVisuals.push_back(visual);
                    visual->SetVisualResource(*model);
                    visual->SetScale(m_newModelScale);

                    m_newModelVisuals.push_back(visual);

                    m_newModelUri = "";
                    m_newModelScale = 1.0f;
                }
            }

            for (va::VisualElement* visual : m_newModelVisuals) {
                if (visual->IsReady()) {
                    visual->SetOrientation(quaternion);
                }
            }
        }

        void OnClose() {
            m_visual = nullptr;
            m_model = nullptr;
        }

    private:
        std::string m_uri;
        va::VisualElement* m_visual{};
        va::ModelResource* m_model{};
        const float m_modelScale;
        const bool m_axisModelAsVisualParent;

        va::VisualElement* m_axis_visual{};
        va::ModelResource* m_axis_model{};
        const VaVolumeUpdateMode m_volumeUpdateMode;

        std::mutex m_newModelUriLock;
        std::string m_newModelUri;
        float m_newModelScale = 1.0f;
        std::vector<va::VisualElement*> m_newModelVisuals;
    };

    void PrintHelp() {
        std::cout << "exit" << std::endl;
        std::cout << "      Exit this app." << std::endl;
        std::cout << "help" << std::endl;
        std::cout << "      Show this help." << std::endl;
        std::cout << "load <glb_file_path>" << std::endl;
        std::cout << "      Load the glb file as a new volume." << std::endl;
        std::cout << "      Example: load e:\\temp\\sample.glb" << std::endl;
        std::cout << "new <glb_file_path>" << std::endl;
        std::cout << "      Load the glb file as a new model in the first volume." << std::endl;
        std::cout << "      Example: load e:\\temp\\sample.glb" << std::endl;
        std::cout << "khronos <glb_file_name>" << std::endl;
        std::cout << "      Load the glb file from Khronos sample gltf GitHub repo." << std::endl;
        std::cout << "      Example: khronos BoomBox" << std::endl;
        std::cout << "               Find valid gltf samples at https://github.com/KhronosGroup/glTF-Sample-Assets/tree/main/Models/" << std::endl;
    }

    std::optional<std::string> ConvertGlbFilepath(const std::string& file) {
        try {
            std::filesystem::path path(file);
            if (!std::filesystem::exists(path) || !std::filesystem::is_regular_file(path) || path.extension() != ".glb") {
                std::cerr << "This is not a valid file path to glb file: " << file << std::endl;
                return {};
            } else {
                auto uri = va::windows::FilePathToUri(va::wide_to_utf8(path.wstring()));
                std::cout << "Convert path to uri : " << uri << std::endl;
                return uri;
            }
        } catch (...) {}
        std::cerr << "Something wrong when loading this file : " << file << std::endl;
        return {};
    }

    std::optional<std::string> GetKhronosSampleUri(const char* name) {
        const std::string KhronosSampleBaseUri = "https://raw.GithubUserContent.com/KhronosGroup/glTF-Sample-Assets/main/Models/";
        return KhronosSampleBaseUri + name + "/glTF-Binary/" + name + ".glb";
    }

} // namespace

bool HandleNewModelCommand(const std::string& command, SpinningModel* volume) {
    std::string filepath = command;
    float scale = 1.0f;

    size_t glbPos = command.find(".glb ");
    if (glbPos != std::string::npos) {
        size_t scaleStartPos = glbPos + 5;
        if (scaleStartPos >= command.length()) {
            filepath = command;
        } else {
            std::string scaleStr = command.substr(scaleStartPos);
            size_t firstNonSpace = scaleStr.find_first_not_of(" \t\r\n");

            if (firstNonSpace != std::string::npos) {
                scaleStr = scaleStr.substr(firstNonSpace);
                try {
                    scale = std::stof(scaleStr);
                    filepath = command.substr(0, glbPos + 4); // Include .glb in filepath
                } catch (const std::exception&) {
                    std::cerr << "Invalid scale value: " << scaleStr << std::endl;
                    return false;
                }
            }
        }
    }

    if (auto uri = ConvertGlbFilepath(filepath)) {
        volume->AddNewModel(*uri, scale);
        std::cout << "Added new model from " << filepath << " with scale " << scale << std::endl;
    }
    return true;
}

int main() {
    auto app = va::CreateVolumetricApp({"cpp_multiple_volumes",
                                        {
                                            VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME,
                                        }});

    SpinningModel* volumeOne = nullptr;

    // When the volumetric app starts we add new volumes, one from web and another from local file.
    app->onStart = [&volumeOne](va::VolumetricApp& app) {
        volumeOne = app.CreateVolume<SpinningModel>(va::windows::GetLocalAssetUri("Duck.glb"), VA_VOLUME_UPDATE_MODE_FULL_FRAMERATE);
        app.CreateVolume<SpinningModel>(va::windows::GetLocalAssetUri("BoomBox.glb"), VA_VOLUME_UPDATE_MODE_QUARTER_FRAMERATE, 50.0f, true);
    };

    // When the volumetric app is stopped, exit the console prompt.
    app->onStop = []() {
        std::cout << "Volumetric app is stopped." << std::endl;
        exit(0);
    };

    // When the volumetric app encounters a fatal error, print the error and then exit.
    app->onFatalError = [](const char* errorMessage) {
        std::cout << "Volumetric app encounters fatal error: " << std::endl << errorMessage << std::endl;
        exit(-1);
    };

    // For this sample, we print some message when a system is disconnected or reconnected.
    app->onDisconnect = [](auto&) { TRACE("System disconnected. Here's a good place for app to update UI."); };
    app->onReconnect = [](auto&) { TRACE("System reconnected. Here's a good place for app to update UI."); };

    // Start the volumetric app in async mode, and yield the main thread to the console prompt
    auto future = app->RunAsync();

    // Loop to handle user's input from the console prompt, until user types "exit".
    std::string input;
    PrintHelp();
    while (true) {
        std::cout << "multiple_volumes : > ";
        std::getline(std::cin, input);

        if (input == "exit") {
            app->RequestExit();
            future.wait(); // And wait for the volumetric app to be stopped.
            break;         // Then exit the console prompt loop.
        } else if (input.find("load ") == 0) {
            if (auto uri = ConvertGlbFilepath(input.substr(5))) {
                app->CreateVolume<SpinningModel>(*uri);
            }
        } else if (input.find("khronos ") == 0) {
            if (auto uri = GetKhronosSampleUri(input.substr(8).c_str())) {
                app->CreateVolume<SpinningModel>(*uri);
            }
        } else if (input.find("new ") == 0) {
            if (volumeOne) {
                HandleNewModelCommand(input.substr(4), volumeOne);
            }
        } else if (input == "help") {
            PrintHelp();
        } else if (!input.empty()) {
            std::cout << "Unknown command: " << input << std::endl;
            PrintHelp();
        }
    }

    return 0;
}
