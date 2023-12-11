namespace SceneGate.UI.Formats.Graphics;

using System;
using Texim.Colors;
using Texim.Palettes;

using Yarhl.IO;

/// <summary>
/// Design view model for ImageView.
/// </summary>
public class DesignImageViewModel : ImageViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DesignImageViewModel"/> class.
    /// </summary>
    public DesignImageViewModel()
        : base(CreateRandomBinaryImage(), CreateRandomPalettes())
    {
    }

    private static IBinary CreateRandomBinaryImage()
    {
        byte[] bytes = new byte[256 * 192 * 4];
        var random = new Random(42);
        random.NextBytes(bytes);

        return new BinaryFormat(DataStreamFactory.FromArray(bytes));
    }

    private static IPaletteCollection CreateRandomPalettes()
    {
        byte[] bytes = new byte[256 * 2 * 4];
        var random = new Random(42);
        random.NextBytes(bytes);

        Rgb[] colors = bytes.DecodeColorsAs<Bgr555>();
        IPalette[] palettes = [
            new Palette(colors[..256]),
            new Palette(colors[256..512]),
            new Palette(colors[512..768]),
            new Palette(colors[768..]),
        ];
        return new PaletteCollection(palettes);
    }
}
