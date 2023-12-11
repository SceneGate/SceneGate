namespace SceneGate.UI.Formats.Graphics;

using System;
using System.Buffers;
using System.Text;
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
    [NotifyPropertyChangedFor(nameof(MaximumOffset))]
    private int? width;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MaximumOffset))]
    private int? height;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MaximumOffset))]
    private PixelEncodingKind pixelEncoding;

    [ObservableProperty]
    private SwizzlingKind swizzlingKind;

    [ObservableProperty]
    private bool isTiled;

    [ObservableProperty]
    private int? tileWidth;

    [ObservableProperty]
    private int? tileHeight;

    [ObservableProperty]
    private string hexContent;

    /// <summary>
    /// Initializes a new instance of the <see cref="RawImageOptionsViewModel"/> class.
    /// </summary>
    /// <param name="binaryFormat">The binary data to see as image.</param>
    public RawImageOptionsViewModel(IBinary binaryFormat)
    {
        ArgumentNullException.ThrowIfNull(binaryFormat);

        hexContent = string.Empty;
        hexBuilder = new StringBuilder();

        this.binaryFormat = binaryFormat;
        offset = 0;

        pixelEncoding = PixelEncodingKind.Indexed8Bpp;
        width = (binaryFormat.Stream.Length > 256 / (pixelEncoding.GetBitsPerPixel() / 8)) ? 256 : 8; 
        height = (int)Math.Ceiling(binaryFormat.Stream.Length / (pixelEncoding.GetBitsPerPixel() / 8.0) / width.Value);
        height = Math.Clamp(height.Value, 8, 512);

        swizzlingKind = SwizzlingKind.None;
        isTiled = false;
        tileWidth = 8;
        tileHeight = 8;

        ReadImage();
    }

    /// <summary>
    /// Get all the possible options for the pixel encoding.
    /// </summary>
    public static PixelEncodingKind[] AllPixelEncodings => Enum.GetValues<PixelEncodingKind>();

    /// <summary>
    /// Get all the possible options for the swizzling kinds.
    /// </summary>
    public static SwizzlingKind[] AllSwizzlingKinds => Enum.GetValues<SwizzlingKind>();

    /// <summary>
    /// Gets the maximum offset value.
    /// </summary>
    public long MaximumOffset => binaryFormat.Stream.Length - Size;

    private int Size => (int)Math.Clamp(
        Math.Ceiling(Width * Height * PixelEncoding.GetBitsPerPixel() / 8.0 ?? 0),
        0,
        binaryFormat.Stream.Length);

    partial void OnOffsetChanged(long value) => ReadImage();

    partial void OnWidthChanged(int? value) => ReadImage();

    partial void OnHeightChanged(int? value) => ReadImage();

    partial void OnPixelEncodingChanged(PixelEncodingKind value) => ReadImage();

    partial void OnSwizzlingKindChanged(SwizzlingKind value)
    {
        // It doesn't affect to size, so no need to re-read hex content.
        IsTiled = value is SwizzlingKind.TiledHorizontal;
        ConvertImage();
    }

    partial void OnTileWidthChanged(int? value) => ConvertImage();

    partial void OnTileHeightChanged(int? value) => ConvertImage();

    private void ReadImage()
    {
        ReadHexContent();
        ConvertImage();
    }

    private void ConvertImage()
    {
        if (Width is null || Height is null || TileWidth is null || TileHeight is null) {
            return;
        }

        var options = new RawIndexedImageParams {
            Width = Width ?? 1,
            Height = Height ?? 1,
            Offset = Offset,
            PixelEncoding = PixelEncoding.GetIndexedEncoding(),
            Size = Size,
        };

        if (SwizzlingKind is SwizzlingKind.TiledHorizontal) {
            options.Swizzling = new TileSwizzling<IndexedPixel>(new(TileWidth ?? 1, TileHeight ?? 1), Width ?? 1);
        }

        try {
            Image = new RawBinary2IndexedImage(options).Convert(binaryFormat);
        } catch {
            // TODO: log
            Image = null;
        }
    }

    private void ReadHexContent()
    {
        binaryFormat.Stream.Position = Offset;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(Size);
        int read = binaryFormat.Stream.Read(buffer, 0, Size);

        _ = hexBuilder.Clear();
        for (int i = 0; i < read; i++) {
            if (i + 1 == read) {
                _ = hexBuilder.AppendFormat("{0:X2}", buffer[i]);
            } else if (i != 0 && ((i + 1) % 16 == 0)) {
                _ = hexBuilder.AppendFormat("{0:X2}\n", buffer[i]);
            } else {
                _ = hexBuilder.AppendFormat("{0:X2} ", buffer[i]);
            }
        }

        HexContent = hexBuilder.ToString();
        ArrayPool<byte>.Shared.Return(buffer);
    }
}
