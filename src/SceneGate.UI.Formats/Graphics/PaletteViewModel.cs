namespace SceneGate.UI.Formats.Graphics;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Texim.Colors;
using Texim.Formats;
using Texim.Palettes;
using Yarhl.IO;

public partial class PaletteViewModel : ObservableObject, IFormatViewModel
{
    private readonly IPalette[] palettes;

    [ObservableProperty]
    private ObservableCollection<Bitmap> paletteImages;

    [ObservableProperty]
    private IColorPalette currentAvaloniaPalette;

    [ObservableProperty]
    private Color[] currentPaletteColors;

    [ObservableProperty]
    private Bitmap? currentImage;

    [ObservableProperty]
    private Color selectedColor;

    public PaletteViewModel()
    {
        byte[] random1 = new byte[16 * 2 * 16];
        byte[] random2 = new byte[256];
        var random = new Random(42);
        random.NextBytes(random1);
        random.NextBytes(random2);

        palettes = [
            new Palette(random1.DecodeColorsAs<Bgr555>()),
            new Palette(random2.DecodeColorsAs<Bgr555>()),
            new Palette(random2.DecodeColorsAs<Bgr555>()),
            new Palette(random2.DecodeColorsAs<Bgr555>()),
            new Palette(random2.DecodeColorsAs<Bgr555>()),
            new Palette(random2.DecodeColorsAs<Bgr555>()),
            new Palette(random2.DecodeColorsAs<Bgr555>()),
            new Palette(random2.DecodeColorsAs<Bgr555>()),
            new Palette(random2.DecodeColorsAs<Bgr555>()),
            new Palette(random2.DecodeColorsAs<Bgr555>()),
        ];

        paletteImages = [];
        GenerateImages();
    }

    public PaletteViewModel(IPalette palette)
    {
        palettes = [palette];
        paletteImages = [];
        GenerateImages();
    }

    public PaletteViewModel(IPaletteCollection palettes)
    {
        this.palettes = [..palettes.Palettes];
        paletteImages = [];
        GenerateImages();
    }

    public void GenerateImages()
    {
        foreach (Bitmap existingImage in PaletteImages) {
            existingImage.Dispose();
        }

        PaletteImages.Clear();

        var palette2Image = new Palette2Bitmap();
        foreach (IPalette palette in palettes) {
            using BinaryFormat pngImage = palette2Image.Convert(palette);

            pngImage.Stream.Position = 0;
            PaletteImages.Add(new Bitmap(pngImage.Stream));
        }

        CurrentImage = PaletteImages.FirstOrDefault();
        CurrentAvaloniaPalette = new AvaloniaPalette(palettes[0]);
        CurrentPaletteColors = palettes[0].Colors.Select(c => c.ToAvaloniaColor()).ToArray();
    }
}
