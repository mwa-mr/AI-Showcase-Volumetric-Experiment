using Microsoft.MixedReality.Volumetric;

namespace CsShapeSpawner;

/// <summary>
/// Builds a wireframe cube from 12 thin stretched template cubes (one per edge).
/// This avoids the unreliable WriteMeshBuffers resize path entirely.
/// </summary>
internal class WireframeManager
{
    private ModelResource? _model;
    private MaterialResource? _material;
    private readonly List<VisualElement> _edges = new();

    public void Create(Volume volume, string templateUri)
    {
        _model = new ModelResource(volume, templateUri);

        // Set wireframe color on the shared model material
        _material = new MaterialResource(_model, "mat");
        var wireColor = ColorHelper.SRGBToLinear(new VaColor4f { r = 0.0f, g = 0.9f, b = 0.9f, a = 1.0f });
        _material.SetBaseColorFactor(wireColor);
        _material.SetMetallicFactor(0.0f);
        _material.SetRoughnessFactor(0.8f);

        float half = Constants.VolumeSize / 2f;
        float t = Constants.WireThickness;
        float s = Constants.VolumeSize;

        // 4 edges along X axis (at each Y/Z corner)
        foreach (float y in new[] { -half, half })
        foreach (float z in new[] { -half, half })
            AddEdge(volume, new VaVector3f { x = 0, y = y, z = z },
                            new VaVector3f { x = s, y = t, z = t });

        // 4 edges along Y axis (at each X/Z corner)
        foreach (float x in new[] { -half, half })
        foreach (float z in new[] { -half, half })
            AddEdge(volume, new VaVector3f { x = x, y = 0, z = z },
                            new VaVector3f { x = t, y = s, z = t });

        // 4 edges along Z axis (at each X/Y corner)
        foreach (float x in new[] { -half, half })
        foreach (float y in new[] { -half, half })
            AddEdge(volume, new VaVector3f { x = x, y = y, z = 0 },
                            new VaVector3f { x = t, y = t, z = s });

        Console.WriteLine($"Wireframe created: {_edges.Count} edge beams");
    }

    private void AddEdge(Volume volume, VaVector3f position, VaVector3f scale)
    {
        var visual = new VisualElement(volume, _model!);
        visual.SetPosition(position);
        visual.SetScale(scale);
        _edges.Add(visual);
    }

    // No mesh writing needed — edges use the template cube geometry directly
    public bool TryWriteMesh() => true;
}
