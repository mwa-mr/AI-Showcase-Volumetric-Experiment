using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

public sealed class XamlSnapshot : IDisposable
{
    public enum ImageFormat { Png, Jpeg }

    public class Options
    {
        public string FilePath { get; init; } = nameof(FilePath);
        public long MaxFileSize { get; init; } // e.g. 5_000_000 (bytes)
        public ImageFormat Format { get; init; } = ImageFormat.Png;
        public double JpegQuality { get; init; } = 0.9;   // only if Format==Jpeg
        public Action<FrameworkElement>? BeforeCapture { get; init; }
        public Action<FrameworkElement>? AfterCapture { get; init; }
    }

    readonly FrameworkElement _root;
    readonly Options _opts;
    readonly RenderTargetBitmap _rtb;
    readonly InMemoryRandomAccessStream _memStream;
    MemoryMappedFile _mmf;
    MemoryMappedViewStream _mmViewStream;

    public XamlSnapshot(FrameworkElement root, Options opts)
    {
        _root = root;
        _opts = opts;
        _rtb = new RenderTargetBitmap();
        _memStream = new InMemoryRandomAccessStream();
    }

    private void OpenMapping()
    {
        // Prep disk file + MMF
        Directory.CreateDirectory(Path.GetDirectoryName(_opts.FilePath)!);
        var fs = new FileStream(
            _opts.FilePath,
            FileMode.Create,
            FileAccess.ReadWrite,
            FileShare.ReadWrite);
        fs.SetLength(_opts.MaxFileSize);

        _mmf = MemoryMappedFile.CreateFromFile(
            fs,
            mapName: null,
            capacity: _opts.MaxFileSize,
            access: MemoryMappedFileAccess.ReadWrite,
            inheritability: HandleInheritability.None,
            leaveOpen: false);

        _mmViewStream = _mmf.CreateViewStream(
            offset: 0,
            size: _opts.MaxFileSize,
            access: MemoryMappedFileAccess.ReadWrite);
    }

    public async Task CaptureAsync()
    {
        // 1) Hide/show
        _opts.BeforeCapture?.Invoke(_root);

        // 2) Force layout
        _root.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        _root.Arrange(new Rect(0, 0, _root.DesiredSize.Width, _root.DesiredSize.Height));
        _root.UpdateLayout();

        if (_root.ActualWidth <= 0 || _root.ActualHeight <= 0)
        {
            return; // Nothing to capture, skip
        }
        try
        {
            // 3) Render tree -> bitmap
            await _rtb.RenderAsync(_root);

            var pixelBuffer = await _rtb.GetPixelsAsync();
            var pixels = pixelBuffer.ToArray();

            // 4) Encode to PNG/JPEG in-memory
            _memStream.Size = 0;
            _memStream.Seek(0);

            var encoderId = _opts.Format == ImageFormat.Png
                ? BitmapEncoder.PngEncoderId
                : BitmapEncoder.JpegEncoderId;

            var encoder = await BitmapEncoder.CreateAsync(encoderId, _memStream);
            var dpi = (_root.XamlRoot?.RasterizationScale ?? 1.0) * 96.0;


            encoder.SetPixelData(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                (uint)_rtb.PixelWidth,
                (uint)_rtb.PixelHeight,
                dpi, dpi,
                pixels);

            if (_opts.Format == ImageFormat.Jpeg)
                encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;

            await encoder.FlushAsync();

            OpenMapping();
            // 5) Copy encoded bytes into the pre‐mapped file
            _mmViewStream.Position = 0;
            _memStream.Seek(0);

            // Use .AsStream() to bridge to .NET streams
            var src = _memStream.AsStream();
            await src.CopyToAsync(_mmViewStream);

            CloseMapping();
            // 6) Restore UI
            _opts.AfterCapture?.Invoke(_root);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error capturing XAML snapshot: {ex.Message}");
        }
    }

    public void CloseMapping()
    {
        _mmViewStream?.Dispose();
        _mmf?.Dispose();
    }

    public void Dispose()
    {
        _mmViewStream.Dispose();
        _mmf.Dispose();
        _memStream.Dispose();
    }
}
