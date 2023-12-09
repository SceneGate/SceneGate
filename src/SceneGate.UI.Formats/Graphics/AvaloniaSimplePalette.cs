namespace SceneGate.UI.Formats.Graphics;

using System;
using Avalonia.Controls;
using Avalonia.Media;
using Texim.Palettes;

/// <summary>
/// Represents a simple palette without shades to display in the Avalonia
/// ColorView and ColorPicket components.
/// </summary>
/// <remarks>
/// It tricks the number of shades to display color as columns of 16.
/// </remarks>
public class AvaloniaSimplePalette : IColorPalette
{
    private readonly IPalette palette;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaSimplePalette"/> class.
    /// </summary>
    /// <param name="palette">The source Yarhl palette.</param>
    public AvaloniaSimplePalette(IPalette palette)
    {
        ArgumentNullException.ThrowIfNull(nameof(palette));
        this.palette = palette;
    }

    /// <summary>
    /// Gets the number of color columns.
    /// </summary>
    public int ColorCount => palette.Colors.Count > 16 ? 16 : 8;

    /// <summary>
    /// Gets the number of color rows.
    /// </summary>
    public int ShadeCount => (int)Math.Ceiling((double)palette.Colors.Count / ColorCount);

    /// <summary>
    /// Gets a color by its row and column coordinates.
    /// </summary>
    /// <param name="colorIndex">The column number.</param>
    /// <param name="shadeIndex">The row number.</param>
    /// <returns>The color of the palette.</returns>
    public Color GetColor(int colorIndex, int shadeIndex)
    {
        int index = (shadeIndex * ColorCount) + colorIndex;
        if (index >= palette.Colors.Count) {
            return default;
        }

        return palette.Colors[index].ToAvaloniaColor();
    }
}
