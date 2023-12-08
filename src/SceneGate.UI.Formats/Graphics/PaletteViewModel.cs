namespace SceneGate.UI.Formats.Graphics;

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SceneGate.UI.Formats.Mvvm;
using Texim.Colors;
using Texim.Formats;
using Texim.Palettes;
using Yarhl.IO;

/// <summary>
/// View model to display palettes.
/// </summary>
public partial class PaletteViewModel : ObservableObject, IFormatViewModel
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveAllPalettesCommand))]
    private ObservableCollection<PaletteRepresentation> palettes;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SavePaletteCommand))]
    [NotifyCanExecuteChangedFor(nameof(ImportPaletteCommand))]
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
        AskOutputFile = new AsyncInteraction<IStorageFile?>();
        AskOutputFolder = new AsyncInteraction<IStorageFolder?>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaletteViewModel"/> class.
    /// </summary>
    /// <param name="palette">The palette to represent.</param>
    public PaletteViewModel(IPalette palette)
    {
        Palettes = [new PaletteRepresentation(0, palette)];
        AskOutputFile = new AsyncInteraction<IStorageFile?>();
        AskOutputFolder = new AsyncInteraction<IStorageFolder?>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaletteViewModel"/> class.
    /// </summary>
    /// <param name="palettes">The collection of palettes to represent.</param>
    public PaletteViewModel(IPaletteCollection palettes)
    {
        Palettes = new(palettes.Palettes.Select((p, idx) => new PaletteRepresentation(idx, p)));
        AskOutputFile = new AsyncInteraction<IStorageFile?>();
        AskOutputFolder = new AsyncInteraction<IStorageFolder?>();
    }

    /// <summary>
    /// Gets the interaction to ask the user for the output file to save the palette.
    /// </summary>
    public AsyncInteraction<IStorageFile?> AskOutputFile { get; }

    /// <summary>
    /// Gets the interaction to ask the user for the output folder to save all the palettes.
    /// </summary>
    public AsyncInteraction<IStorageFolder?> AskOutputFolder { get; }

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
        if (SelectedPalette is null) {
            return;
        }

        IStorageFile? file = await AskOutputFile.HandleAsync().ConfigureAwait(false);
        if (file is null) {
            return;
        }

        BinaryFormat outputFormat;
        if (file.Name.EndsWith(".pal")) {
            var palette2Riff = new Palette2BinaryRiff(gimpCompatibility: true);
            outputFormat = palette2Riff.Convert(SelectedPalette.Palette);
        } else {
            var palette2Png = new Palette2Bitmap();
            outputFormat = palette2Png.Convert(SelectedPalette.Palette);
        }

        using Stream output = await file.OpenWriteAsync().ConfigureAwait(false);
        outputFormat.Stream.WriteTo(output);
        outputFormat.Dispose();
    }

    private bool CanSavePalette() => SelectedPalette is { IsError: false };

    [RelayCommand(CanExecute = nameof(CanSaveAllPalettes))]
    private async Task SaveAllPalettesAsync(string formatName)
    {
        IStorageFolder? folder = await AskOutputFolder.HandleAsync().ConfigureAwait(false);
        if (folder is null) {
            return;
        }

        foreach (PaletteRepresentation palette in Palettes) {
            if (palette.IsError) {
                continue;
            }

            BinaryFormat outputFormat;
            string extension;
            if (formatName == "RIFF") {
                var palette2Riff = new Palette2BinaryRiff(gimpCompatibility: true);
                outputFormat = palette2Riff.Convert(palette.Palette);
                extension = ".pal";
            } else {
                var palette2Png = new Palette2Bitmap();
                outputFormat = palette2Png.Convert(palette.Palette);
                extension = ".png";
            }

            string name = $"palette{palette.Index:D2}" + extension;
            using IStorageFile? file = await folder.CreateFileAsync(name).ConfigureAwait(false);
            if (file is null) {
                outputFormat.Dispose();
                continue;
            }

            using Stream output = await file.OpenWriteAsync().ConfigureAwait(false);
            outputFormat.Stream.WriteTo(output);
            outputFormat.Dispose();
        }
    }

    private bool CanSaveAllPalettes() => Palettes.Count > 0;

    [RelayCommand(CanExecute = nameof(CanImportPalette))]
    private Task ImportPaletteAsync()
    {
        throw new NotImplementedException();
    }

    private bool CanImportPalette() => SelectedPalette is not null;

    [RelayCommand]
    private Task ImportAllPalettesAsync()
    {
        throw new NotImplementedException();
    }
}
