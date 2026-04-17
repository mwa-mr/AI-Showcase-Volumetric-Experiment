using Microsoft.MixedReality.Volumetric;

namespace CsShapeSpawner;

/// <summary>
/// Labels use the template cube flattened into a plane via non-uniform scale.
/// No WriteMeshBuffers needed — the cube geometry is squashed flat.
/// </summary>
internal class LabelManager
{
    private SpaceLocator? _locator;

    public void Create(Volume volume)
    {
        _locator = new SpaceLocator(volume);
    }

    public (ModelResource model, VisualElement visual, MaterialResource material, TextureResource texture)
        CreateLabel(Volume volume, string templateUri, string colorName, string shapeName, VaVector3f labelPos)
    {
        var model = new ModelResource(volume, templateUri);
        var visual = new VisualElement(volume, model);
        visual.SetPosition(labelPos);
        // Start invisible (scale 0); animation will apply non-uniform scale
        visual.SetScale(0);

        var material = new MaterialResource(model, "mat");
        material.SetBaseColorFactor(new VaColor4f { r = 1, g = 1, b = 1, a = 1 });
        material.SetMetallicFactor(0.0f);
        material.SetRoughnessFactor(1.0f);

        var texture = new TextureResource(volume);
        string textureUri = LabelTextureCache.GetOrCreate(colorName, shapeName);
        texture.SetImageUri(textureUri);
        material.SetPbrBaseColorTexture(texture);

        return (model, visual, material, texture);
    }

    public void UpdateBillboards(IReadOnlyList<SpawnedShape> activeShapes)
    {
        if (_locator?.IsReady != true) return;
        _locator.Update();

        var viewer = _locator.Locations.viewer;
        if (!viewer.isTracked) return;

        var viewerPos = viewer.pose.position;

        foreach (var shape in activeShapes)
        {
            if (shape.State == ShapeState.ScalingDown) continue;

            var labelPos = shape.LabelPosition;
            float dx = viewerPos.x - labelPos.x;
            float dz = viewerPos.z - labelPos.z;
            float angle = MathF.Atan2(dx, dz);

            var q = new VaQuaternionf
            {
                x = 0,
                y = MathF.Sin(angle / 2),
                z = 0,
                w = MathF.Cos(angle / 2),
            };
            shape.LabelVisual.SetOrientation(in q);
        }
    }
}
