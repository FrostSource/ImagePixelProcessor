using ImagePixelProcessor;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace ConsoleTests;

[SupportedOSPlatform("windows")]
internal class Program
{
    static void Main(string[] args)
    {
        string testPath = @"..\..\..\TestImages";
        string testImage = Path.Join(testPath, "alpha_image.png");

        var bitmap = new Bitmap(testImage);
        using var _ = PixelProcessor.New(bitmap)
            // Create alpha mask
            .Shift(ColorChannel.A, "mask", ColorChannel.RGB)
            .SetValue("mask", ColorChannel.A, 255)
            // Remove alpha
            .Copy(ColorChannel.RGB, "no_alpha")
            .SetValue("no_alpha", ColorChannel.A, 255)
            // Grayscale
            .Grayscale("grayscale")
            // Clear alpha to value/color
            .ClearAlpha(255, "alpha_white")
            .SetValue("alpha_white", ColorChannel.A, 255)
            .ClearAlpha(128, "alpha_gray")
            .SetValue("alpha_gray", ColorChannel.A, 255)
            .ClearAlpha(Color.Red, "alpha_red")
            .ProcessSave(Path.Join(testPath, Path.GetFileNameWithoutExtension(testImage) + "_{0}"), true, ImageFormat.Png)
            ;

        //// Get grayscale alpha mask
        //PixelProcessor.New(bitmap)
        //    .Shift(ColorChannel.A, "mask", ColorChannel.RGB)
        //    .ProcessSave(@".\alpha_{0}", true, ImageFormat.Png)
        //    .GetBitmap("mask", out Bitmap bmap)
        //    .Finish();
        //bmap.Dispose();
    }
}
