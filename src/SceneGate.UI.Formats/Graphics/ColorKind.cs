namespace SceneGate.UI.Formats.Graphics;

using System;
using Texim.Colors;

/// <summary>
/// Color encoding formats.
/// </summary>
public enum ColorKind
{
    /// <summary>
    /// BGR 5-bits encoding.
    /// </summary>
    BGR555,

    /// <summary>
    /// RGB 8-bits encoding.
    /// </summary>
    RGB32,

    /// <summary>
    /// RGB 8-bits with alpha encoding.
    /// </summary>
    RGBA32,
}

/// <summary>
/// Extension methos for the color kind enum.
/// </summary>
public static class ColorKindExtensions
{
    /// <summary>
    /// Gets the encoding instance for the color kind.
    /// </summary>
    /// <param name="kind">The color kind.</param>
    /// <returns>Its color encoding instance.</returns>
    /// <exception cref="NotSupportedException">Unsupported format.</exception>
    public static IColorEncoding GetEncoding(this ColorKind kind) =>
        kind switch {
            ColorKind.BGR555 => Bgr555.Instance,
            ColorKind.RGB32 => Rgb32.Instance,
            ColorKind.RGBA32 => Rgba32.Instance,
            _ => throw new NotSupportedException("Unsupported color kind"),
        };
}
