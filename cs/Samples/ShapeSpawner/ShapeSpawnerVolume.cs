using Microsoft.MixedReality.Volumetric;

namespace CsShapeSpawner;

internal class ShapeSpawnerVolume
{
    private readonly Volume _volume;
    private readonly VolumetricApp _app;
    private readonly string _templateUri;

    private WireframeManager? _wireframe;
    private HandInteractionManager? _handInteraction;
    private LabelManager? _labelManager;
    private ShapeManager? _shapeManager;

    private float _lastFrameTime;

    public Volume Volume => _volume;
    public ShapeManager? ShapeManagerInstance => _shapeManager;

    public ShapeSpawnerVolume(VolumetricApp app)
    {
        _app = app;
        _templateUri = VolumetricApp.GetAssetUri("template.glb");

        _volume = new Volume(app)
        {
            OnReady = HandleOnReady,
            OnUpdate = HandleOnUpdate,
            OnClose = _ =>
            {
                LabelTextureCache.Cleanup();
                _app.RequestExit();
            },
        };
    }

    private void HandleOnReady(Volume volume)
    {
        // Configure volume
        volume.Content.SetSizeBehavior(VaVolumeSizeBehavior.Fixed);
        volume.Content.SetSize(Constants.VolumeSize);
        volume.Container.SetDisplayName("Shape Spawner");
        volume.Container.AllowInteractiveMode(true);
        volume.RequestUpdate(VaVolumeUpdateMode.FullFramerate);

        // Create wireframe
        _wireframe = new WireframeManager();
        _wireframe.Create(volume, _templateUri);

        // Create hand interaction
        _handInteraction = new HandInteractionManager();
        _handInteraction.Create(volume);

        // Create label manager
        _labelManager = new LabelManager();
        _labelManager.Create(volume);

        // Create shape manager
        _shapeManager = new ShapeManager(volume, _templateUri, _labelManager, _wireframe);

        // Wire up pinch → spawn
        _handInteraction.OnPinchReleased += pos => _shapeManager.SpawnShape(pos);

        _lastFrameTime = (float)volume.FrameState.frameTime * 1e-9f;
    }

    private void HandleOnUpdate(Volume volume)
    {
        try
        {
            HandleOnUpdateCore(volume);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in HandleOnUpdate: {ex}");
        }
    }

    private void HandleOnUpdateCore(Volume volume)
    {
        float currentTime = (float)volume.FrameState.frameTime * 1e-9f;
        float deltaTime = currentTime - _lastFrameTime;
        _lastFrameTime = currentTime;

        // Clamp delta to avoid large jumps
        deltaTime = MathF.Min(deltaTime, 0.1f);

        var volumeSize = new VaExtent3Df
        {
            width = Constants.VolumeSize,
            height = Constants.VolumeSize,
            depth = Constants.VolumeSize,
        };

        // Update hand interaction (pinch + poke detection)
        _handInteraction?.Update(currentTime, volumeSize, _shapeManager!.ActiveShapes);

        // Update shape animations, mesh writes, and label billboards
        _shapeManager?.Update(deltaTime);
    }
}
