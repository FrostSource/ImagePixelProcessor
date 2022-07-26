using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ImagePixelProcessor;

/// <summary>
/// A fast access bitmap for processing.
/// </summary>
/// <remarks>
/// Source found at https://stackoverflow.com/a/34801225/15190248
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed class DirectBitmap : IDisposable
{
    /// <summary>
    /// The underlying <see cref="Bitmap"/> used by this object.
    /// </summary>
    public Bitmap InternalBitmap { get; private set; }
    /// <summary>
    /// Height of the bitmap in pixels.
    /// </summary>
    public int Height => InternalBitmap.Height;
    /// <summary>
    /// Width of the bitmap in pixels.
    /// </summary>
    public int Width => InternalBitmap.Width;

    private bool Disposed = false;
    private readonly int[] Bits;
    private GCHandle BitsHandle;

    /// <summary>
    /// Create a new <see cref="DirectBitmap"/> with a specified size.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public DirectBitmap(int width, int height)
    {
        Bits = new int[width * height];
        BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
        InternalBitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, BitsHandle.AddrOfPinnedObject());
    }
    /// <summary>
    /// Creates a new <see cref="DirectBitmap"/> from an existing <see cref="Bitmap"/> by cloning it.
    /// </summary>
    /// <param name="bitmap"></param>
    public DirectBitmap(Bitmap bitmap) : this(bitmap.Width, bitmap.Height)
    {
        InternalBitmap.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);
        using var image = Graphics.FromImage(InternalBitmap);
        image.DrawImage(bitmap, 0, 0);
        image.Flush();
    }
    /// <summary>
    /// Creates a new <see cref="DirectBitmap"/> from an existing <see cref="DirectBitmap"/> by cloning it.
    /// </summary>
    /// <param name="bitmap"></param>
    public DirectBitmap(DirectBitmap bitmap) : this(bitmap.InternalBitmap) { }

    /// <summary>
    /// Sets a pixel color at [x,y] position.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="colour"></param>
    public void SetPixel(int x, int y, Color colour)
    {
        var index = x + (y * Width);
        var col = colour.ToArgb();

        Bits[index] = col;
    }
    /// <summary>
    /// Gets a pixel color from [x,y] position.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Color GetPixel(int x, int y)
    {
        var index = x + (y * Width);
        var col = Bits[index];
        var result = Color.FromArgb(col);

        return result;
    }

    /// <summary>
    /// Saves the bitmap to a stream.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="format"></param>
    public void Save(Stream stream, ImageFormat format)
    {
        InternalBitmap.Save(stream, format);
    }
    /// <summary>
    /// Saves the bitmap to a path.
    /// </summary>
    /// <param name="path"></param>
    public void Save(string path)
    {
        InternalBitmap.Save(path);
    }

    /// <summary>
    /// Disposes of the bitmap.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        Disposed = true;
        InternalBitmap.Dispose();
        BitsHandle.Free();
    }

    /// <summary>
    /// Converts a <see cref="Bitmap"/> object to a <see cref="DirectBitmap"/> object.
    /// </summary>
    /// <param name="bitmap"></param>
    public static implicit operator DirectBitmap(Bitmap bitmap) => new DirectBitmap(bitmap);
}
