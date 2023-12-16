namespace SceneGate.UI.Formats.Graphics;

using System;
using System.Buffers;
using System.IO;
using System.Text;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Texim.Formats;
using Texim.Palettes;
using Yarhl.IO;

public partial class BinaryPaletteViewModel : ObservableObject, IFormatViewModel
{
    private const int DefaultMaxPalettes = 1;
    private const int DefaultMaxLength = 256 * 2 * DefaultMaxPalettes;

    private readonly Stream stream;
    private readonly IBinary binaryFormat;
    private readonly StringBuilder hexBuilder;

    [ObservableProperty]
    private IPaletteCollection rawPalettes;

    [ObservableProperty]
    private PaletteViewModel paletteInfo;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MaximumPalettes))]
    private long offset;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MaximumOffset))]
    private int palettesCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MaximumPalettes))]
    [NotifyPropertyChangedFor(nameof(MaximumOffset))]
    private int colorsPerPalette;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MaximumPalettes))]
    [NotifyPropertyChangedFor(nameof(MaximumOffset))]
    private ColorKind colorKind;

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

            palettesCount = 8;
            colorsPerPalette = 16;
            colorKind = ColorKind.BGR555;
        } else {
            stream = null!;
            binaryFormat = null!;
        }

        hexBuilder = new StringBuilder();
        hexContent = string.Empty;

        rawPalettes = new PaletteCollection();
        PaletteInfo = new PaletteViewModel(rawPalettes) {
            IsModelPropertyVisible = false,
        };

        if (Design.IsDesignMode) {
            ReadRawPalette();
        }
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

        // Try to auto-detect some good default settings from the file length
        // we load by default a maximum of 1 palettes of 256 colors.
        int length = stream.Length > DefaultMaxLength ? DefaultMaxLength : (int)stream.Length;

        offset = 0;
        colorKind = ColorKind.BGR555;
        colorsPerPalette = length >= (256 * colorKind.GetBytesPerColor()) ? 256 : 16;
        palettesCount = length / colorKind.GetBytesPerColor() / colorsPerPalette;

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

    /// <summary>
    /// Gets the list of supported color kinds.
    /// </summary>
    public static ColorKind[] AllColorKinds => Enum.GetValues<ColorKind>();

    /// <summary>
    /// Gets the maximum offset.
    /// </summary>
    public long MaximumOffset => stream.Length - Length;

    /// <summary>
    /// Gets the maximum number of palettes it's possible to read from the stream.
    /// </summary>
    public int MaximumPalettes =>
        (int)Math.Ceiling((stream.Length - Offset) / ColorKind.GetBytesPerColor() / (double)ColorsPerPalette);

    private int Length => ColorsPerPalette * ColorKind.GetBytesPerColor() * PalettesCount;

    partial void OnOffsetChanged(long value) => ReadRawPalette();

    partial void OnPalettesCountChanged(int value) => ReadRawPalette();

    partial void OnColorsPerPaletteChanged(int value) => ReadRawPalette();

    partial void OnColorKindChanged(ColorKind value) => ReadRawPalette();

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
            ColorEncoding = ColorKind.GetEncoding(),
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
