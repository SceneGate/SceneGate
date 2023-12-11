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
    private readonly IPaletteCollection? palettes;
    private IIndexedImage? indexedImage;

    [ObservableProperty]
    private IFullImage? image;

    [ObservableProperty]
    private IFormat? sourceFormat;

    [ObservableProperty]
    private Bitmap? bitmap;

    [ObservableProperty]
    private Bitmap? paletteImage;

    [ObservableProperty]
    private bool isSinglePalette;

    [ObservableProperty]
    private bool canChangeToMultiPalette;

    [ObservableProperty]
    private int paletteIndex;

    [ObservableProperty]
    private int maximumPaletteIndex;

    [ObservableProperty]
    private bool isFirstColorTransparent;

    [ObservableProperty]
    private bool isRawImage;

    [ObservableProperty]
    private RawImageOptionsViewModel? rawImageOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageViewModel"/> class.
    /// </summary>
    private ImageViewModel()
    {
        isFirstColorTransparent = false;
        paletteIndex = 0;

        AskOutputFile = new AsyncInteraction<IStorageFile?>();
        CopyImageToClipboard = new AsyncInteraction<Bitmap, object?>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageViewModel"/> class.
    /// </summary>
    /// <param name="fullImage">Image to display.</param>
    public ImageViewModel(IFullImage fullImage)
        : this()
    {
        ArgumentNullException.ThrowIfNull(fullImage);

        sourceFormat = fullImage;
        isSinglePalette = false;
        canChangeToMultiPalette = false;
        isRawImage = false;

        image = fullImage;
        UpdateImage();
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

        sourceFormat = indexedImage;
        isSinglePalette = true;
        canChangeToMultiPalette = true;
        isRawImage = false;

        this.indexedImage = indexedImage;
        this.palettes = palettes;
        maximumPaletteIndex = palettes.Palettes.Count - 1;

        UpdateIndexedImage();
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

        sourceFormat = map;
        isSinglePalette = false;
        canChangeToMultiPalette = false;
        isRawImage = false;

        this.indexedImage = new MapDecompression(map).Convert(indexedImage);
        this.palettes = palettes;

        UpdateIndexedImage();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageViewModel"/> class.
    /// </summary>
    /// <param name="binaryImage">Binary data to display as image.</param>
    /// <param name="palettes">Palette for the indexed image.</param>
    public ImageViewModel(IBinary binaryImage, IPaletteCollection palettes)
        : this()
    {
        ArgumentNullException.ThrowIfNull(binaryImage);
        ArgumentNullException.ThrowIfNull(palettes);

        this.palettes = palettes;

        isSinglePalette = true;
        canChangeToMultiPalette = false;
        isRawImage = true;
        rawImageOptions = new RawImageOptionsViewModel(binaryImage);

        rawImageOptions.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(RawImageOptionsViewModel.Image)) {
                indexedImage = rawImageOptions.Image;
                UpdateIndexedImage();
            }
        };
    }

    /// <summary>
    /// Gets the action to ask the user to select the output file to save.
    /// </summary>
    public AsyncInteraction<IStorageFile?> AskOutputFile { get; }

    /// <summary>
    /// Gets the action to copy the provided image to the clipboard.
    /// </summary>
    public AsyncInteraction<Bitmap, object?> CopyImageToClipboard { get; }

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

    /// <summary>
    /// Copy the current image to the clipboard.
    /// </summary>
    /// <returns>Asynchronous task.</returns>
    [RelayCommand(CanExecute = nameof(CanCopyImage))]
    public async Task CopyImageAsync()
    {
        if (Bitmap is null) {
            return;
        }

        await CopyImageToClipboard.HandleAsync(Bitmap).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a value indicating if it can copy the image.
    /// </summary>
    /// <returns>Value indicating if it can copy the image..</returns>
    public bool CanCopyImage() => Bitmap is not null;

    partial void OnIsFirstColorTransparentChanged(bool value) => UpdateIndexedImage();

    partial void OnPaletteIndexChanged(int value) => UpdateIndexedImage();

    partial void OnIsSinglePaletteChanged(bool value) => UpdateIndexedImage();

    partial void OnImageChanged(IFullImage? value) => UpdateImage();

    private void UpdateIndexedImage()
    {
        if (indexedImage is null || palettes is null) {
            return;
        }

        try {
            // TODO: detect palettes without colors
            Indexed2FullImage converter;
            if (IsSinglePalette) {
                IPalette paletteToUse = palettes.Palettes[PaletteIndex];
                converter = new Indexed2FullImage(new PaletteCollection(paletteToUse), IsFirstColorTransparent);

                using BinaryFormat palettePng = new Palette2Bitmap().Convert(paletteToUse);
                palettePng.Stream.Position = 0;
                PaletteImage = new Bitmap(palettePng.Stream);
            } else {
                converter = new Indexed2FullImage(palettes);
                PaletteImage = null;
            }

            // This will trigger UpdateImage()
            Image = converter.Convert(indexedImage);
        } catch {
            // TODO: log
            Image = null;
            PaletteImage = null;
        }
    }

    private void UpdateImage()
    {
        if (Image is null) {
            // TODO: show error image
            Bitmap = null;
            return;
        }

        var converter = new FullImage2Bitmap();
        using BinaryFormat binaryPng = converter.Convert(Image);

        binaryPng.Stream.Position = 0;
        Bitmap = new Bitmap(binaryPng.Stream);
    }
}
