using Microsoft.MixedReality.Volumetric;

namespace CsShapeSpawner;

internal class ShapeManager
{
    private readonly List<SpawnedShape> _activeShapes = new();
    private readonly LabelManager _labelManager;
    private readonly string _templateUri;
    private readonly Volume _volume;
    private bool _wireframeMeshWritten;
    private readonly WireframeManager _wireframe;

    public IReadOnlyList<SpawnedShape> ActiveShapes => _activeShapes;

    public ShapeManager(Volume volume, string templateUri, LabelManager labelManager, WireframeManager wireframe)
    {
        _volume = volume;
        _templateUri = templateUri;
        _labelManager = labelManager;
        _wireframe = wireframe;
    }

    public void SpawnShape(VaVector3f position)
    {
        if (_activeShapes.Count >= Constants.MaxActiveShapes) return;

        try
        {
            SpawnShapeCore(position);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error spawning shape: {ex}");
        }
    }

    private void SpawnShapeCore(VaVector3f position)
    {
        var (colorName, color) = ColorHelper.GetRandomColor();
        var shapeName = ColorHelper.GetRandomShape();
        var linearColor = ColorHelper.SRGBToLinear(color);

        // Load the shape-specific GLB (cube.glb, sphere.glb, etc.)
        var shapeUri = VolumetricApp.GetAssetUri($"{shapeName.ToLowerInvariant()}.glb");
        var shapeModel = new ModelResource(_volume, shapeUri);
        var shapeVisual = new VisualElement(_volume, shapeModel);
        shapeVisual.SetPosition(position);
        shapeVisual.SetScale(0);

        var shapeMaterial = new MaterialResource(shapeModel, "mat");
        shapeMaterial.SetBaseColorFactor(linearColor);
        shapeMaterial.SetMetallicFactor(0.3f);
        shapeMaterial.SetRoughnessFactor(0.5f);

        // Create label
        var labelPos = new VaVector3f
        {
            x = position.x,
            y = position.y + Constants.LabelOffsetY,
            z = position.z,
        };
        var (labelModel, labelVisual, labelMaterial, labelTexture) =
            _labelManager.CreateLabel(_volume, _templateUri, colorName, shapeName, labelPos);

        var shape = new SpawnedShape
        {
            ColorName = colorName,
            ShapeName = shapeName,
            Position = position,
            ShapeModel = shapeModel,
            ShapeVisual = shapeVisual,
            ShapeMaterial = shapeMaterial,
            LabelModel = labelModel,
            LabelVisual = labelVisual,
            LabelMaterial = labelMaterial,
            LabelTexture = labelTexture,
        };

        _activeShapes.Add(shape);
        Console.WriteLine($"Spawned: {colorName} {shapeName} at ({position.x:F3}, {position.y:F3}, {position.z:F3})");
    }

    public void SpawnAtRandom()
    {
        var rng = new Random();
        float half = Constants.VolumeSize / 2f * 0.7f; // 70% of volume to avoid edges
        var pos = new VaVector3f
        {
            x = (float)(rng.NextDouble() * 2 - 1) * half,
            y = (float)(rng.NextDouble() * 2 - 1) * half,
            z = (float)(rng.NextDouble() * 2 - 1) * half,
        };
        SpawnShape(pos);
    }

    public void PokeShape(int index)
    {
        if (index >= 0 && index < _activeShapes.Count)
        {
            var shape = _activeShapes[index];
            if (shape.State == ShapeState.Alive)
            {
                shape.BeginDestroy();
                Console.WriteLine($"Poked: {shape.ColorName} {shape.ShapeName} (#{index})");
            }
        }
    }

    public void Update(float deltaTime)
    {
        try
        {
            UpdateCore(deltaTime);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in ShapeManager.Update: {ex}");
        }
    }

    private void UpdateCore(float deltaTime)
    {
        // Try writing wireframe mesh once
        if (!_wireframeMeshWritten)
        {
            _wireframeMeshWritten = _wireframe.TryWriteMesh();
        }

        // Update animations
        UpdateAnimations(deltaTime);

        // Update label billboard orientations
        _labelManager.UpdateBillboards(_activeShapes);
    }

    public void DumpState()
    {
        Console.WriteLine($"--- Active shapes: {_activeShapes.Count} ---");
        for (int i = 0; i < _activeShapes.Count; i++)
        {
            var s = _activeShapes[i];
            Console.WriteLine($"  [{i}] {s.ColorName} {s.ShapeName} @ ({s.Position.x:F3},{s.Position.y:F3},{s.Position.z:F3}) state={s.State}");
        }
    }

    private void UpdateAnimations(float deltaTime)
    {
        for (int i = _activeShapes.Count - 1; i >= 0; i--)
        {
            var shape = _activeShapes[i];

            switch (shape.State)
            {
                case ShapeState.ScalingUp:
                    shape.AnimProgress += deltaTime / Constants.ScaleUpDuration;
                    if (shape.AnimProgress >= 1f)
                    {
                        shape.AnimProgress = 1f;
                        shape.State = ShapeState.Alive;
                    }
                    float scaleUp = EaseOutCubic(shape.AnimProgress) * Constants.ShapeSize;
                    shape.ShapeVisual.SetScale(scaleUp);
                    SetLabelScale(shape, EaseOutCubic(shape.AnimProgress));
                    break;

                case ShapeState.ScalingDown:
                    shape.AnimProgress += deltaTime / Constants.ScaleDownDuration;
                    if (shape.AnimProgress >= 1f)
                    {
                        shape.DestroyElements();
                        _activeShapes.RemoveAt(i);
                        Console.WriteLine($"Destroyed: {shape.ColorName} {shape.ShapeName}");
                    }
                    else
                    {
                        float scaleDown = (1f - EaseInCubic(shape.AnimProgress)) * Constants.ShapeSize;
                        shape.ShapeVisual.SetScale(scaleDown);
                        SetLabelScale(shape, 1f - EaseInCubic(shape.AnimProgress));
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Sets label scale as a flat plane: wide in X, thin in Z, paper-thin in Y.
    /// Negative Y flips the cube's UVs so the texture reads right-side up.
    /// </summary>
    private static void SetLabelScale(SpawnedShape shape, float t)
    {
        shape.LabelVisual.SetScale(new VaVector3f
        {
            x = t * Constants.LabelWidth,
            y = -(t * Constants.LabelHeight),
            z = 0.001f, // Paper thin
        });
    }

    private static float EaseOutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);
    private static float EaseInCubic(float t) => t * t * t;
}
