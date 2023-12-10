namespace SceneGate.UI.Formats.Graphics;

using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SceneGate.UI.Formats.Mvvm;
using Texim.Colors;
using Texim.Compressions.Nitro;
using Texim.Formats;
using Texim.Images;
using Texim.Palettes;
using Yarhl.FileFormat;
using Yarhl.IO;

/// <summary>
/// View model to display Yarhl images.
/// </summary>
public partial class ImageViewModel : ObservableObject, IFormatViewModel
{
    [ObservableProperty]
    private IFullImage image;

    [ObservableProperty]
    private IFormat sourceFormat;

    [ObservableProperty]
    private Bitmap? bitmap;

    [ObservableProperty]
    private bool canShowPalette;

    [ObservableProperty]
    private Bitmap? paletteImage;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageViewModel"/> class.
    /// </summary>
    public ImageViewModel()
    {
        if (Design.IsDesignMode) {
            byte[] bytes = new byte[256 * 192 * 4];
            var random = new Random(42);
            random.NextBytes(bytes);
            Rgb[] pixels = bytes.DecodeColorsAs<Rgb32>();

            sourceFormat = null!;
            Image = new FullImage(256, 192) { Pixels = pixels };

            var testPalette = new Palette(pixels[..256]);
            using BinaryFormat palettePng = new Palette2Bitmap().Convert(testPalette);
            palettePng.Stream.Position = 0;
            PaletteImage = new Bitmap(palettePng.Stream);
            CanShowPalette = true;
        } else {
            image = null!;
            sourceFormat = null!;
        }

        AskOutputFile = new AsyncInteraction<IStorageFile?>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageViewModel"/> class.
    /// </summary>
    /// <param name="fullImage">Image to display.</param>
    public ImageViewModel(IFullImage fullImage)
        : this()
    {
        Image = fullImage;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageViewModel"/> class.
    /// </summary>
    /// <param name="indexedImage">Image to display.</param>
    /// <param name="palettes">Palette for the indexed image.</param>
    public ImageViewModel(IIndexedImage indexedImage, IPaletteCollection palettes)
        : this()
    {
        ArgumentNullException.ThrowIfNull(indexedImage);
        ArgumentNullException.ThrowIfNull(palettes);

        Image = new Indexed2FullImage(palettes).Convert(indexedImage);
        SourceFormat = indexedImage;

        CanShowPalette = true;
        using BinaryFormat palettePng = new Palette2Bitmap().Convert(palettes.Palettes[0]);
        palettePng.Stream.Position = 0;
        PaletteImage = new Bitmap(palettePng.Stream);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageViewModel"/> class.
    /// </summary>
    /// <param name="map">Image compression information.</param>
    /// <param name="indexedImage">Image to display.</param>
    /// <param name="palettes">Palette for the indexed image.</param>
    public ImageViewModel(IScreenMap map, IIndexedImage indexedImage, IPaletteCollection palettes)
        : this()
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(indexedImage);
        ArgumentNullException.ThrowIfNull(palettes);

        indexedImage = new MapDecompression(map).Convert(indexedImage);
        Image = new Indexed2FullImage(palettes).Convert(indexedImage);
        SourceFormat = indexedImage;

        // Don't show the palette as there are many
        CanShowPalette = false;
    }

    /// <summary>
    /// Ask the user to select the output file to save.
    /// </summary>
    public AsyncInteraction<IStorageFile?> AskOutputFile { get; }

    /// <summary>
    /// Save the current image to disk.
    /// </summary>
    /// <returns></returns>
    [RelayCommand(CanExecute = nameof(CanSaveImage))]
    public async Task SaveImageAsync()
    {
        if (Bitmap is null) {
            return;
        }

        IStorageFile? file = await AskOutputFile.HandleAsync().ConfigureAwait(false);
        if (file is null) {
            return;
        }

        using Stream output = await file.OpenWriteAsync().ConfigureAwait(false);
        Bitmap.Save(output);
    }

    /// <summary>
    /// Gets a value indicating if it can save the image.
    /// </summary>
    /// <returns>Value indicating if it can save the image..</returns>
    public bool CanSaveImage() => Bitmap is not null;

    partial void OnImageChanged(IFullImage value)
    {
        SourceFormat = value;
        UpdateImage();
    }

    private void UpdateImage()
    {
        ConvertToBitmap();
    }

    private void ConvertToBitmap()
    {
        var converter = new FullImage2Bitmap();
        using BinaryFormat binaryPng = converter.Convert(Image);

        binaryPng.Stream.Position = 0;
        Bitmap = new Bitmap(binaryPng.Stream);
    }
}
