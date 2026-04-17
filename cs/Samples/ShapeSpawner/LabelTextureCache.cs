using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace CsShapeSpawner;

internal static class LabelTextureCache
{
    private static readonly Dictionary<(string color, string shape), string> _cache = new();
    private static readonly string _cacheDir = Path.Combine(Path.GetTempPath(), "ShapeSpawner");

    public static string GetOrCreate(string colorName, string shapeName)
    {
        var key = (colorName, shapeName);
        if (_cache.TryGetValue(key, out var uri))
            return uri;

        uri = GenerateLabelTexture(colorName, shapeName);
        _cache[key] = uri;
        return uri;
    }

    public static void Cleanup()
    {
        try
        {
            if (Directory.Exists(_cacheDir))
                Directory.Delete(_cacheDir, true);
        }
        catch
        {
            // Best effort cleanup
        }
    }

    private static string GenerateLabelTexture(string colorName, string shapeName)
    {
        int width = 512, height = 128;

#pragma warning disable CA1416 // Platform compatibility - Windows only app
        using var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var gfx = Graphics.FromImage(bmp);

        gfx.SmoothingMode = SmoothingMode.AntiAlias;
        gfx.TextRenderingHint = TextRenderingHint.AntiAlias;
        gfx.Clear(Color.Transparent);

        // Draw rounded rectangle background
        using var bgBrush = new SolidBrush(Color.FromArgb(200, 30, 30, 30));
        using var path = CreateRoundedRect(0, 0, width, height, 20);
        gfx.FillPath(bgBrush, path);

        // Draw text centered
        string text = $"{colorName} {shapeName}";
        using var font = new Font("Segoe UI", 36, FontStyle.Bold, GraphicsUnit.Pixel);
        using var textBrush = new SolidBrush(Color.White);
        var sf = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        gfx.DrawString(text, font, textBrush, new RectangleF(0, 0, width, height), sf);

        // Save
        Directory.CreateDirectory(_cacheDir);
        string filePath = Path.Combine(_cacheDir, $"{colorName}_{shapeName}.png");
        bmp.Save(filePath, ImageFormat.Png);
#pragma warning restore CA1416

        return new Uri(filePath).AbsoluteUri;
    }

    private static GraphicsPath CreateRoundedRect(float x, float y, float w, float h, float r)
    {
        var gp = new GraphicsPath();
        gp.AddArc(x, y, r * 2, r * 2, 180, 90);
        gp.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
        gp.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
        gp.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
        gp.CloseFigure();
        return gp;
    }
}
