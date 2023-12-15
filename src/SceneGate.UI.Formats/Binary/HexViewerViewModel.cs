namespace SceneGate.UI.Formats.Binary;

using System;
using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using Yarhl.IO;

/// <summary>
/// View model for the hexadecimal view.
/// </summary>
public partial class HexViewerViewModel : ObservableObject, IFormatViewModel
{
    private readonly StringBuilder textBuilder;
    private readonly Encoding utf32BigEndian;
    private readonly Stream stream;
    private bool cursorUpdate;
    private long maxScroll;
    private long currentScroll;
    private int visibleLines;
    private string offsetsText;
    private int hexCursorPos;
    private string hexText;
    private int asciiCursorPos;
    private string asciiText;
    private bool isBigEndian;
    private string customEncodingName;
    private Encoding? customEncoding;

    /// <summary>
    /// Initializes a new instance of the <see cref="HexViewerViewModel" /> class.
    /// </summary>
    public HexViewerViewModel()
        : this(DataStreamFactory.FromArray(new byte[0x100]))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HexViewerViewModel" /> class.
    /// </summary>
    /// <param name="binary">Data to view.</param>
    public HexViewerViewModel(IBinary binary)
        : this(binary.Stream)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HexViewerViewModel" /> class.
    /// </summary>
    /// <param name="stream">Data to view.</param>
    public HexViewerViewModel(Stream stream)
    {
        // Make sure that the shift-jis encoding is initialized.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        textBuilder = new StringBuilder();
        DataTypes =  [
            new DataTypeItem(typeof(byte), "8-bits"),
            new DataTypeItem(typeof(ushort), "unsigned 16-bits"),
            new DataTypeItem(typeof(short), "signed 16-bits"),
            new DataTypeItem(typeof(uint), "unsigned 32-bits"),
            new DataTypeItem(typeof(int), "signed 32-bits"),
            new DataTypeItem(typeof(ulong), "unsigned 64-bits"),
            new DataTypeItem(typeof(long), "signed 64-bits"),
            new DataTypeItem(typeof(float), "float/single 32-bits"),
            new DataTypeItem(typeof(double), "double 64-bits"),
            new DataTypeItem(typeof(UTF8Encoding), "UTF-8"),
            new DataTypeItem(typeof(UnicodeEncoding), "UTF-16"),
            new DataTypeItem(typeof(UTF32Encoding), "UTF-32"),
            new DataTypeItem(typeof(Encoding), "Custom encoding"),
        ];
        utf32BigEndian = new UTF32Encoding(true, false);
        IsBigEndian = false;
        CustomEncodingName = "shift-jis";

        offsetsText = string.Empty;
        hexText = string.Empty;
        asciiText = string.Empty;

        this.stream = stream;
        AdjustScroll();
        HexCursorPos = 0;
        UpdateVisibleText();
    }

    /// <summary>
    /// Notifies there was an update in the data type inspector values.
    /// </summary>
    public event EventHandler? OnDataTypesUpdate;

    /// <summary>
    /// Gets the number of bytes per row.
    /// </summary>
    public static int BytesPerRow => 0x10;

    /// <summary>
    /// Gets text indicating each position column.
    /// </summary>
    public static string PositionText =>
        string.Join(' ', Enumerable.Range(0, BytesPerRow).Select(i => $"{i:X2}"));

    /// <summary>
    /// Gets or sets the maximum scroll range.
    /// </summary>
    public long MaximumScroll {
        get => maxScroll;
        set => SetProperty(ref maxScroll, value);
    }

    /// <summary>
    /// Gets or sets the current scroll value.
    /// </summary>
    public long CurrentScroll {
        get => currentScroll;
        set {
            if (value < 0) {
                value = 0;
            } else if (value >= MaximumScroll) {
                value = MaximumScroll;
            }

            if (SetProperty(ref currentScroll, value)) {
                UpdateVisibleText();
            }
        }
    }

    /// <summary>
    /// Gets or sets the visible text rows.
    /// </summary>
    public int VisibleTextRows {
        get => visibleLines;
        set {
            if (SetProperty(ref visibleLines, value)) {
                AdjustScroll();
                UpdateVisibleText();
            }
        }
    }

    /// <summary>
    /// Gets or sets the text for the "offset" box.
    /// </summary>
    public string OffsetsText {
        get => offsetsText;
        set => SetProperty(ref offsetsText, value);
    }

    /// <summary>
    /// Gets or sets the caret/cursor position of the hexadecimal view.
    /// </summary>
    public int HexCursorPos {
        get => hexCursorPos;
        set {
            if (SetProperty(ref hexCursorPos, value)) {
                UpdateAsciiCursor();
                UpdateDataTypes();
            }
        }
    }

    /// <summary>
    /// Gets or sets the text for the hexadecimal view.
    /// </summary>
    public string HexText {
        get => hexText;
        set => SetProperty(ref hexText, value);
    }

    /// <summary>
    /// Gets or sets the caret/cursor position of the ASCII view.
    /// </summary>
    public int AsciiCursorPos {
        get => asciiCursorPos;
        set {
            if (SetProperty(ref asciiCursorPos, value)) {
                UpdateHexCursor();
                UpdateDataTypes();
            }
        }
    }

    /// <summary>
    /// Gets or sets the text for the ASCII view.
    /// </summary>
    public string AsciiText {
        get => asciiText;
        set => SetProperty(ref asciiText, value);
    }

    /// <summary>
    /// Gets the collection of items for the data type inspector.
    /// </summary>
    public ObservableCollection<DataTypeItem> DataTypes { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the values of the data type inspectors
    /// show as big or little endian.
    /// </summary>
    public bool IsBigEndian {
        get => isBigEndian;
        set {
            SetProperty(ref isBigEndian, value);
            UpdateDataTypes();
        }
    }

    /// <summary>
    /// Gets or sets the name of the custom encoding for the data type inspector.
    /// </summary>
    public string CustomEncodingName {
        get => customEncodingName;
        [MemberNotNull(nameof(customEncodingName))]
        set {
            SetProperty(ref customEncodingName, value);

            try {
                customEncoding = Encoding.GetEncoding(
                    value,
                    EncoderFallback.ReplacementFallback,
                    DecoderFallback.ReplacementFallback);
            } catch {
                customEncoding = null;
            }

            UpdateDataTypes();
        }
    }

    private void AdjustScroll()
    {
        // We use floor because we start at 0 so it's already one line.
        MaximumScroll = (long)Math.Floor((float)stream.Length / BytesPerRow);
    }

    private void UpdateStreamRowPosition()
    {
        stream.Position = currentScroll * BytesPerRow;

        // Pad down
        stream.Position &= (~BytesPerRow) + 1;

        // Leave at least 3 lines
        long realMax = stream.Length - (3 * BytesPerRow);
        if (realMax > 0 && stream.Position > realMax) {
            stream.Position = realMax;
        }
    }

#pragma warning disable IDE0047 // false positive
    private void UpdateStreamBytePosition()
    {
        UpdateStreamRowPosition();
        int hexCharsPerLine = BytesPerRow * 3;
        int y = HexCursorPos / hexCharsPerLine;
        int x = (HexCursorPos % hexCharsPerLine) / 3;

        int position = x + (y * BytesPerRow);
        if (stream.Position + position < stream.Length) {
            stream.Position += position;
        } else {
            stream.Position = stream.Length;
        }
    }

    private void UpdateVisibleText()
    {
        UpdateDataTypes();

        long startPosition = stream.Position;

        int numBytes = BytesPerRow * VisibleTextRows;
        byte[] buffer = new byte[numBytes];
        int read = stream.Read(buffer);

        textBuilder.Clear();
        for (int i = 0; i < read; i += BytesPerRow) {
            textBuilder.AppendFormat("{0:X8}\n", startPosition + i);
        }

        OffsetsText = textBuilder.ToString();

        textBuilder.Clear();
        for (int i = 0; i < read; i++) {
            if (i + 1 == read) {
                textBuilder.AppendFormat("{0:X2}", buffer[i]);
            } else if (i != 0 && ((i + 1) % BytesPerRow == 0)) {
                textBuilder.AppendFormat("{0:X2}\n", buffer[i]);
            } else {
                textBuilder.AppendFormat("{0:X2} ", buffer[i]);
            }
        }

        HexText = textBuilder.ToString();

        textBuilder.Clear();
        for (int i = 0; i < read; i++) {
            char ch = (buffer[i] >= 0x21 && buffer[i] <= 0x7F) ? (char)buffer[i] : '.';
            if (i + 1 == read) {
                textBuilder.Append(ch);
            } else if (i != 0 && ((i + 1) % BytesPerRow == 0)) {
                textBuilder.AppendFormat("{0}\n", ch);
            } else {
                textBuilder.AppendFormat("{0} ", ch);
            }
        }

        AsciiText = textBuilder.ToString();
    }

    private void UpdateAsciiCursor()
    {
        if (cursorUpdate) {
            return;
        }

        cursorUpdate = true;

        int hexCharsPerLine = BytesPerRow * 3;
        int asciiCharsPerLine = BytesPerRow * 2;
        int y = HexCursorPos / hexCharsPerLine;
        int x = (HexCursorPos % hexCharsPerLine) / 3;
        AsciiCursorPos = (x * 2) + (y * asciiCharsPerLine);

        cursorUpdate = false;
    }

    private void UpdateHexCursor()
    {
        if (cursorUpdate) {
            return;
        }

        cursorUpdate = true;

        int hexCharsPerLine = BytesPerRow * 3;
        int asciiCharsPerLine = BytesPerRow * 2;
        int y = AsciiCursorPos / asciiCharsPerLine;
        int x = (AsciiCursorPos % asciiCharsPerLine) / 2;
        HexCursorPos = (x * 3) + (y * hexCharsPerLine);

        cursorUpdate = false;
    }
#pragma warning restore IDE0047

    private void UpdateDataTypes()
    {
        if (stream is null) {
            return;
        }

        UpdateStreamBytePosition();
        ClearTypeValues();

        byte[] buffer = new byte[16];
        int read = stream.Read(buffer, 0, buffer.Length);
        stream.Position -= read;
        if (read == 0) {
            OnDataTypesUpdate?.Invoke(this, EventArgs.Empty);
            return;
        }

        ulong value = IsBigEndian
            ? BinaryPrimitives.ReadUInt64BigEndian(buffer)
            : BinaryPrimitives.ReadUInt64LittleEndian(buffer);

        int numBits = 8;
        if (read >= 2) {
            numBits = 16;
            ushort bits16 = IsBigEndian ? (ushort)(value >> (64 - 16)) : (ushort)value;
            UpdateTypeValue<ushort>(bits16.ToString(CultureInfo.CurrentCulture));
            UpdateTypeValue<short>(((short)bits16).ToString(CultureInfo.CurrentCulture));
        }

        if (read >= 4) {
            numBits = 32;
            uint bits32 = IsBigEndian ? (uint)(value >> (64 - 32)) : (uint)value;
            UpdateTypeValue<uint>(bits32.ToString(CultureInfo.CurrentCulture));
            UpdateTypeValue<int>(((int)bits32).ToString(CultureInfo.CurrentCulture));

            float floatValue = IsBigEndian
                ? BinaryPrimitives.ReadSingleBigEndian(buffer)
                : BinaryPrimitives.ReadSingleLittleEndian(buffer);
            UpdateTypeValue<float>(floatValue.ToString(CultureInfo.CurrentCulture));
        }

        if (read >= 8) {
            UpdateTypeValue<ulong>(value.ToString(CultureInfo.CurrentCulture));
            UpdateTypeValue<long>(((long)value).ToString(CultureInfo.CurrentCulture));

            double doubleValue = IsBigEndian
                ? BinaryPrimitives.ReadDoubleBigEndian(buffer)
                : BinaryPrimitives.ReadDoubleLittleEndian(buffer);
            UpdateTypeValue<double>(doubleValue.ToString(CultureInfo.CurrentCulture));
        }

        // We want to start from MSB (high) so LSB is on the right.
        textBuilder.Clear();
        uint bitValue = IsBigEndian ? (uint)(value >> (64 - numBits)) : (uint)value;
        for (int i = numBits - 1; i >= 0; i--) {
            textBuilder.Append((bitValue & (1u << i)) == 0 ? "0" : "1");
            if (i != 0 && (i % 8) == 0) {
                textBuilder.Append(' ');
            }
        }

        UpdateTypeValue<byte>(textBuilder.ToString());

        UpdateTypeValue<UTF8Encoding>(Regex.Escape(Encoding.UTF8.GetString(buffer)));

        var utf16 = IsBigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode;
        UpdateTypeValue<UnicodeEncoding>(Regex.Escape(utf16.GetString(buffer)));

        var utf32 = IsBigEndian ? Encoding.UTF32 : utf32BigEndian;
        UpdateTypeValue<UTF32Encoding>(Regex.Escape(utf32.GetString(buffer)));

        if (customEncoding is not null) {
            UpdateTypeValue<Encoding>(Regex.Escape(customEncoding.GetString(buffer)));
        } else {
            UpdateTypeValue<Encoding>("Unknown encoding name");
        }

        // Eto GridView doesn't implement MVVM binding.
        // https://github.com/picoe/Eto/issues/530
        OnDataTypesUpdate?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateTypeValue<T>(string value)
    {
        var type = typeof(T);
        DataTypes.First(i => i.Type == type).Value = value;
    }

    private void ClearTypeValues()
    {
        foreach (var item in DataTypes) {
            item.Value = string.Empty;
        }
    }

    /// <summary>
    /// Item of the data type inspector.
    /// </summary>
    public partial class DataTypeItem : ObservableObject
    {
        /// <summary>
        /// Gets or sets the value for this type instance.
        /// </summary>
        [ObservableProperty]
        private string value;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypeItem" /> class.
        /// </summary>
        /// <param name="type">The type to represent.</param>
        /// <param name="description">The description of the type.</param>
        public DataTypeItem(Type type, string description)
        {
            Type = type;
            Description = description;
            value = string.Empty;
        }

        /// <summary>
        /// Gets the type of the item.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the description of the type.
        /// </summary>
        public string Description { get; }
    }
}
