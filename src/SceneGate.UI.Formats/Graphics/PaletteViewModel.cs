namespace SceneGate.UI.Formats.Graphics;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Texim.Colors;
using Texim.Palettes;

public partial class PaletteViewModel : ObservableObject, IFormatViewModel
{
    [ObservableProperty]
    private ObservableCollection<PaletteRepresentation> palettes;

    [ObservableProperty]
    private PaletteRepresentation? selectedPalette;

    [ObservableProperty]
    private Color selectedColor;

    public PaletteViewModel()
    {
        byte[] random1 = new byte[16 * 2 * 16];
        byte[] random2 = new byte[256];
        var random = new Random(42);
        random.NextBytes(random1);
        random.NextBytes(random2);

        IPalette[] testPalettes = [
            new Palette(random1.DecodeColorsAs<Bgr555>()),
            new Palette(random2.DecodeColorsAs<Bgr555>()),
        ];

        Palettes = new(testPalettes.Select((p, idx) => new PaletteRepresentation(idx, p)));
        SelectedPalette = Palettes.FirstOrDefault();
        SelectedColor = SelectedPalette?.Colors.FirstOrDefault() ?? default;
    }

    public PaletteViewModel(IPalette palette)
    {
        Palettes = [new PaletteRepresentation(0, palette)];
        SelectedPalette = Palettes[0];
        SelectedColor = SelectedPalette.Colors.FirstOrDefault();
    }

    public PaletteViewModel(IPaletteCollection palettes)
    {
        Palettes = new(palettes.Palettes.Select((p, idx) => new PaletteRepresentation(idx, p)));
        SelectedPalette = Palettes.FirstOrDefault();
        SelectedColor = SelectedPalette?.Colors.FirstOrDefault() ?? default;
    }
}
