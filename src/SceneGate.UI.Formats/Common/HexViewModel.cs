// Copyright (c) 2021 SceneGate

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Yarhl.FileFormat;
using Yarhl.IO;

namespace SceneGate.UI.Formats.Common
{
    /// <summary>
    /// View model for the hexadecimal view.
    /// </summary>
    public class HexViewModel : ObservableObject, IFormatViewModel
    {
        private readonly StringBuilder textBuilder;
        private DataStream stream;
        private bool cursorUpdate;
        private int maxScroll;
        private int currentScroll;
        private int visibleLines;
        private string offsetsText;
        private int hexCursorPos;
        private string hexText;
        private int asciiCursorPos;
        private string asciiText;
        private bool isBigEndian;
        private string customEncodingName;
        private Encoding customEncoding;

        /// <summary>
        /// Initializes a new instance of the <see cref="HexViewModel" /> class.
        /// </summary>
        public HexViewModel()
        {
            textBuilder = new StringBuilder();
            DataTypes = new ObservableCollection<DataTypeItem>();
            DataTypes.Add(new DataTypeItem(typeof(byte), "8-bits"));
            DataTypes.Add(new DataTypeItem(typeof(ushort), "int 16-bits"));
            DataTypes.Add(new DataTypeItem(typeof(short), "signed int 16-bits"));
            DataTypes.Add(new DataTypeItem(typeof(uint), "int 32-bits"));
            DataTypes.Add(new DataTypeItem(typeof(int), "signed int 32-bits"));
            DataTypes.Add(new DataTypeItem(typeof(ulong), "int 64-bits"));
            DataTypes.Add(new DataTypeItem(typeof(long), "signed int 64-bits"));
            DataTypes.Add(new DataTypeItem(typeof(float), "float/single 32-bits"));
            DataTypes.Add(new DataTypeItem(typeof(double), "double 64-bits"));
            DataTypes.Add(new DataTypeItem(typeof(UTF8Encoding), "UTF-8"));
            DataTypes.Add(new DataTypeItem(typeof(UnicodeEncoding), "UTF-16"));
            DataTypes.Add(new DataTypeItem(typeof(UTF32Encoding), "UTF-32"));
            DataTypes.Add(new DataTypeItem(typeof(Encoding), "Custom encoding"));
            IsBigEndian = false;
            CustomEncodingName = "shift-jis";
        }

        /// <summary>
        /// Notifies there was an update in the data type inspector values.
        /// </summary>
        public event EventHandler OnDataTypesUpdate;

        /// <summary>
        /// Gets the number of bytes per row.
        /// </summary>
        public int BytesPerRow {
            get => 0x10;
        }

        /// <summary>
        /// Gets or sets the maximum scroll range.
        /// </summary>
        public int MaximumScroll {
            get => maxScroll;
            set => SetProperty(ref maxScroll, value);
        }

        /// <summary>
        /// Gets or sets the current scroll value.
        /// </summary>
        public int CurrentScroll {
            get => currentScroll;
            set {
                SetProperty(ref currentScroll, value);
                UpdateVisibleText();
            }
        }

        /// <summary>
        /// Gets or sets the visible text rows.
        /// </summary>
        public int VisibleTextRows {
            get => visibleLines;
            set {
                SetProperty(ref visibleLines, value);
                AdjustScroll();
                UpdateVisibleText();
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
                SetProperty(ref hexCursorPos, value);
                UpdateAsciiCursor();
                UpdateDataTypes();
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
                SetProperty(ref asciiCursorPos, value);
                UpdateHexCursor();
                UpdateDataTypes();
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

        /// <inheritdoc/>
        public bool CanShow(IFormat format)
        {
            return format is IBinary;
        }

        /// <inheritdoc/>
        public void Show(IFormat format)
        {
            if (format is not IBinary binary) {
                return;
            }

            stream = binary.Stream;
            AdjustScroll();
            HexCursorPos = 0;
            UpdateVisibleText();
        }

        private void AdjustScroll()
        {
            // We use floor because we start at 0 so it's already one line.
            MaximumScroll = (int)Math.Floor((float)stream.Length / BytesPerRow);
        }

        private void UpdateStreamRowPosition()
        {
            stream.Position = currentScroll * BytesPerRow;
        }

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

            UpdateStreamRowPosition();
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

        private void UpdateDataTypes()
        {
            if (stream is null || stream.Disposed) {
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

            ulong value = BitConverter.ToUInt64(buffer, 0);
            if (read >= 2) {
                UpdateTypeValue<ushort>(((ushort)value).ToString(CultureInfo.CurrentCulture));
                UpdateTypeValue<short>(((short)value).ToString(CultureInfo.CurrentCulture));
            }

            if (read >= 4) {
                UpdateTypeValue<uint>(((uint)value).ToString(CultureInfo.CurrentCulture));
                UpdateTypeValue<int>(((int)value).ToString(CultureInfo.CurrentCulture));
                UpdateTypeValue<float>(BitConverter.ToSingle(buffer, 0).ToString(CultureInfo.CurrentCulture));
            }

            if (read >= 8) {
                UpdateTypeValue<ulong>(value.ToString(CultureInfo.CurrentCulture));
                UpdateTypeValue<long>(((long)value).ToString(CultureInfo.CurrentCulture));
                UpdateTypeValue<double>(BitConverter.ToDouble(buffer, 0).ToString(CultureInfo.CurrentCulture));
            }

            textBuilder.Clear();
            byte bits8 = (byte)value;
            for (int i = 7; i >= 0; i--) {
                textBuilder.Append((bits8 & (1 << i)) == 0 ? "0" : "1");
            }

            UpdateTypeValue<byte>(textBuilder.ToString());

            UpdateTypeValue<UTF8Encoding>(Regex.Escape(Encoding.UTF8.GetString(buffer)));
            UpdateTypeValue<UnicodeEncoding>(Regex.Escape(Encoding.Unicode.GetString(buffer)));
            UpdateTypeValue<UTF32Encoding>(Regex.Escape(Encoding.UTF32.GetString(buffer)));

            if (customEncoding is not null) {
                UpdateTypeValue<Encoding>(Regex.Escape(customEncoding.GetString(buffer)));
            } else {
                UpdateTypeValue<Encoding>("Unknown encoding name");
            }

            // GridView doesn't implement MVVM binding.
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
        public class DataTypeItem : ObservableObject
        {
            private string dataValue;

            /// <summary>
            /// Initializes a new instance of the <see cref="DataTypeItem" /> class.
            /// </summary>
            /// <param name="type">The type to represent.</param>
            /// <param name="description">The description of the type.</param>
            public DataTypeItem(Type type, string description)
            {
                Type = type;
                Description = description;
                Value = string.Empty;
            }

            /// <summary>
            /// Gets the type of the item.
            /// </summary>
            public Type Type { get; }

            /// <summary>
            /// Gets the description of the type.
            /// </summary>
            public string Description { get; }

            /// <summary>
            /// Gets or sets the value for this type instance.
            /// </summary>
            public string Value {
                get => dataValue;
                set => SetProperty(ref dataValue, value);
            }
        }
    }
}
