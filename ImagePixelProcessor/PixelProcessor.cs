using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

//using BitmapType = System.Drawing.Bitmap;
using BitmapType = ImagePixelProcessor.DirectBitmap;

namespace ImagePixelProcessor;

/// <summary>
/// Performs actions on an image pixel-by-pixel.
/// </summary>
/// <example>
/// <code>
/// // Get grayscale alpha mask
/// PixelProcessor.New(bitmap)
///     .Shift(ColorChannel.A, "mask", ColorChannel.RGB)
///     .SetValue("mask", ColorChannel.A, 255)
///     .ProcessSave(@".\alpha_{0}", true, ImageFormat.Png)
///     .GetBitmap("mask", out Bitmap bmap)
///     .Finish();
///bmap.Dispose();
/// </code>
/// </example>

[SupportedOSPlatform("windows")]
public sealed class PixelProcessor : IDisposable
{
    private readonly Dictionary<string, BitmapType> ProcessingBitmaps = new();
    private readonly List<Action<int, int>> Actions = new();

    /// <summary>
    /// Gets the main bitmap for this processor.
    /// </summary>
    public BitmapType Bitmap { get; private set; }
    /// <summary>
    /// Width of the image being processed.
    /// </summary>
    public int Width => Bitmap.Width;
    /// <summary>
    /// Height of the image being processed.
    /// </summary>
    public int Height => Bitmap.Height;
    /// <summary>
    /// Gets an array of all existing named bitmaps.
    /// </summary>
    public string[] Names => ProcessingBitmaps.Keys.ToArray();

    /// <summary>
    /// Creates a new <see cref="PixelProcessor"/> object from an existing <see cref="System.Drawing.Bitmap"/>.
    /// </summary>
    /// <param name="bitmap"></param>
    public PixelProcessor(Bitmap bitmap) => Bitmap = new BitmapType(bitmap);
    /// <summary>
    /// Creates a new <see cref="PixelProcessor"/> object from an existing <see cref="DirectBitmap"/>
    /// </summary>
    /// <param name="bitmap"></param>
    public PixelProcessor(DirectBitmap bitmap) => Bitmap = new BitmapType(bitmap);

    /// <summary>
    /// Gets a named bitmap. This is used internally whenever a named bitmap is referenced.
    /// </summary>
    /// <param name="name">Name of the bitmap.</param>
    /// <returns>The named <see cref="BitmapType"/> object.</returns>
    public BitmapType GetProcessingBitmap(string name)
    {
        if (!ProcessingBitmaps.ContainsKey(name))
        {
            ProcessingBitmaps[name] = new BitmapType(Width, Height);
        }
        return ProcessingBitmaps[name];
    }
    /// <summary>
    /// Gets a named bitmap as a <see cref="System.Drawing.Bitmap"/> object.
    /// </summary>
    /// <param name="name">Name of the bitmap.</param>
    /// <returns>The bitmap.</returns>
    public Bitmap GetBitmap(string name)
    {
        return new Bitmap(GetProcessingBitmap(name).InternalBitmap);
    }
    /// <summary>
    /// Gets a named bitmap as a <see cref="System.Drawing.Bitmap"/> object.
    /// </summary>
    /// <param name="name">Name of the bitmap.</param>
    /// <param name="outBitmap">Out bitmap to set.</param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    public PixelProcessor GetBitmap(string name, out Bitmap outBitmap)
    {
        outBitmap = new Bitmap(GetProcessingBitmap(name).InternalBitmap);
        return this;
    }

    #region Pixel operations

    private PixelProcessor Extract(BitmapType bitmap, ColorChannel channel, string output = "")
    {
        BitmapType newBitmap = string.IsNullOrEmpty(output) ? bitmap : GetProcessingBitmap(output);
        Actions.Add((x, y) =>
        {
            Color pixel = bitmap.GetPixel(x, y);
            Color pixel2 = newBitmap.GetPixel(x, y);
            Color color = channel switch
            {
                ColorChannel.A => Color.FromArgb(pixel.A, pixel2.R, pixel2.G, pixel2.B),
                ColorChannel.R => Color.FromArgb(pixel2.A, pixel.R, pixel2.G, pixel2.B),
                ColorChannel.G => Color.FromArgb(pixel2.A, pixel2.R, pixel.G, pixel2.B),
                ColorChannel.B => Color.FromArgb(pixel2.A, pixel2.R, pixel2.G, pixel.B),
                ColorChannel.RGB => Color.FromArgb(pixel2.A, pixel.R, pixel.G, pixel.B),
                _ => throw new NotImplementedException(),
            };
            newBitmap.SetPixel(x, y, color);
        });
        return this;
    }
    /// <summary>
    /// Extracts a color value from a named bitmap into another named bitmap.
    /// </summary>
    /// <param name="name">Named bitmap to extract from.</param>
    /// <param name="channel">Channel to extract.</param>
    /// <param name="output">Named bitmap to extract to.</param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public PixelProcessor Extract(string name, ColorChannel channel, string output)
    {
        return Extract(GetProcessingBitmap(name), channel, output);
    }

    /// <summary>
    /// Extracts a color value from the main bitmap into a named bitmap.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="output"></param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public PixelProcessor Extract(ColorChannel channel, string output)
    {
        return Extract(Bitmap, channel, output);
    }

    /// <inheritdoc cref="Extract(string, ColorChannel, string)"/>
    public PixelProcessor Copy(string name, ColorChannel channel, string output)
    {
        return Extract(GetProcessingBitmap(name), channel, output);
    }
    ///<inheritdoc cref="Extract(ColorChannel, string)"/>
    public PixelProcessor Copy(ColorChannel channel, string output)
    {
        return Extract(Bitmap, channel, output);
    }

    private PixelProcessor Invert(BitmapType bitmap, ColorChannel channel, string output = "")
    {
        BitmapType newBitmap = string.IsNullOrEmpty(output) ? bitmap : GetProcessingBitmap(output);
        Actions.Add((x, y) =>
        {
            Color pixel = bitmap.GetPixel(x, y);
            Color color = channel switch
            {
                ColorChannel.A => Color.FromArgb(255 - pixel.A, pixel.R, pixel.G, pixel.B),
                ColorChannel.R => Color.FromArgb(pixel.A, 255 - pixel.R, pixel.G, pixel.B),
                ColorChannel.G => Color.FromArgb(pixel.A, pixel.R, 255 - pixel.G, pixel.B),
                ColorChannel.B => Color.FromArgb(pixel.A, pixel.R, pixel.G, 255 - pixel.B),
                ColorChannel.RGB => Color.FromArgb(pixel.A, 255 - pixel.R, 255 - pixel.G, 255 - pixel.B),
                _ => throw new NotImplementedException(),
            };
            newBitmap.SetPixel(x, y, color);
        });
        return this;
    }
    /// <summary>
    /// Inverts a color channel of a named bitmap and saves the output to another named bitmap.
    /// Subtracts the current value from 255.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="channel"></param>
    /// <param name="output"></param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public PixelProcessor Invert(string name, ColorChannel channel, string output = "")
    {
        return Invert(GetProcessingBitmap(name), channel, output);
    }
    /// <summary>
    /// Inverts a color channel of the main bitmap and saves the output to a named bitmap.
    /// Subtracts the current value from 255.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="output"></param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public PixelProcessor Invert(ColorChannel channel, string output = "")
    {
        return Invert(Bitmap, channel, output);
    }

    private PixelProcessor Set(BitmapType bitmap, Color color, string output = "")
    {
        BitmapType newBitmap = string.IsNullOrEmpty(output) ? bitmap : GetProcessingBitmap(output);
        Actions.Add((x, y) =>
        {
            newBitmap.SetPixel(x, y, color);
        });
        return this;
    }
    /// <summary>
    /// Sets the pixel of the main bitmap to a new <see cref="Color"/> value.
    /// </summary>
    /// <param name="color"><see cref="Color"/> value to set as.</param>
    /// <param name="output">Named bitmap to output to; or directly to the main bitmap if not specified.</param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    public PixelProcessor Set(Color color, string output = "")
    {
        return Set(Bitmap, color, output);
    }
    /// <summary>
    /// Sets the pixel of the main bitmap to a new <see cref="Color"/> value.
    /// </summary>
    /// <param name="name">Named bitmap to operate on.</param>
    /// <param name="color"><see cref="Color"/> value to set as.</param>
    /// <param name="output">Named bitmap to output to; or directly to the main bitmap if not specified.</param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    public PixelProcessor Set(string name, Color color, string output = "")
    {
        return Set(GetProcessingBitmap(name), color, output);
    }

    private PixelProcessor SetValue(BitmapType bitmap, ColorChannel channel, int value, string output = "")
    {
        BitmapType newBitmap = string.IsNullOrEmpty(output) ? bitmap : GetProcessingBitmap(output);
        value = Math.Clamp(value, 0, 255);
        Actions.Add((x, y) =>
        {
            Color pixel = bitmap.GetPixel(x, y);
            Color color = channel switch
            {
                ColorChannel.A => Color.FromArgb(value, pixel.R, pixel.G, pixel.B),
                ColorChannel.R => Color.FromArgb(pixel.A, value, pixel.G, pixel.B),
                ColorChannel.G => Color.FromArgb(pixel.A, pixel.R, value, pixel.B),
                ColorChannel.B => Color.FromArgb(pixel.A, pixel.R, pixel.G, value),
                ColorChannel.RGB => Color.FromArgb(pixel.A, value, value, value),
                _ => throw new NotImplementedException(),
            };
            newBitmap.SetPixel(x, y, color);
        });
        return this;
    }
    /// <summary>
    /// Sets the value of a named bitmap's color channel and saves the output to another named bitmap.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="channel"></param>
    /// <param name="value">[0-255]</param>
    /// <param name="output"></param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public PixelProcessor SetValue(string name, ColorChannel channel, int value, string output = "")
    {
        return SetValue(GetProcessingBitmap(name), channel, value, output);
    }
    /// <summary>
    /// Sets the value of the main bitmap's color channel and saves the output to a named bitmap.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="value">[0-255]</param>
    /// <param name="output"></param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public PixelProcessor SetValue(ColorChannel channel, int value, string output = "")
    {
        return SetValue(Bitmap, channel, value, output);
    }

    /// <summary>
    /// Merges two named bitmaps together by adding their color values and saves the output in a new named bitmap.
    /// </summary>
    /// <param name="name1"></param>
    /// <param name="name2"></param>
    /// <param name="output">If output is blank then <paramref name="name1"/> will be copied directly into <paramref name="name2"/></param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    public PixelProcessor Merge(string name1, string name2, string output = "")
    {
        BitmapType bitmap1 = GetProcessingBitmap(name1);
        BitmapType bitmap2 = GetProcessingBitmap(name2);
        BitmapType finalBitmap = string.IsNullOrEmpty(output) ? bitmap2 : GetProcessingBitmap(output);
        Actions.Add((x, y) =>
        {
            Color pixel1 = bitmap1.GetPixel(x, y);
            Color pixel2 = bitmap2.GetPixel(x, y);
            finalBitmap.SetPixel(x, y, Color.FromArgb(
                Math.Min(pixel1.A + pixel2.A, 255),
                Math.Min(pixel1.R + pixel2.R, 255),
                Math.Min(pixel1.G + pixel2.G, 255),
                Math.Min(pixel1.B + pixel2.B, 255)
                ));
        });
        return this;
    }

    private PixelProcessor Shift(BitmapType bitmap1, ColorChannel from, BitmapType bitmap2, ColorChannel to)
    {
        Actions.Add((x, y) =>
        {
            Color pixel = bitmap1.GetPixel(x, y);
            //Color pixel2 = bitmap2.GetPixel(x, y);
            int value = from switch
            {
                ColorChannel.A => pixel.A,
                ColorChannel.R => pixel.R,
                ColorChannel.G => pixel.G,
                ColorChannel.B => pixel.B,
                //ColorChannel.RGB => 0,
                ColorChannel.RGB => throw new ArgumentException("RGB is not valid in the 'from' channel."),
                _ => throw new NotImplementedException(),
            };
            Color pixel2 = bitmap2.GetPixel(x, y);
            switch (to)
            {
                case ColorChannel.A:
                    bitmap2.SetPixel(x, y, Color.FromArgb(value, pixel2.R, pixel2.G, pixel2.B));
                    break;
                case ColorChannel.R:
                    bitmap2.SetPixel(x, y, Color.FromArgb(pixel2.A, value, pixel2.G, pixel2.B));
                    break;
                case ColorChannel.G:
                    bitmap2.SetPixel(x, y, Color.FromArgb(pixel2.A, pixel2.R, value, pixel2.B));
                    break;
                case ColorChannel.B:
                    bitmap2.SetPixel(x, y, Color.FromArgb(pixel2.A, pixel2.R, pixel2.G, value));
                    break;
                case ColorChannel.RGB:
                    bitmap2.SetPixel(x, y, Color.FromArgb(pixel2.A, value, value, value));
                    break;
            }
        });
        return this;
    }
    /// <summary>
    /// Copies color channel value of '<paramref name="from"/>' in <paramref name="name1"/> into color channel '<paramref name="to"/>' of <paramref name="name2"/>.
    /// </summary>
    /// <param name="name1">Name of the bitmap to shift from.</param>
    /// <param name="from">Must be a single channel, may not be <see cref="ColorChannel.RGB"/>.</param>
    /// <param name="name2">Name of the bitmap to shift into.</param>
    /// <param name="to">The channel to copy into. May be <see cref="ColorChannel.RGB"/></param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public PixelProcessor Shift(string name1, ColorChannel from, string name2, ColorChannel to)
    {
        return Shift(GetProcessingBitmap(name1), from, GetProcessingBitmap(name2), to);
    }
    /// <summary>
    /// Copies the color channel value of '<paramref name="from"/>' in the main bitmap into color channel '<paramref name="to"/>' of <paramref name="name"/>.
    /// </summary>
    /// <param name="from">Must be a single channel, may not be <see cref="ColorChannel.RGB"/>.</param>
    /// <param name="name">Name of the bitmap to shift into.</param>
    /// <param name="to">The channel to copy into. May be <see cref="ColorChannel.RGB"/></param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public PixelProcessor Shift(ColorChannel from, string name, ColorChannel to)
    {
        return Shift(Bitmap, from, GetProcessingBitmap(name), to);
    }

    // CUSTOM

    /// <summary>
    /// A custom pixel delegate is used to perform user operations on a pixel color value.
    /// </summary>
    /// <param name="pixel1">The color value from the input bitmap.</param>
    /// <param name="pixel2">The color value from the output bitmap.</param>
    /// <returns>The color value to set the output bitmap.</returns>
    public delegate Color CustomPixelDelegate(Color pixel1, Color pixel2);

    private PixelProcessor Custom(BitmapType bitmap, CustomPixelDelegate func, string output = "")
    {
        BitmapType newBitmap = string.IsNullOrEmpty(output) ? bitmap : GetProcessingBitmap(output);
        Actions.Add((x, y) =>
        {
            Color pixel = func(bitmap.GetPixel(x, y), newBitmap.GetPixel(x, y));
            newBitmap.SetPixel(x, y, pixel);
        });
        return this;
    }
    /// <summary>
    /// Takes a custom pixel handling delegate to decide the end result of the pixel.
    /// <para/>
    /// <paramref name="func"/> has the following signature:
    /// <code>
    /// Color func(Color pixel1, Color pixel2)
    /// </code>
    /// Where:
    /// <para/>pixel1 = The color value from the main bitmap.
    /// <para/>pixel2 = The color value from <paramref name="output"/>, or the main bitmap if not specified.
    /// <para/>return = The color value to set <paramref name="output"/>, or directly to the main bitmap if not specified.
    /// </summary>
    /// <param name="func"></param>
    /// <param name="output">The named bitmap to set, or directly modifies the main bitmap if not specified.</param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    public PixelProcessor Custom(CustomPixelDelegate func, string output = "")
    {
        return Custom(Bitmap, func, output);
    }
    /// <summary>
    /// Takes a custom pixel handling delegate to decide the end result of the pixel.
    /// <para/>
    /// <paramref name="func"/> has the following signature:
    /// <code>
    /// Color func(Color pixel1, Color pixel2)
    /// </code>
    /// Where:
    /// <para/>pixel1 = The color value from the named bitmap <paramref name="name"/>.
    /// <para/>pixel2 = The color value from <paramref name="output"/>, or the main bitmap if not specified.
    /// <para/>return = The color value to set <paramref name="output"/>, or directly to the named bitmap if not specified.
    /// </summary>
    /// <param name="name">Name of the bitmap to operate on.</param>
    /// <param name="func"></param>
    /// <param name="output">The named bitmap to set, or directly modifies the named bitmap if not specified.</param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    public PixelProcessor Custom(string name, CustomPixelDelegate func, string output = "")
    {
        return Custom(GetProcessingBitmap(name), func, output);
    }

    #endregion Pixel operations

    #region Quick helper functions

    private PixelProcessor Grayscale(BitmapType bitmap, bool maintainTransparency = false, string output = "")
    {
        return Custom(bitmap, (p1, p2) =>
        {
            var b = (int)(p1.GetBrightness() * 255);
            return Color.FromArgb(maintainTransparency ? p1.A : 255, b, b, b);
        }, output);
    }
    /// <summary>
    /// Converts pixels to their grayscale value.
    /// <para/>
    /// Set alpha to 255 using <see cref="SetValue(string, ColorChannel, int, string)"/> afterwards for a complete grayscale image.
    /// </summary>
    /// <param name="maintainTransparency">If transparency should be kept or removed (set to 255).</param>
    /// <param name="output">The named bitmap to set, or directly modifies the main bitmap if not specified.</param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    public PixelProcessor Grayscale(bool maintainTransparency = false, string output = "")
    {
        return Grayscale(Bitmap, maintainTransparency, output);
    }
    /// <summary>
    /// Converts pixels to their grayscale value.
    /// <para/>
    /// Set alpha to 255 using <see cref="SetValue(string, ColorChannel, int, string)"/> afterwards for a complete grayscale image.
    /// </summary>
    /// <param name="name">Name of the bitmap to operate on.</param>
    /// <param name="maintainTransparency">If transparency should be kept or removed (set to 255).</param>
    /// <param name="output">The named bitmap to set, or directly modifies the named bitmap if not specified.</param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    public PixelProcessor Grayscale(string name, bool maintainTransparency = false, string output = "")
    {
        return Grayscale(GetProcessingBitmap(name), maintainTransparency, output);
    }

    private PixelProcessor ClearAlpha(BitmapType bitmap, int value, string output = "")
    {
        return Custom(bitmap, (p1, p2) =>
        {
            return p1.A > 0 ? p1 : Color.FromArgb(0, value, value, value);
        }, output);
    }
    /// <summary>
    /// Clears completely transparent pixels to a given <paramref name="value"/>.
    /// Modified pixels still maintain their 0 alpha.
    /// </summary>
    /// <param name="value">Value to assign to completely transparent pixels.</param>
    /// <param name="output">The named bitmap to set, or directly modifies the main bitmap if not specified.</param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    public PixelProcessor ClearAlpha(int value, string output = "")
    {
        return ClearAlpha(Bitmap, value, output);
    }
    /// <summary>
    /// Clears completely transparent pixels to transparent black.
    /// Modified pixels still maintain their 0 alpha.
    /// </summary>
    /// <param name="output">The named bitmap to set, or directly modifies the main bitmap if not specified.</param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    public PixelProcessor ClearAlpha(string output = "")
    {
        return ClearAlpha(0, output);
    }
    /// <summary>
    /// Clears completely transparent pixels to a given <paramref name="value"/>.
    /// Modified pixels still maintain their 0 alpha.
    /// </summary>
    /// <param name="name">Name of the bitmap to operate on.</param>
    /// <param name="value">Value to assign to completely transparent pixels.</param>
    /// <param name="output">The named bitmap to set, or directly modifies the named bitmap if not specified.</param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    public PixelProcessor ClearAlpha(string name, int value, string output = "")
    {
        return ClearAlpha(GetProcessingBitmap(name), value, output);
    }
    /// <summary>
    /// Clears completely transparent pixels to transparent black.
    /// Modified pixels still maintain their 0 alpha.
    /// </summary>
    /// <param name="name">Name of the bitmap to operate on.</param>
    /// <param name="output">The named bitmap to set, or directly modifies the named bitmap if not specified.</param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    public PixelProcessor ClearAlpha(string name, string output = "")
    {
        return ClearAlpha(GetProcessingBitmap(name), 0, output);
    }

    private PixelProcessor ClearAlpha(BitmapType bitmap, Color color, string output = "")
    {
        return Custom(bitmap, (p1, p2) =>
        {
            return p1.A > 0 ? p1 : color;
        }, output);
    }
    /// <summary>
    /// Clears completely transparent pixels to a given <paramref name="color"/>.
    /// Modified pixels will have their alpha changed to that of <paramref name="color"/>s alpha.
    /// </summary>
    /// <param name="color">Color to replace completely transparent pixels.</param>
    /// <param name="output">The named bitmap to set, or directly modifies the main bitmap if not specified.</param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    public PixelProcessor ClearAlpha(Color color, string output = "")
    {
        return ClearAlpha(Bitmap, color, output);
    }
    /// <summary>
    /// Clears completely transparent pixels to a given <paramref name="color"/>.
    /// Modified pixels will have their alpha changed to that of <paramref name="color"/>s alpha.
    /// </summary>
    /// <param name="name">Name of the bitmap to operate on.</param>
    /// <param name="color">Color to replace completely transparent pixels.</param>
    /// <param name="output">The named bitmap to set, or directly modifies the named bitmap if not specified.</param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    public PixelProcessor ClearAlpha(string name, Color color, string output = "")
    {
        return ClearAlpha(GetProcessingBitmap(name), color, output);
    }

    #endregion Quick helper functions

    /// <summary>
    /// Saves a named bitmap to a file. Should only be invoked after <see cref="Process"/>!
    /// </summary>
    /// <param name="name">Name of the bitmap to save.</param>
    /// <param name="path">Path to save as.</param>
    /// <param name="imageFormat"><see cref="ImageFormat"/> to save as.</param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    public PixelProcessor Save(string name, string path, ImageFormat? imageFormat = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using FileStream stream = File.Create(path);
        GetProcessingBitmap(name).Save(stream, imageFormat ?? ImageFormat.Png);
        return this;
    }

    /// <summary>
    /// Saves the main bitmap to a file.
    /// </summary>
    /// <param name="path">Path to save as.</param>
    /// <param name="imageFormat"><see cref="ImageFormat"/> to save as.</param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    public PixelProcessor Save(string path, ImageFormat? imageFormat = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using FileStream stream = File.Create(path);
        Bitmap.Save(stream, imageFormat ?? ImageFormat.Png);
        return this;
    }

    /// <summary>
    /// Does the processing set up by previous methods.
    /// No bitmap manipulation actually occurs until this or <see cref="ProcessSave(string, bool, ImageFormat?)"/> is called.
    /// </summary>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    public PixelProcessor Process()
    {
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                foreach (Action<int, int> action in Actions)
                    action(x, y);
            }
        }
        return this;
    }

    /// <summary>
    /// Does the processing set up by previous methods and then saves all named bitmaps.
    /// No bitmap manipulation actually occurs until this or <see cref="Process"/> is called.
    /// </summary>
    /// <param name="template">The template format for the filename. Must have at least one place holder for the name of the bitmap, e.g. "./my_image_called_{0}.png"</param>
    /// <param name="automaticExtension">If the extension should be inferred from <paramref name="imageFormat"/>.</param>
    /// <param name="imageFormat">Image format to save as. Default is <see cref="ImageFormat.Png"/>.</param>
    /// <returns>This <see cref="PixelProcessor"/> to continue the chain.</returns>
    public PixelProcessor ProcessSave(string template, bool automaticExtension = false, ImageFormat? imageFormat = null)
    {
        Process();
        imageFormat ??= ImageFormat.Png;
        if (automaticExtension)
        {
            template = Path.ChangeExtension(template, ImageFormats[imageFormat]);
        }
        foreach (var name in ProcessingBitmaps.Keys)
        {
            Save(name, string.Format(template, name), imageFormat);
        }
        return this;
    }

    /// <summary>
    /// Disposes all bitmaps created by this instance.
    /// </summary>
    public void Dispose()
    {
        foreach (BitmapType bitmap in ProcessingBitmaps.Values)
        {
            bitmap.Dispose();
        }
        Bitmap.Dispose();
    }

    /// <summary>
    /// Ends processing and disposes of bitmaps.
    /// Call this after all processing and saving has finished.
    /// </summary>
    public void Finish()
    {
        Dispose();
    }

    /// <summary>
    /// Image formats associated with extensions.
    /// </summary>
    private readonly Dictionary<ImageFormat, string> ImageFormats = new()
    {
        [ImageFormat.Bmp] = ".bmp",
        [ImageFormat.Emf] = ".emf",
        [ImageFormat.Exif] = ".exif",
        [ImageFormat.Gif] = ".gif",
        [ImageFormat.Icon] = ".ico",
        [ImageFormat.Jpeg] = ".jpg",
        [ImageFormat.Png] = ".png",
        [ImageFormat.Tiff] = ".tiff",
        [ImageFormat.Wmf] = ".wmf",
    };

    /// <summary>
    /// Starts a new <see cref="PixelProcessor"/> chain by creating and returning it.
    /// </summary>
    /// <param name="bitmap">The bitmap to process.</param>
    /// <returns>The new <see cref="PixelProcessor"/>.</returns>
    public static PixelProcessor New(Bitmap bitmap)
    {
        return new PixelProcessor(bitmap);
    }
    ///<inheritdoc cref="New(System.Drawing.Bitmap)"/>
    public static PixelProcessor New(DirectBitmap bitmap)
    {
        return new PixelProcessor(bitmap);
    }

    /// <summary>
    /// Converts a channel name to a <see cref="ColorChannel"/> enum.
    /// </summary>
    /// <param name="channel"></param>
    /// <returns>The color channel or <see langword="null"/>.</returns>
    public static ColorChannel? GetColorChannelFromString(string channel)
    {
        return channel.ToLower() switch
        {
            "r" => ColorChannel.R,
            "g" => ColorChannel.G,
            "b" => ColorChannel.B,
            "a" => ColorChannel.A,
            "rgb" => ColorChannel.RGB,
            _ => null,
        };
    }
}
