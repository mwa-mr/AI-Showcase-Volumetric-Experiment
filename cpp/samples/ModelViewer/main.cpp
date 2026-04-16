#define VA_ENABLE_DEBUG_PRINTF 1
#include <vaError.h>
#include <vaUuid.h>
#include <vaEnums.h>
#include <VolumetricApp.h>
#include "FileCollection.h"

namespace {
    constexpr char s_SingleModelTemplate[] = R"(
{
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "type": "AdaptiveCard",
    "version": "1.4",
    "body": [
        {
            "id": "modelInfo",
            "type": "TextBlock",
            "text": "${currentModel}",
            "horizontalAlignment": "center"
        }
    ]
})";

    constexpr char s_MultiModelTemplate[] = R"(
{
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "type": "AdaptiveCard",
    "version": "1.4",
    "body": [
        {
            "id": "modelInfo",
            "type": "TextBlock",
            "text": "${currentModel}",
            "horizontalAlignment": "center"
        }
    ],
    "actions": [
    {
        "type": "Action.Execute",
        "title": "<",
        "verb": "dec"
    },
    {
        "type": "Action.Execute",
        "title": ">",
        "verb": "inc"
    }
  ]
})";

    std::string FormatAdaptiveCardData(const std::string& modelName) {
        constexpr char adaptiveCardDataFormat[] = R"({"currentModel": "%s"})";
        auto length = snprintf(nullptr, 0, adaptiveCardDataFormat, modelName.c_str());
        std::string result(length, 0);
        snprintf(result.data(), length + 1, adaptiveCardDataFormat, modelName.c_str());
        return result;
    }

    class ModelViewerSample : public va::Volume {
    public:
        ModelViewerSample(va::VolumetricApp& app, std::unique_ptr<IFileCollection> fileCollection)
            : va::Volume(app)
            , m_fileCollection(std::move(fileCollection)) {
            InitializeVolume();
        }

        ModelViewerSample(va::VolumetricApp& app, const VaUuid& restoreId)
            : va::Volume(app, restoreId)
            , m_fileCollection(LoadFileCollection(restoreId)) {
            InitializeVolume();
            m_fileCollection->RestoreState(va::to_wstring(restoreId));
        }

    private:
        void InitializeVolume() {
            using std::placeholders::_1;

            Container().SetDisplayName("Volumetric Model Viewer");

            onReady = std::bind(&ModelViewerSample::OnReady, this);
            onClose = std::bind(&ModelViewerSample::OnClose, this);

            Container().AllowSharingInTeams(true);
            Container().AllowSubpartMode(true);
            Container().AllowOneToOneMode(true);

            Container().SetThumbnailIconUri(va::windows::GetLocalAssetUri("ModelViewerThumbnail.png").c_str());
        }

        void OnReady() {
            using std::placeholders::_1, std::placeholders::_2;

            m_elements.model = CreateElement<va::ModelResource>();
            m_elements.model->onAsyncStateChange = std::bind(&ModelViewerSample::OnModelAsyncStateChanged, this, _1);

            m_elements.visual = CreateElement<va::VisualElement>(*m_elements.model);

            // We'll only create the adaptive card right now if there are multiple models and we need to show the next/previous buttons.
            if (m_fileCollection->Count() > 1) {
                m_elements.centerCard = CreateElement<va::AdaptiveCardElement>();
                m_elements.centerCard->SetTemplate(s_MultiModelTemplate);
                m_elements.centerCard->SetData(FormatAdaptiveCardData("..."));
                m_elements.centerCard->onAction = std::bind(&ModelViewerSample::OnAdaptiveCardAction, this, _1, _2);
            }

            StartLoadingModel();
        }

        void UpdateModelSelection() {
            ModelData modelData{};
            if (m_fileCollection->TryGetCurrent(modelData) && modelData.fileUri != m_currentModelUri) {
                StartLoadingModel();
            }
        }

        void OnClose() {
            m_elements = {};
            App().RequestExit();
        }

        void StartLoadingModel() {
            ModelData modelData{};
            if (!m_fileCollection->TryGetCurrent(modelData)) {
                TRACE("There's no model to load.");
                return;
            }

            Container().SetDisplayName(modelData.fileNameExt.c_str());
            Container().SetThumbnailModelUri(modelData.fileUri.c_str());

            m_elements.model->SetModelUri(modelData.fileUri);
            m_currentModelUri = modelData.fileUri;
        }

        void OnModelAsyncStateChanged(VaElementAsyncState newState) {
            switch (newState) {
            case VA_ELEMENT_ASYNC_STATE_READY:
                TRACE("Load model completed, uri = %s", m_currentModelUri.c_str());

                if (m_elements.centerCard) {
                    // No data when the model is loaded.
                    m_elements.centerCard->SetData(FormatAdaptiveCardData(""));
                }
                break;
            case VA_ELEMENT_ASYNC_STATE_PENDING:
                TRACE("Load model in progress, uri = %s", m_currentModelUri.c_str());

                if (m_elements.centerCard) {
                    m_elements.centerCard->SetData(FormatAdaptiveCardData("Loading ..."));
                }
                break;
            case VA_ELEMENT_ASYNC_STATE_ERROR:
                TRACE("Load model has error, uri = %s", m_currentModelUri.c_str());

                // In the error case we may need to create the single model Adaptive Card.
                if (m_elements.centerCard) {
                    m_elements.centerCard->SetData(FormatAdaptiveCardData("Model load failed."));
                } else {
                    m_elements.centerCard = CreateElement<va::AdaptiveCardElement>();
                    m_elements.centerCard->SetTemplate(s_SingleModelTemplate);
                    m_elements.centerCard->SetData(FormatAdaptiveCardData("Model load failed."));
                }

                // This sample prints out each error to traces.
                m_elements.model->GetAsyncErrors(
                    [](VaElementAsyncError error, const char* errorMessage) { TRACE("Model loading error: %s. Error message : %s", va::ToString(error), errorMessage); });
                break;
            default:
                break;
            }

            UpdateModelSelection();
        }

        void OnAdaptiveCardAction(const std::string& verb, const std::string& /*data*/) {
            TRACE("Button was clicked! verb = %s", verb.c_str());
            if (verb == "inc") {
                m_fileCollection->Next();
            } else if (verb == "dec") {
                m_fileCollection->Previous();
            }

            UpdateModelSelection();
        }

    private:
        const std::unique_ptr<IFileCollection> m_fileCollection;

        std::string m_currentModelUri;

        struct {
            va::VisualElement* visual;
            va::ModelResource* model;
            va::AdaptiveCardElement* centerCard;
        } m_elements = {};
    };

} // namespace

int WINAPI WinMain(HINSTANCE, HINSTANCE, LPSTR, int) {
    auto argv = va::windows::GetCommandLineArgs();

    // Prepare a list of glb files from the command line arguments
    auto fileCollection = CreateFileCollection(argv);

    va::AppCreateInfo createInfo{};
    createInfo.applicationName = "cpp_model_viewer";
    createInfo.requiredExtensions = {VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME,
                                     VA_EXT_ADAPTIVE_CARD_ELEMENT_EXTENSION_NAME,
                                     VA_EXT_VOLUME_CONTAINER_MODES_EXTENSION_NAME,
                                     VA_EXT_VOLUME_CONTAINER_THUMBNAIL_EXTENSION_NAME};

    auto app = va::CreateVolumetricApp(std::move(createInfo));

    app->onStart = [&](va::VolumetricApp& app) { app.CreateVolume<ModelViewerSample>(std::move(fileCollection)); };
    app->onRestoreVolumeRequest = [&](va::VolumetricApp& app, const VaUuid& restoreId) { app.CreateVolume<ModelViewerSample>(restoreId); };

    return app->Run();
}
