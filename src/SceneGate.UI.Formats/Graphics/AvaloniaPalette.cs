namespace SceneGate.UI.Formats.Graphics;

using System;
using Avalonia.Controls;
using Avalonia.Media;
using Texim.Palettes;

public class AvaloniaPalette : IColorPalette
{
    private readonly IPalette palette;

    public AvaloniaPalette(IPalette palette)
    {
        ArgumentNullException.ThrowIfNull(nameof(palette));
        this.palette = palette;
    }

    public int ColorCount => palette.Colors.Count / ShadeCount;

    public int ShadeCount => palette.Colors.Count > 16 ? 16 : 4;

    public Color GetColor(int colorIndex, int shadeIndex)
    {
        return palette.Colors[(colorIndex * ShadeCount) + shadeIndex].ToAvaloniaColor();
    }
}
