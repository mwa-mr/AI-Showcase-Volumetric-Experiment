using Microsoft.MixedReality.Volumetric;

sealed class Program
{
    static int Main()
    {
        var app = new VolumetricApp("cs_hello_world",
            requiredExtensions: new string[] { Extensions.VA_EXT_gltf2_model_resource, });
        app.OnStart += __ =>
        {
            var volume = new Volume(app);
            volume.OnReady += _ =>
            {
                var uri = VolumetricApp.GetAssetUri("world.glb");
                var model = new ModelResource(volume, uri);
                var visual = new VisualElement(volume, model);
            };
            volume.OnClose += _ => app.RequestExit();
        };
        return app.Run();
    }
}
