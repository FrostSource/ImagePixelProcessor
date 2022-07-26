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
        string testImage = Path.Join(testPath, "analyze.png");

        var bitmap = new Bitmap(testImage);
        var avg = PixelAnalyzer.GetAverageColor(bitmap);
        var common = PixelAnalyzer.GetCommonColor(bitmap);
        using var pp = PixelProcessor.New(bitmap)
            // Create alpha mask
            .Shift(ColorChannel.A, "mask", ColorChannel.RGB)
            .SetValue("mask", ColorChannel.A, 255)
            // Remove alpha
            .Copy(ColorChannel.RGB, "no_alpha")
            .SetValue("no_alpha", ColorChannel.A, 255)
            // Grayscale
            .Grayscale(output: "grayscale")
            // Clear alpha to value/color
            .ClearAlpha(255, "alpha_white")
            .SetValue("alpha_white", ColorChannel.A, 255)
            .ClearAlpha(128, "alpha_gray")
            .SetValue("alpha_gray", ColorChannel.A, 255)
            .ClearAlpha(Color.Red, "alpha_red")
            // Test average
            .Set(avg, "average")
            .Set(common, "common")
            // ColorChannel flags
            .Extract(ColorChannel.R | ColorChannel.G | ColorChannel.A, "rga")
            .Extract(ColorChannel.B | ColorChannel.A, "ba")
            .ProcessSave(Path.Join(testPath, Path.GetFileNameWithoutExtension(testImage) + "_{0}"), true, ImageFormat.Png)
            ;
        Console.WriteLine(PixelAnalyzer.IsGrayscale(pp.GetProcessingBitmap("grayscale")));
        Console.WriteLine(PixelAnalyzer.IsGrayscale(pp.GetProcessingBitmap("grayscale"), true));
        Console.WriteLine(PixelAnalyzer.IsGrayscale(pp.GetProcessingBitmap("grayscale"), true, PixelAnalyzerOptions.HalfRes));
        Console.WriteLine(PixelAnalyzer.IsGrayscale(pp.GetProcessingBitmap("grayscale"), true, PixelAnalyzerOptions.QuarterRes));

        Console.Write("Press any key to delete created images... ");
        Console.ReadKey();
        foreach (var name in pp.Names)
        {
            var f = Path.Join(testPath, Path.GetFileNameWithoutExtension(testImage) + "_" + name + ".png");
            if (File.Exists(f))
                File.Delete(f);
        }

        

        //// Get grayscale alpha mask
        //PixelProcessor.New(bitmap)
        //    .Shift(ColorChannel.A, "mask", ColorChannel.RGB)
        //    .ProcessSave(@".\alpha_{0}", true, ImageFormat.Png)
        //    .GetBitmap("mask", out Bitmap bmap)
        //    .Finish();
        //bmap.Dispose();
    }
}
