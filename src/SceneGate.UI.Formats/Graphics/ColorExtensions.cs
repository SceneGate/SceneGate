namespace SceneGate.UI.Formats.Graphics;

using Avalonia.Media;
using Texim.Colors;

/// <summary>
/// Extension methods for Avalonia colors.
/// </summary>
public static class ColorExtensions
{
    /// <summary>
    /// Converts the Yarhl RGB color to Avalonia format.
    /// </summary>
    /// <param name="rgb">The RGB color.</param>
    /// <returns>The Avalonia RGB color.</returns>
    public static Color ToAvaloniaColor(this Rgb rgb)
    {
        return Color.FromArgb(rgb.Alpha, rgb.Red, rgb.Green, rgb.Blue);
    }
}
