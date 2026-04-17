using Microsoft.MixedReality.Volumetric;

namespace CsShapeSpawner;

internal static class ColorHelper
{
    public static readonly (string Name, VaColor4f Color)[] Colors =
    [
        ("Red",    new VaColor4f { r = 1.0f, g = 0.1f, b = 0.1f, a = 1.0f }),
        ("Blue",   new VaColor4f { r = 0.1f, g = 0.3f, b = 1.0f, a = 1.0f }),
        ("Green",  new VaColor4f { r = 0.1f, g = 0.8f, b = 0.2f, a = 1.0f }),
        ("Yellow", new VaColor4f { r = 1.0f, g = 0.9f, b = 0.1f, a = 1.0f }),
        ("Purple", new VaColor4f { r = 0.6f, g = 0.1f, b = 0.9f, a = 1.0f }),
        ("Orange", new VaColor4f { r = 1.0f, g = 0.5f, b = 0.0f, a = 1.0f }),
        ("Cyan",   new VaColor4f { r = 0.0f, g = 0.9f, b = 0.9f, a = 1.0f }),
        ("Pink",   new VaColor4f { r = 1.0f, g = 0.4f, b = 0.7f, a = 1.0f }),
        ("White",  new VaColor4f { r = 1.0f, g = 1.0f, b = 1.0f, a = 1.0f }),
    ];

    public static readonly string[] ShapeNames = ["Cube", "Sphere", "Cylinder", "Cone", "Pyramid"];

    private static readonly Random _random = new();

    public static (string colorName, VaColor4f color) GetRandomColor()
    {
        var entry = Colors[_random.Next(Colors.Length)];
        return (entry.Name, entry.Color);
    }

    public static string GetRandomShape()
    {
        return ShapeNames[_random.Next(ShapeNames.Length)];
    }

    public static VaColor4f SRGBToLinear(VaColor4f srgb)
    {
        return new VaColor4f
        {
            r = SRGBChannelToLinear(srgb.r),
            g = SRGBChannelToLinear(srgb.g),
            b = SRGBChannelToLinear(srgb.b),
            a = srgb.a,
        };
    }

    private static float SRGBChannelToLinear(float c)
    {
        return c <= 0.04045f
            ? c / 12.92f
            : MathF.Pow((c + 0.055f) / 1.055f, 2.4f);
    }
}
