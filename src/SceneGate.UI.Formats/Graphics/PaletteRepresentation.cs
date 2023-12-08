namespace SceneGate.UI.Formats.Graphics;

using System.Linq;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Texim.Formats;
using Texim.Palettes;
using Yarhl.IO;

/// <summary>
/// Representation of a Yarhl palette in different formats.
/// </summary>
public record PaletteRepresentation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaletteRepresentation"/> class.
    /// </summary>
    /// <param name="index">The palette index.</param>
    /// <param name="palette">The palette.</param>
    public PaletteRepresentation(int index, IPalette palette)
    {
        Index = index;
        Palette = palette;

        AvaloniaPalette = new AvaloniaPalette(palette);
        Colors = palette.Colors.Select(c => c.ToAvaloniaColor()).ToArray();

        using BinaryFormat binaryPng = new Palette2Bitmap().Convert(Palette);
        binaryPng.Stream.Position = 0;
        Image = new Bitmap(binaryPng.Stream);
    }

    public int Index { get; }

    public IPalette Palette { get; }

    public Bitmap Image { get; }

    public AvaloniaPalette AvaloniaPalette { get; }

    public Color[] Colors { get; }
}
