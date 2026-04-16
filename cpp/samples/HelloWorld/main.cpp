#include <VolumetricApp.h>

int main() {
    auto app = va::CreateVolumetricApp({"hello_world_cpp",
                                        {
                                            VA_EXT_GLTF2_MODEL_RESOURCE_EXTENSION_NAME,
                                        }});
    app->onStart = [](va::VolumetricApp& app) {
        auto volume = app.CreateVolume<va::Volume>();
        volume->onReady = [](va::Volume& volume) {
            auto uri = va::windows::GetLocalAssetUri("world.glb");
            auto model = volume.CreateElement<va::ModelResource>(uri);
            volume.CreateElement<va::VisualElement>(*model);
        };
        volume->onClose = [&app](auto&) { app.RequestExit(); };
    };
    return app->Run();
}
