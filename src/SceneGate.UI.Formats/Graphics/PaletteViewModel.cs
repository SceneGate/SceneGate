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
using Yarhl.FileFormat;
using Yarhl.IO;

/// <summary>
/// View model to display palettes.
/// </summary>
public partial class PaletteViewModel : ObservableObject, IFormatViewModel
{
    [ObservableProperty]
    private IFormat sourceFormat;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveAllPalettesCommand))]
    private ObservableCollection<PaletteRepresentation> palettes;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SavePaletteCommand))]
    [NotifyCanExecuteChangedFor(nameof(ImportPaletteCommand))]
    private PaletteRepresentation? selectedPalette;

    [ObservableProperty]
    private Color selectedColor;

    [ObservableProperty]
    private HsvColor selectedHsvColor;

    [ObservableProperty]
    private string? selectedHexColor;

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

        SourceFormat = testPalettes[0];
        Palettes = new(testPalettes.Select((p, idx) => new PaletteRepresentation(idx, p)));

        AskOutputFile = new AsyncInteraction<IStorageFile?>();
        AskOutputFolder = new AsyncInteraction<IStorageFolder?>();
        AskInputFile = new AsyncInteraction<IStorageFile?>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaletteViewModel"/> class.
    /// </summary>
    /// <param name="palette">The palette to represent.</param>
    public PaletteViewModel(IPalette palette)
    {
        SourceFormat = palette;
        Palettes = [new PaletteRepresentation(0, palette)];
        AskOutputFile = new AsyncInteraction<IStorageFile?>();
        AskOutputFolder = new AsyncInteraction<IStorageFolder?>();
        AskInputFile = new AsyncInteraction<IStorageFile?>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaletteViewModel"/> class.
    /// </summary>
    /// <param name="palettes">The collection of palettes to represent.</param>
    public PaletteViewModel(IPaletteCollection palettes)
    {
        SourceFormat = palettes;
        Palettes = new(palettes.Palettes.Select((p, idx) => new PaletteRepresentation(idx, p)));
        AskOutputFile = new AsyncInteraction<IStorageFile?>();
        AskOutputFolder = new AsyncInteraction<IStorageFolder?>();
        AskInputFile = new AsyncInteraction<IStorageFile?>();
    }

    /// <summary>
    /// Gets the interaction to ask the user for the output file to save the palette.
    /// </summary>
    public AsyncInteraction<IStorageFile?> AskOutputFile { get; }

    /// <summary>
    /// Gets the interaction to ask the user for the output folder to save all the palettes.
    /// </summary>
    public AsyncInteraction<IStorageFolder?> AskOutputFolder { get; }

    /// <summary>
    /// Gets the interaction to ask the usre for the input file to import.
    /// </summary>
    public AsyncInteraction<IStorageFile?> AskInputFile { get; }

    partial void OnPalettesChanged(ObservableCollection<PaletteRepresentation> value)
    {
        SelectedPalette = value.FirstOrDefault();
    }

    partial void OnSelectedPaletteChanged(PaletteRepresentation? value)
    {
        SelectedColor = value?.Colors.FirstOrDefault() ?? default;
    }

    partial void OnSelectedColorChanged(Color value)
    {
        SelectedHsvColor = value.ToHsv();
        SelectedHexColor = $"{value.R:X2}{value.G:X2}{value.B:X2}";
    }

    /// <summary>
    /// Save the current selected palette into a disk file as RIFF or PNG format.
    /// </summary>
    /// <returns>The asynchronous operation.</returns>
    [RelayCommand(CanExecute = nameof(CanSavePalette))]
    public async Task SavePaletteAsync()
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

    /// <summary>
    /// Save all the palettes into files on a selected folder.
    /// </summary>
    /// <param name="formatName">The name of the format. Either: PNG or RIFF.</param>
    /// <returns>The asynchronous operation.</returns>
    [RelayCommand(CanExecute = nameof(CanSaveAllPalettes))]
    public async Task SaveAllPalettesAsync(string formatName)
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

    /// <summary>
    /// Import the content of a disk file palette into the current view.
    /// </summary>
    /// <returns>Asynchronous task.</returns>
    /// <exception cref="NotImplementedException">No formats supported yet.</exception>
    [RelayCommand(CanExecute = nameof(CanImportPalette))]
    public async Task ImportPaletteAsync()
    {
        IStorageFile? file = await AskInputFile.HandleAsync().ConfigureAwait(false);
        if (file is null) {
            return;
        }

        throw new NotImplementedException("Converters not implemented in Texim yet");
    }

    private bool CanImportPalette() => SelectedPalette is not null;
}
