namespace SceneGate.UI.Formats.Graphics;

using System;
using System.Buffers;
using System.IO;
using System.Text;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Texim.Formats;
using Texim.Images;
using Texim.Pixels;
using Yarhl.IO;

/// <summary>
/// View model to set and display the options of raw images.
/// </summary>
public partial class RawImageOptionsViewModel : ObservableObject
{
    private readonly IBinary binaryFormat;
    private readonly StringBuilder hexBuilder;

    [ObservableProperty]
    private IndexedImage? image;

    [ObservableProperty]
    private long offset;

    [ObservableProperty]
    private long maximumOffset;

    [ObservableProperty]
    private int width;

    [ObservableProperty]
    private int height;

    [ObservableProperty]
    private int pixelEncoding;

    [ObservableProperty]
    private int swizzlingKind;

    [ObservableProperty]
    private bool isTiled;

    [ObservableProperty]
    private int tileWidth;

    [ObservableProperty]
    private int tileHeight;

    [ObservableProperty]
    private string hexContent;

    /// <summary>
    /// Initializes a new instance of the <see cref="RawImageOptionsViewModel"/> class.
    /// </summary>
    public RawImageOptionsViewModel()
    {
        Offset = 0;
        TileWidth = 8;
        TileHeight = 8;
        hexContent = string.Empty;
        hexBuilder = new StringBuilder();

        if (Design.IsDesignMode) {
            Width = 256;
            Height = 192;

            var random = new Random(42);
            byte[] colorBytes = new byte[Width * Height];
            random.NextBytes(colorBytes);
            DataStream stream = DataStreamFactory.FromArray(colorBytes);
            binaryFormat = new BinaryFormat(stream);

            ReadImage();
        } else {
            binaryFormat = null!;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RawImageOptionsViewModel"/> class.
    /// </summary>
    /// <param name="binaryFormat">The binary data to see as image.</param>
    public RawImageOptionsViewModel(IBinary binaryFormat)
        : this()
    {
        ArgumentNullException.ThrowIfNull(binaryFormat);

        // TODO: guess sizes
        this.binaryFormat = binaryFormat;
        MaximumOffset = binaryFormat.Stream.Length;

        ReadImage();
    }

    private int Size => (int)Math.Ceiling(Width * Height * Indexed4Bpp.Instance.BitsPerPixel / 8.0);

    private void ReadImage()
    {
        ReadHexContent();
        ConvertImage();
    }

    private void ConvertImage()
    {
        var options = new RawIndexedImageParams {
            Width = Width,
            Height = Height,
            Offset = Offset,
            PixelEncoding = Indexed8Bpp.Instance,
            Size = Size,
            Swizzling = new TileSwizzling<IndexedPixel>(new(TileWidth, TileHeight), Width),
        };
        Image = new RawBinary2IndexedImage(options).Convert(binaryFormat);
    }

    private void ReadHexContent()
    {
        binaryFormat.Stream.Position = Offset;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(Size);
        int read = binaryFormat.Stream.Read(buffer, 0, Size);

        hexBuilder.Clear();
        for (int i = 0; i < read; i++) {
            if (i + 1 == read) {
                hexBuilder.AppendFormat("{0:X2}", buffer[i]);
            } else if (i != 0 && ((i + 1) % 16 == 0)) {
                hexBuilder.AppendFormat("{0:X2}\n", buffer[i]);
            } else {
                hexBuilder.AppendFormat("{0:X2} ", buffer[i]);
            }
        }

        HexContent = hexBuilder.ToString();
        ArrayPool<byte>.Shared.Return(buffer);
    }
}
