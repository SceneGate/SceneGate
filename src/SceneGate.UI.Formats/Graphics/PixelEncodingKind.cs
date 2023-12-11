namespace SceneGate.UI.Formats.Graphics;

using System;
using Texim.Pixels;

/// <summary>
/// Kind of pixel encoding.
/// </summary>
public enum PixelEncodingKind
{
    /// <summary>
    /// Indexed 4 bpp in little endian.
    /// </summary>
    Indexed4BppLittleEndian,

    /// <summary>
    /// Indexed 4 bpp in big endian.
    /// </summary>
    Indexed4BppBigEndian,

    /// <summary>
    /// Indexed 8 bpp.
    /// </summary>
    Indexed8Bpp,

    /// <summary>
    /// Indexed 8bpp: alpha 3-bits, index 5-bits
    /// </summary>
    IndexedA3I5,

    /// <summary>
    /// Indexed 8bpp: alpha 5-bits, index 3-bits.
    /// </summary>
    IndexedA5I3,
}

/// <summary>
/// Extension methods for the <see cref="PixelEncodingKind"/> enum.
/// </summary>
public static class PixelEncodingExtensions
{
    /// <summary>
    /// Get the pixel encoding of the indexed kinds.
    /// </summary>
    /// <param name="kind">Pixel encoding kind.</param>
    /// <returns>The encoding implementation.</returns>
    /// <exception cref="NotSupportedException">Unsupported format.</exception>
    public static IIndexedPixelEncoding GetIndexedEncoding(this PixelEncodingKind kind) =>
        kind switch {
            PixelEncodingKind.Indexed4BppLittleEndian => Indexed4Bpp.Instance,
            PixelEncodingKind.Indexed4BppBigEndian => Indexed4BppBigEndian.Instance,
            PixelEncodingKind.Indexed8Bpp => Indexed8Bpp.Instance,
            PixelEncodingKind.IndexedA3I5 => IndexedA3I5.Instance,
            PixelEncodingKind.IndexedA5I3 => IndexedA5I3.Instance,
            _ => throw new NotSupportedException("Unsupported format"),
        };

    /// <summary>
    /// Gets the bits per pixel for the pixel encoding.
    /// </summary>
    /// <param name="kind">Pixel encoding kind.</param>
    /// <returns>The bits per pixel for the encoding.</returns>
    /// <exception cref="NotSupportedException">Unsupported format.</exception>
    public static int GetBitsPerPixel(this PixelEncodingKind kind) =>
        // TODO: move to texim
        kind switch {
            PixelEncodingKind.Indexed4BppLittleEndian => Indexed4Bpp.Instance.BitsPerPixel,
            PixelEncodingKind.Indexed4BppBigEndian => Indexed4BppBigEndian.Instance.BitsPerPixel,
            PixelEncodingKind.Indexed8Bpp => Indexed8Bpp.Instance.BitsPerPixel,
            PixelEncodingKind.IndexedA3I5 => IndexedA3I5.Instance.BitsPerPixel,
            PixelEncodingKind.IndexedA5I3 => IndexedA5I3.Instance.BitsPerPixel,
            _ => throw new NotSupportedException("Unsupported format"),
        };
}
