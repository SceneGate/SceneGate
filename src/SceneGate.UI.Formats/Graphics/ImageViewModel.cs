namespace SceneGate.UI.Formats.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Texim.Colors;
using Texim.Formats;
using Texim.Images;
using Yarhl.FileFormat;
using Yarhl.IO;

public partial class ImageViewModel : ObservableObject, IFormatViewModel
{
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

    partial void OnImageChanged(IFullImage value)
    {
        SourceFormat = value;
        Width = value.Width;
        Height = value.Height;
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
