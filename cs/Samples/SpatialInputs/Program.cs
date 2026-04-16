using CsBoids;
using Microsoft.MixedReality.Volumetric;

internal sealed class Program
{
    static void Main()
    {
        SpatialInputs? spatialInputs = null;

        var app = new VolumetricApp("cs_spatial_input",
            requiredExtensions: new string[] {
                Extensions.VA_EXT_gltf2_model_resource,
                Extensions.VA_EXT_locate_spaces,
                Extensions.VA_EXT_locate_joints,
                Extensions.VA_EXT_volume_container_modes
            });
        app.OnStart += __ =>
        {
            _ = new Volume(app)
            {
                OnReady = volume =>
                {
                    volume.RequestUpdate(VaVolumeUpdateMode.FullFramerate);
                    spatialInputs = new SpatialInputs(volume);
                    spatialInputs.OnReady();

                    volume.Content.SetSizeBehavior(VaVolumeSizeBehavior.Fixed);
                    volume.Content.SetSize(1.0f); // Scale to meters
                    volume.Container.AllowInteractiveMode(true);
                },
                OnUpdate = _ =>
                {
                    spatialInputs?.OnUpdate();
                },
                OnClose = _ =>
                {
                    spatialInputs = null;
                    app.RequestExit();
                }
            };
        };
        app.Run();
    }
}
