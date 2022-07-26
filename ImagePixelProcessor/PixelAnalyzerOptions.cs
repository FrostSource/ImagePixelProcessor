using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePixelProcessor;

/// <summary>
/// Options for <see cref="PixelAnalyzer"/>.
/// </summary>
public sealed class PixelAnalyzerOptions
{
    /// <summary>
    /// Gets or sets the number of pixels to skip after a pixel is analyzed.
    /// A value of 1 would give an effective analyzing resolution of half.
    /// <para/>Must be >= 0.
    /// </summary>
    public int PixelSkip { get => _PixelSkip; set => _PixelSkip = Math.Max(0, value); }
    private int _PixelSkip = 0;

    /// <summary>
    /// Gets the default options object. Same as creating a new <see cref="PixelAnalyzerOptions"/>.
    /// </summary>
    public static PixelAnalyzerOptions Default => new();
    /// <summary>
    /// Analyze at half resolution.
    /// </summary>
    public static PixelAnalyzerOptions HalfRes => new() { PixelSkip = 1 };
    /// <summary>
    /// Analyze at quarter resolution.
    /// </summary>
    public static PixelAnalyzerOptions QuarterRes => new() { PixelSkip = 3 };
}
