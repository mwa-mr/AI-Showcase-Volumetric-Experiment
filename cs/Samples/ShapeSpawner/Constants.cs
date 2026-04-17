namespace CsShapeSpawner;

internal static class Constants
{
    // Volume
    public const float VolumeSize = 0.4f;

    // Pinch detection
    public const float PinchStartThreshold = 0.02f;
    public const float PinchReleaseThreshold = 0.04f;
    public const float PinchCooldown = 0.5f;

    // Poke detection
    public const float PokeThreshold = 0.03f;

    // Shape sizing
    public const float ShapeSize = 0.04f;
    public const float LabelOffsetY = 0.05f;
    public const float LabelWidth = 0.0762f;   // 3 inches
    public const float LabelHeight = 0.0254f;  // 1 inch

    // Wireframe
    public const float WireThickness = 0.003f;

    // Animations
    public const float ScaleUpDuration = 0.3f;
    public const float ScaleDownDuration = 0.15f;

    // Limits
    public const int MaxActiveShapes = 20;
}
