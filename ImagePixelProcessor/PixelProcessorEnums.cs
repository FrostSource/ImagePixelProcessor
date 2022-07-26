
namespace ImagePixelProcessor;

/// <summary>
/// Named color channels.
/// Can be combined, e.g. (ColorChannel.A | ColorChannel.B) for alpha and blue channels.
/// <para/>Function parameter will mention if combining is not allowed.
/// </summary>
[Flags]
public enum ColorChannel
{
    /// <summary>
    /// Alpha channel.
    /// </summary>
    A = 1,
    /// <summary>
    /// Red channel.
    /// </summary>
    R = 2,
    /// <summary>
    /// Green channel.
    /// </summary>
    G = 4,
    /// <summary>
    /// Blue channel.
    /// </summary>
    B = 8,
    /// <summary>
    /// Red, green and blue channels for simplicity.
    /// </summary>
    RGB = R | G | B,
    /// <summary>
    /// Alpha, red, green and blue channels for simplicity.
    /// </summary>
    ARGB = A | RGB,
}
