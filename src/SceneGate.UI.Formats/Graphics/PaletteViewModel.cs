namespace SceneGate.UI.Formats.Graphics;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Texim.Colors;
using Texim.Palettes;

/// <summary>
/// View model to display palettes.
/// </summary>
public partial class PaletteViewModel : ObservableObject, IFormatViewModel
{
    [ObservableProperty]
    private ObservableCollection<PaletteRepresentation> palettes;

    [ObservableProperty]
    private PaletteRepresentation? selectedPalette;

    [ObservableProperty]
    private Color selectedColor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaletteViewModel"/> class.
    /// </summary>
    /// <remarks>
    /// It uses dummy / test data.
    /// </remarks>
    public PaletteViewModel()
    {
        var random = new Random(42);
        byte[] colorBytes = new byte[256 * 2];
        random.NextBytes(colorBytes);
        Rgb[] colors = colorBytes.DecodeColorsAs<Bgr555>();

        IPalette[] testPalettes = [
            new Palette(colors),
            new Palette(colors[..128]),
            new Palette(colors[..16]),
            new Palette(colors[16..30]),
            new Palette(colors[0..0]),
            new Palette(colors[..20]),
        ];

        Palettes = new(testPalettes.Select((p, idx) => new PaletteRepresentation(idx, p)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaletteViewModel"/> class.
    /// </summary>
    /// <param name="palette">The palette to represent.</param>
    public PaletteViewModel(IPalette palette)
    {
        Palettes = [new PaletteRepresentation(0, palette)];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaletteViewModel"/> class.
    /// </summary>
    /// <param name="palettes">The collection of palettes to represent.</param>
    public PaletteViewModel(IPaletteCollection palettes)
    {
        Palettes = new(palettes.Palettes.Select((p, idx) => new PaletteRepresentation(idx, p)));
    }

    partial void OnPalettesChanged(ObservableCollection<PaletteRepresentation> value)
    {
        SelectedPalette = value.FirstOrDefault();
    }

    partial void OnSelectedPaletteChanged(PaletteRepresentation? value)
    {
        SelectedColor = value?.Colors.FirstOrDefault() ?? default;
    }

    [RelayCommand(CanExecute = nameof(CanSavePalette))]
    private async Task SavePaletteAsync()
    {
        await Task.Delay(100);
    }

    private bool CanSavePalette() => SelectedPalette is { IsError: false };

    [RelayCommand(CanExecute = nameof(CanSaveAllPalettes))]
    private async Task SaveAllPalettesAsync()
    {
        await Task.Delay(100);
    }

    private bool CanSaveAllPalettes() => Palettes.Count > 0;

    [RelayCommand(CanExecute = nameof(CanImportPalette))]
    private async Task ImportPaletteAsync()
    {
        await Task.Delay(100);
    }

    private bool CanImportPalette() => SelectedPalette is not null;

    [RelayCommand]
    private async Task ImportAllPalettesAsync()
    {
        await Task.Delay(100);
    }
}
