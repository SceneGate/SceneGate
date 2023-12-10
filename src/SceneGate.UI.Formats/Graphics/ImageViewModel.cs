namespace SceneGate.UI.Formats.Graphics;

using System;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Texim.Colors;
using Texim.Formats;
using Texim.Images;
using Yarhl.FileFormat;
using Yarhl.IO;

/// <summary>
/// View model to display Yarhl images.
/// </summary>
public partial class ImageViewModel : ObservableObject, IFormatViewModel
{
    private const int MinZoom = 50;
    private const int MaxZoom = 10_000;
    private const double ZoomDelta = 1.1; // 10%

    [ObservableProperty]
    private IFullImage image;

    [ObservableProperty]
    private IFormat sourceFormat;

    [ObservableProperty]
    private Bitmap? bitmap;

    [ObservableProperty]
    private int width;

    [ObservableProperty]
    private int height;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ZoomInCommand))]
    [NotifyCanExecuteChangedFor(nameof(ZoomOutCommand))]
    private int zoom;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageViewModel"/> class.
    /// </summary>
    public ImageViewModel()
    {
        zoom = 100;

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
    /// Zoom in the image.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanZoomIn))]
    public void ZoomIn()
    {
        double newZoom = Math.Round(Zoom * ZoomDelta);
        Zoom = (int)Math.Clamp(newZoom, MinZoom, MaxZoom);
    }

    /// <summary>
    /// Gets a value indicating if the image can zoom in.
    /// </summary>
    /// <returns>Value indicating if the image can zoom in.</returns>
    public bool CanZoomIn() => Zoom < MaxZoom;

    /// <summary>
    /// Zoom out the image.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanZoomOut))]
    public void ZoomOut()
    {
        double newZoom = Math.Round(Zoom / ZoomDelta);
        Zoom = (int)Math.Clamp(newZoom, MinZoom, MaxZoom);
    }

    /// <summary>
    /// Gets a value indicating if the image can zoom out.
    /// </summary>
    /// <returns>Value indicating if the image can zoom out.</returns>
    public bool CanZoomOut() => Zoom > MinZoom;

    partial void OnImageChanged(IFullImage value)
    {
        SourceFormat = value;
        Width = value.Width * Zoom / 100;
        Height = value.Height * Zoom / 100;
        UpdateImage();
    }

    partial void OnZoomChanged(int value)
    {
        Width = Image.Width * Zoom / 100;
        Height = Image.Height * Zoom / 100;
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
