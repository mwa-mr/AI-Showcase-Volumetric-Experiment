using Microsoft.MixedReality.Volumetric;

namespace CsSubElements;

public class SpinningModel : Volume
{
    // Reference to following Gltf file on GitHub for node name and scene structures.
    // https://github.com/KhronosGroup/glTF-Sample-Models/blob/main/2.0/AntiqueCamera/glTF/AntiqueCamera.gltf
    private readonly string _modelUri = VolumetricApp.GetAssetUri("AntiqueCamera.glb");

    private ModelResource? _model;
    private VisualElement? _visual;
    private VisualElement? _cameraNode;
    private VisualElement? _tripodNode;

    public SpinningModel(VolumetricApp app) : base(app)
    {
        OnReady += HandleOnReady;
        OnUpdate += (_) => HandleOnUpdate();
        OnClose += (_) => app.RequestExit();
    }

    private void HandleOnReady(Volume volume)
    {
        _model = new ModelResource(volume, _modelUri);
        _visual = new VisualElement(volume, _model);
        _cameraNode = new VisualElement(volume, _visual, "camera");
        _tripodNode = new VisualElement(volume, _visual, "tripod");

        // Request a repeated OnUpdate event for rotating the model.
        volume.RequestUpdate(VaVolumeUpdateMode.FullFramerate);
    }

    private void HandleOnUpdate()
    {
        float seconds = (float)FrameState.frameTime * 1e-9f;
        float angle = seconds * MathF.PI / 2;
        VaQuaternionf quaternion = new VaQuaternionf() { x = 0, y = MathF.Sin(angle / 2), z = 0, w = MathF.Cos(angle / 2) };
        _cameraNode?.SetOrientation(in quaternion);

        quaternion.y *= -1; // Rotate the tripod in the opposite direction
        _tripodNode?.SetOrientation(in quaternion);
    }
}

internal sealed class Program
{
    static int Main()
    {
        var app = new VolumetricApp("CsNamedNodesSample",
            requiredExtensions: new[] {
                Extensions.VA_EXT_gltf2_model_resource });
        return app.Run(onStart: app => _ = new SpinningModel(app));
    }
}
