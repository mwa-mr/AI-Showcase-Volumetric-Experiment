using Microsoft.MixedReality.Volumetric;

namespace CsShapeSpawner;

internal enum ShapeState
{
    ScalingUp,
    Alive,
    ScalingDown,
}

internal class SpawnedShape
{
    public required string ColorName { get; init; }
    public required string ShapeName { get; init; }
    public required VaVector3f Position { get; init; }

    public ShapeState State { get; set; } = ShapeState.ScalingUp;
    public float AnimProgress { get; set; }
    public bool ShapeMeshWritten => true; // Shapes load from GLB directly, always "ready"

    // Shape elements
    public required ModelResource ShapeModel { get; init; }
    public required VisualElement ShapeVisual { get; init; }
    public required MaterialResource ShapeMaterial { get; init; }

    // Label elements
    public required ModelResource LabelModel { get; init; }
    public required VisualElement LabelVisual { get; init; }
    public required MaterialResource LabelMaterial { get; init; }
    public required TextureResource LabelTexture { get; init; }

    public VaVector3f LabelPosition => new VaVector3f
    {
        x = Position.x,
        y = Position.y + Constants.LabelOffsetY,
        z = Position.z,
    };

    public void BeginDestroy()
    {
        if (State != ShapeState.ScalingDown)
        {
            State = ShapeState.ScalingDown;
            AnimProgress = 0f;
        }
    }

    public void DestroyElements()
    {
        LabelTexture.Destroy();
        LabelMaterial.Destroy();
        LabelVisual.Destroy();
        LabelModel.Destroy();

        ShapeMaterial.Destroy();
        ShapeVisual.Destroy();
        ShapeModel.Destroy();
    }
}
