namespace SceneGate.UI.Formats.Graphics;

using System;
using System.Linq;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Texim.Formats;
using Texim.Palettes;
using Yarhl.IO;

/// <summary>
/// Representation of a Yarhl palette in different formats.
/// </summary>
public record PaletteRepresentation : IDisposable
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

        AvaloniaPalette = new AvaloniaSimplePalette(palette);
        Colors = palette.Colors.Select(c => c.ToAvaloniaColor()).ToArray();

        if (palette.Colors.Count > 0) {
            try {
                using BinaryFormat binaryPng = new Palette2Bitmap().Convert(palette);
                binaryPng.Stream.Position = 0;
                Image = new Bitmap(binaryPng.Stream);
                IsError = false;
            } catch {
                IsError = true;
            }
        } else {
            IsError = true;
        }
    }

    /// <summary>
    /// Gets the index of the palette in the collection.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets the source Yarhl palette.
    /// </summary>
    public IPalette Palette { get; }

    /// <summary>
    /// Gets a value indicating whether the palette is valid or it causes errors to display.
    /// </summary>
    /// <remarks>
    /// If the palette has no color it will be set to true.
    /// </remarks>
    public bool IsError { get; }

    /// <summary>
    /// Gets an image displaying all the palette colors.
    /// </summary>
    public Bitmap? Image { get; }

    /// <summary>
    /// Gets the palette in Avalonia format.
    /// </summary>
    public AvaloniaSimplePalette AvaloniaPalette { get; }

    /// <summary>
    /// Gets the palette colors in Avalonia format.
    /// </summary>
    public Color[] Colors { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose the resources of the instance.
    /// </summary>
    /// <param name="disposing">Value indicating whether it's disposing or finalizing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing) {
            Image?.Dispose();
        }
    }
}
