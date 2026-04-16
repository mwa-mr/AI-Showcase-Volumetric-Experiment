using Microsoft.MixedReality.Volumetric;

namespace CsMultipleVolumes;

public class SimpleVolume : Volume
{
    private readonly string _modelUri;
    private readonly float _scale = 1.0f;

    private ModelResource? _axisModelResource;
    private VisualElement? _axisVisualElement;
    private ModelResource? _modelResource;
    private VisualElement? _visualElement;

    public SimpleVolume(VolumetricApp app, string modelUri, float scale = 1.0f) : base(app)
    {
        _modelUri = modelUri;
        _scale = scale;
        OnReady += HandleOnReady;
    }

    private void HandleOnReady(Volume volume)
    {
        _axisModelResource = new ModelResource(volume, VolumetricApp.GetAssetUri("axis_xyz_rub.glb"));
        _axisVisualElement = new VisualElement(volume, _axisModelResource);

        _modelResource = new ModelResource(volume, _modelUri);
        _visualElement = new VisualElement(volume, _modelResource);
        _visualElement.SetScale(_scale);
    }
}

internal sealed class Program
{
    static void Main()
    {
        // Enable the C# library to trace into the console and observe what's happening in the Library.
        VaTrace.EnableTraceToConsole = true;

        var app = new VolumetricApp("MultipleVolumetricApp",
            requiredExtensions: new string[] { Extensions.VA_EXT_gltf2_model_resource, });
        app.OnStart += __ =>
        {
            _ = new SimpleVolume(app, VolumetricApp.GetAssetUri("BoomBox.glb"), 50.0f);
            _ = new SimpleVolume(app, VolumetricApp.GetAssetUri("BoxAnimated.glb"));
        };
        app.OnFatalError += (errorMessage) =>
        {
            Console.WriteLine($"Fatal error: {errorMessage}");
        };

        while (app.PollEvents())
        {
            // If all volumes are closed, request exiting the app
            if (app.IsStarted && app.Volumes.Count == 0)
            {
                app.RequestExit();
            }
            Thread.Sleep(100);
        }
    }
}
