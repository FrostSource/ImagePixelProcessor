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
        //Console.WriteLine(new FileInfo(Path.Join(testPath, "alpha_image.png")).FullName);

        var bitmap = new Bitmap(Path.Join(testPath, "alpha_image.png"));
        //var bitmap = new Bitmap(Path.Join(testPath, "alpha_image_1024.png"));
        bitmap.Save(Path.Join(testPath, "mainoriginal.png"));
        var direct = new DirectBitmap(bitmap);
        direct.InternalBitmap.Save(Path.Join(testPath, "mainoriginal.png"));
        PixelProcessor.New(bitmap)
            .Shift(ColorChannel.A, "mask", ColorChannel.RGB)
            .SetValue("mask", ColorChannel.A, 255)
            //.Shift(ColorChannel.R, "col", ColorChannel.R)
            //.Shift(ColorChannel.G, "col", ColorChannel.G)
            //.Shift(ColorChannel.B, "col", ColorChannel.B)
            .ProcessSave(Path.Join(testPath, "{0}"), true, ImageFormat.Png)
            .Save(Path.Join(testPath, "main.png"))
            //.Process()
            //.Save("mask", Path.Join(testPath, "test.png"))
            .Finish();

        //// Get grayscale alpha mask
        //PixelProcessor.New(bitmap)
        //    .Shift(ColorChannel.A, "mask", ColorChannel.RGB)
        //    .ProcessSave(@".\alpha_{0}", true, ImageFormat.Png)
        //    .GetBitmap("mask", out Bitmap bmap)
        //    .Finish();
        //bmap.Dispose();
    }
}
