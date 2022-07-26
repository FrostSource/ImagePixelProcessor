using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.Versioning;

namespace ImagePixelProcessor;

/// <summary>
/// Adds extensions for the <see cref="Bitmap"/> class.
/// </summary>
[SupportedOSPlatform("windows")]
public static class BitmapExtensions
{
    /// <summary>
    /// Converts a <see cref="Bitmap"/> object to a <see cref="DirectBitmap"/> object.
    /// <para/>
    /// This can also be done with implict/explicit casting.
    /// </summary>
    /// <param name="bitmap">The <see cref="Bitmap"/> to convert.</param>
    /// <returns>The new <see cref="DirectBitmap"/> object.</returns>
    public static DirectBitmap ToDirectBitmap(this Bitmap bitmap)
    {
        return new DirectBitmap(bitmap);
    }
}
