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
    /// <param name="palette">Palette for the indexed image.</param>
    public ImageViewModel(IIndexedImage indexedImage, IPaletteCollection palette)
        : this()
    {
        ArgumentNullException.ThrowIfNull(indexedImage);
        ArgumentNullException.ThrowIfNull(palette);

        Image = new Indexed2FullImage(palette).Convert(indexedImage);
        SourceFormat = indexedImage;
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
