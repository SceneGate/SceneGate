namespace SceneGate.UI.Formats.Graphics;

using System;
using System.Buffers;
using System.IO;
using System.Text;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Texim.Colors;
using Texim.Formats;
using Texim.Palettes;
using Yarhl.IO;

public partial class BinaryPaletteViewModel : ObservableObject
{
    private const int DefaultMaxLength = 256 * 2 * 16;

    private readonly Stream stream;
    private readonly IBinary binaryFormat;
    private readonly StringBuilder hexBuilder;

    [ObservableProperty]
    private IPaletteCollection rawPalettes;

    [ObservableProperty]
    private PaletteViewModel paletteInfo;

    [ObservableProperty]
    private long offset;

    [ObservableProperty]
    private long maximumOffset;

    [ObservableProperty]
    private int length;

    [ObservableProperty]
    private int maximumLength;

    [ObservableProperty]
    private int colorsPerPalette;

    [ObservableProperty]
    private string hexContent;

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryPaletteViewModel"/> class.
    /// </summary>
    public BinaryPaletteViewModel()
    {
        if (Design.IsDesignMode) {
            var random = new Random(42);
            byte[] colorBytes = new byte[256 * 2];
            random.NextBytes(colorBytes);
            stream = DataStreamFactory.FromArray(colorBytes);
            binaryFormat = new BinaryFormat(stream);
        } else {
            stream = null!;
            binaryFormat = null!;
        }

        hexBuilder = new StringBuilder();

        maximumOffset = stream.Length - 512;
        length = 512;
        maximumLength = (int)stream.Length;
        colorsPerPalette = 16;
        hexContent = string.Empty;

        rawPalettes = new PaletteCollection();
        PaletteInfo = new PaletteViewModel(rawPalettes) {
            IsModelPropertyVisible = false,
        };

        ReadRawPalette();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryPaletteViewModel"/> class.
    /// </summary>
    /// <param name="binary">The raw data to analyze.</param>
    public BinaryPaletteViewModel(IBinary binary)
        : this()
    {
        ArgumentNullException.ThrowIfNull(nameof(binary));

        stream = binary.Stream;
        binaryFormat = binary;

        // Try to auto-detect some good default settings
        offset = 0;
        length = stream.Length > DefaultMaxLength ? DefaultMaxLength : (int)stream.Length;
        maximumOffset = stream.Length - length;
        maximumLength = length;
        colorsPerPalette = length >= (256 * 2) ? 256 : 16;

        ReadRawPalette();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryPaletteViewModel"/> class.
    /// </summary>
    /// <param name="stream">The raw data to analyze.</param>
    public BinaryPaletteViewModel(Stream stream)
        : this(new BinaryFormat(stream))
    {
    }

    partial void OnOffsetChanged(long value)
    {
        MaximumLength = (stream.Length - value) > DefaultMaxLength
            ? DefaultMaxLength
            : (int)(stream.Length - value);

        ReadRawPalette();
    }

    partial void OnLengthChanged(int value)
    {
        MaximumOffset = stream.Length - value;

        ReadRawPalette();
    }

    partial void OnColorsPerPaletteChanged(int value)
    {
        ReadRawPalette();
    }

    private void ReadRawPalette()
    {
        UpdateHexContent();
        ConvertRawPalette();
    }

    private void UpdateHexContent()
    {
        stream.Position = Offset;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(Length);
        int read = stream.Read(buffer, 0, Length);

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

    private void ConvertRawPalette()
    {
        var options = new RawPaletteParams() {
            Offset = Offset,
            Size = Length,
            ColorsPerPalette = ColorsPerPalette,
            ColorEncoding = Bgr555.Instance,
        };
        var raw2Palette = new RawBinary2PaletteCollection(options);

        IPaletteCollection palettes;
        try {
            palettes = raw2Palette.Convert(new BinaryFormat(binaryFormat.Stream));
        } catch {
            palettes = new PaletteCollection();
        }

        PaletteInfo.ReplacePalettes(palettes);
    }
}
