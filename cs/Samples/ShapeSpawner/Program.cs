using Microsoft.MixedReality.Volumetric;

namespace CsShapeSpawner;

internal sealed class Program
{
    static int Main(string[] args)
    {
        bool desktopTest = args.Contains("--desktop-test");

        var app = new VolumetricApp("CsShapeSpawner",
            requiredExtensions:
            [
                Extensions.VA_EXT_gltf2_model_resource,
                Extensions.VA_EXT_mesh_edit,
                Extensions.VA_EXT_material_resource,
                Extensions.VA_EXT_texture_resource,
                Extensions.VA_EXT_locate_joints,
                Extensions.VA_EXT_locate_spaces,
                Extensions.VA_EXT_volume_container_modes,
            ]);

        ShapeSpawnerVolume? spawnerVolume = null;

        return app.Run(onStart: _ =>
        {
            spawnerVolume = new ShapeSpawnerVolume(app);

            if (desktopTest)
            {
                DesktopTestMode.Start(
                    app,
                    () => spawnerVolume.Volume,
                    () => spawnerVolume.ShapeManagerInstance);
            }
        });
    }
}
