using System.Drawing;
using System.Runtime.Versioning;

//using BitmapType = System.Drawing.Bitmap;
using BitmapType = ImagePixelProcessor.DirectBitmap;

namespace ImagePixelProcessor;

/// <summary>
/// Analyzes an image pixel-by-pixel to get color information.
/// </summary>
[SupportedOSPlatform("windows")]
public static class PixelAnalyzer
{
    private static readonly List<Func<int, int, bool>> Actions = new();
    private static readonly Dictionary<string, dynamic> Values = new();
    private static void Process(BitmapType bitmap, PixelAnalyzerOptions? options = null)
    {
        options ??= PixelAnalyzerOptions.Default;
        for (var x = 0; x < bitmap.Width; x += 1 + options.PixelSkip)
        {
            for (var y = 0; y < bitmap.Height; y++)
            {
                foreach (Func<int, int, bool>? action in Actions)
                    // Can early exit.
                    if (!action(x, y))
                        return;
            }
        }
    }
    private static void Clear()
    {
        Actions.Clear();
        Values.Clear();
    }

    private static void GetAverageChannel(BitmapType bitmap, ColorChannel channel)
    {
        var values = new List<int>();
        Values.Add(channel.ToString(), values);
        Actions.Add((x, y) =>
        {
            Color pixel = bitmap.GetPixel(x, y);
            values.Add(channel switch
            {
                ColorChannel.A => pixel.A,
                ColorChannel.R => pixel.R,
                ColorChannel.G => pixel.G,
                ColorChannel.B => pixel.B,
                _ => throw new ArgumentException($"Channel {channel} cannot be used for averaging.")
            });
            return true;
        });
    }
    /// <summary>
    /// Gets the average value of a specific <see cref="ColorChannel"/>.
    /// </summary>
    /// <param name="bitmap">The image to analyze.</param>
    /// <param name="channel">The channel to analyze. Must be a single channel.</param>
    /// <param name="options">Analyzing options to use.</param>
    /// <returns></returns>
    public static int GetAverageChannel(BitmapType bitmap, ColorChannel channel, PixelAnalyzerOptions? options = null)
    {
        GetAverageChannel(bitmap, channel);
        Process(bitmap, options);
        var ret = (int)((List<int>)Values[channel.ToString()]).Average();
        Clear();
        return ret;
    }
    /// <summary>
    /// Gets the average of all colors in the image, including alpha.
    /// </summary>
    /// <param name="bitmap">The image to analyze.</param>
    /// <param name="options">Analyzing options to use.</param>
    /// <returns>The average of all colors in the image, including alpha.</returns>
    public static Color GetAverageColor(BitmapType bitmap, PixelAnalyzerOptions? options = null)
    {
        GetAverageChannel(bitmap, ColorChannel.A);
        GetAverageChannel(bitmap, ColorChannel.R);
        GetAverageChannel(bitmap, ColorChannel.G);
        GetAverageChannel(bitmap, ColorChannel.B);
        Process(bitmap, options);
        var a = (int)((List<int>)Values[nameof(ColorChannel.A)]).Average();
        var r = (int)((List<int>)Values[nameof(ColorChannel.R)]).Average();
        var g = (int)((List<int>)Values[nameof(ColorChannel.G)]).Average();
        var b = (int)((List<int>)Values[nameof(ColorChannel.B)]).Average();
        Clear();
        return Color.FromArgb(a, r, g, b);
        //var AList = new List<int>();
        //var RList = new List<int>();
        //var GList = new List<int>();
        //var BList = new List<int>();
        //for (var x = 0; x < bitmap.Width; x++)
        //{
        //    for (var y = 0; y < bitmap.Height; y++)
        //    {
        //        Color pixel = bitmap.GetPixel(x, y);
        //        AList.Add(pixel.A);
        //        RList.Add(pixel.R);
        //        GList.Add(pixel.G);
        //        BList.Add(pixel.B);
        //    }
        //}
        //return Color.FromArgb((int)AList.Average(), (int)RList.Average(), (int)GList.Average(), (int)BList.Average());
    }

    /// <summary>
    /// Gets the color that appears the most throughout the image.
    /// </summary>
    /// <param name="bitmap">The image to analyze.</param>
    /// <param name="options">Analyzing options to use.</param>
    /// <returns>The color that appears the most throughout the image.</returns>
    public static Color GetCommonColor(BitmapType bitmap, PixelAnalyzerOptions? options = null)
    {
        // Setup
        var colorCount = new Dictionary<Color, int>();
        Values.Add("colorCount", colorCount);
        Actions.Add((x, y) =>
        {
            Color pixel = bitmap.GetPixel(x, y);
            if (!colorCount.ContainsKey(pixel))
                colorCount[pixel] = 1;
            else
                colorCount[pixel]++;
            return true;
        });
        //Console.WriteLine(colorCount.Count);
        //return colorCount.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
        //return Color.FromArgb((int)AList.Average(), (int)RList.Average(), (int)GList.Average(), (int)BList.Average());
        //Process
        Process(bitmap, options);
        Color ret = ((Dictionary<Color, int>)Values["colorCount"]).Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
        Clear();
        return ret;
    }

    /// <summary>
    /// Gets if the image is a grayscale image.
    /// </summary>
    /// <param name="bitmap">The image to analyze.</param>
    /// <param name="testAlpha">If <see langword="true"/> then any pixel with an alpha below 255 will be considered not grayscale.</param>
    /// <param name="options">Analyzing options to use.</param>
    /// <returns>If the image is grayscale.</returns>
    public static bool IsGrayscale(BitmapType bitmap, bool testAlpha = false, PixelAnalyzerOptions? options = null)
    {
        // Setup
        Values.Add("IsGrayscale", true);
        Actions.Add((x, y) =>
        {
            Color pixel = bitmap.GetPixel(x, y);
            if (pixel.A > 0 && ((testAlpha && pixel.A < 255) || pixel.R != pixel.G || pixel.G != pixel.B))
            {
                Values["IsGrayscale"] = false;
                return false;
            }
            return true;
        });
        // Process
        Process(bitmap, options);
        var ret = (bool)Values["IsGrayscale"];
        Clear();
        return ret;
    }
}
