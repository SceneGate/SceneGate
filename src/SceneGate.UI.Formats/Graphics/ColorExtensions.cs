namespace SceneGate.UI.Formats.Graphics;

using Avalonia.Media;
using Texim.Colors;

public static class ColorExtensions
{
    public static Color ToAvaloniaColor(this Rgb rgb)
    {
        return Color.FromArgb(rgb.Alpha, rgb.Red, rgb.Green, rgb.Blue);
    }
}
