namespace SceneGate.UI.Formats.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Texim.Colors;
using Texim.Formats;
using Texim.Palettes;
using Yarhl.IO;

public partial class BinaryPaletteViewModel : ObservableObject
{
    private readonly BinaryFormat binaryFormat;

    [ObservableProperty]
    private IPaletteCollection rawPalettes;

    [ObservableProperty]
    private PaletteViewModel paletteInfo;

    [ObservableProperty]
    private long offset;

    [ObservableProperty]
    private long length;

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
            binaryFormat = new BinaryFormat(DataStreamFactory.FromArray(colorBytes));
        } else {
            binaryFormat = null!;
        }

        length = binaryFormat.Stream.Length;
        colorsPerPalette = 16;

        rawPalettes = new PaletteCollection();
        PaletteInfo = new PaletteViewModel(rawPalettes) {
            IsModelPropertyVisible = false,
        };


        ReadRawPalette();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryPaletteViewModel"/> class.
    /// </summary>
    /// <param name="stream">The raw data to analyze.</param>
    public BinaryPaletteViewModel(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(nameof(stream));

        binaryFormat = new BinaryFormat(stream);

        rawPalettes = new PaletteCollection();
        PaletteInfo = new PaletteViewModel(rawPalettes) {
            IsModelPropertyVisible = false,
        };
        ReadRawPalette();
    }

    partial void OnOffsetChanged(long value)
    {
        ReadRawPalette();
    }

    partial void OnLengthChanged(long value)
    {
        ReadRawPalette();
    }

    partial void OnColorsPerPaletteChanged(int value)
    {
        ReadRawPalette();
    }

    private void ReadRawPalette()
    {
        byte[] buffer = new byte[Length];
        binaryFormat.Stream.Position = Offset;
        int read = binaryFormat.Stream.Read(buffer);

        var textBuilder = new StringBuilder();
        for (int i = 0; i < read; i++) {
            if (i + 1 == read) {
                textBuilder.AppendFormat("{0:X2}", buffer[i]);
            } else if (i != 0 && ((i + 1) % 16 == 0)) {
                textBuilder.AppendFormat("{0:X2}\n", buffer[i]);
            } else {
                textBuilder.AppendFormat("{0:X2} ", buffer[i]);
            }
        }
        HexContent = textBuilder.ToString();

        var options = new RawPaletteParams() {
            Offset = Offset,
            Size = (int)Length,
            ColorEncoding = new Bgr555(),
            ColorsPerPalette = ColorsPerPalette,
        };
        var raw2Palette = new RawBinary2PaletteCollection(options);

        IPaletteCollection palettes;
        try {
            palettes = raw2Palette.Convert(binaryFormat);
        } catch {
            return;
        }

        PaletteInfo.UpdatePalettes(palettes);
    }
}
