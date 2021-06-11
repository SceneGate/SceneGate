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
using System.Globalization;
using System.Text;
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
        private readonly StringBuilder textBuilder = new StringBuilder();
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
        private string bitsText;
        private string ushortText;
        private string shortText;
        private string uintText;
        private string intText;
        private string ulongText;
        private string longText;
        private string floatText;
        private string doubleText;
        private string utf8Text;
        private string utf16Text;
        private string utf32Text;
        private string shiftJisText;

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
                UpdateTypeConversions();
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
                UpdateTypeConversions();
            }
        }

        /// <summary>
        /// Gets or sets the text for the ASCII view.
        /// </summary>
        public string AsciiText {
            get => asciiText;
            set => SetProperty(ref asciiText, value);
        }

        public string BitsText {
            get => bitsText;
            set => SetProperty(ref bitsText, value);
        }

        public string UshortText {
            get => ushortText;
            set => SetProperty(ref ushortText, value);
        }

        public string ShortText {
            get => shortText;
            set => SetProperty(ref shortText, value);
        }

        public string UintText {
            get => uintText;
            set => SetProperty(ref uintText, value);
        }

        public string IntText {
            get => intText;
            set => SetProperty(ref intText, value);
        }

        public string UlongText {
            get => ulongText;
            set => SetProperty(ref ulongText, value);
        }

        public string LongText {
            get => longText;
            set => SetProperty(ref longText, value);
        }

        public string FloatText {
            get => floatText;
            set => SetProperty(ref floatText, value);
        }

        public string DoubleText {
            get => doubleText;
            set => SetProperty(ref doubleText, value);
        }

        public string Utf8Text {
            get => utf8Text;
            set => SetProperty(ref utf8Text, value);
        }

        public string Utf16Text {
            get => utf16Text;
            set => SetProperty(ref utf16Text, value);
        }

        public string Utf32Text {
            get => utf32Text;
            set => SetProperty(ref utf32Text, value);
        }

        public string ShiftJisText {
            get => shiftJisText;
            set => SetProperty(ref shiftJisText, value);
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
            MaximumScroll = (int)Math.Ceiling((float)stream.Length / BytesPerRow);
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
            stream.Position += x + (y * BytesPerRow);
        }

        private void UpdateVisibleText()
        {
            UpdateTypeConversions();

            UpdateStreamRowPosition();

            textBuilder.Clear();
            for (int i = 0; i < VisibleTextRows; i++) {
                textBuilder.AppendFormat("{0:X8}\n", stream.Position + (i * BytesPerRow));
            }

            OffsetsText = textBuilder.ToString();

            int numBytes = BytesPerRow * VisibleTextRows;
            if (stream.Position + numBytes > stream.Length) {
                numBytes = (int)(stream.Length - stream.Position);
            }

            byte[] buffer = new byte[numBytes];
            stream.Read(buffer);

            textBuilder.Clear();
            for (int i = 0; i < buffer.Length; i++) {
                if (i != 0 && (i % BytesPerRow == 0)) {
                    textBuilder.AppendFormat("\n{0:X2} ", buffer[i]);
                } else {
                    textBuilder.AppendFormat("{0:X2} ", buffer[i]);
                }
            }

            HexText = textBuilder.ToString();

            textBuilder.Clear();
            for (int i = 0; i < buffer.Length; i++) {
                char ch = (buffer[i] >= 0x21 && buffer[i] <= 0x7F) ? (char)buffer[i] : '.';
                if (i != 0 && (i % BytesPerRow == 0)) {
                    textBuilder.AppendFormat("\n{0} ", ch);
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

        private void UpdateTypeConversions()
        {
            if (cursorUpdate) {
                return;
            }

            UpdateStreamBytePosition();

            byte[] buffer = new byte[16];
            int read = stream.Read(buffer, 0, buffer.Length);
            stream.Position -= read;

            ulong value = BitConverter.ToUInt64(buffer, 0);
            UshortText = ((ushort)value).ToString("X2");
            ShortText = ((short)value).ToString("X2");
            UintText = ((uint)value).ToString("X4");
            IntText = ((int)value).ToString("X4");
            UlongText = value.ToString("X8");
            LongText = ((long)value).ToString("X8");

            textBuilder.Clear();
            byte bits8 = (byte)value;
            for (int i = 7; i >= 0; i--) {
                textBuilder.Append((bits8 & (1 << i)) == 0 ? "0" : "1");
            }

            BitsText = textBuilder.ToString();

            FloatText = BitConverter.ToSingle(buffer, 0).ToString(CultureInfo.CurrentCulture);
            DoubleText = BitConverter.ToDouble(buffer, 0).ToString(CultureInfo.CurrentCulture);
            Utf8Text = Encoding.UTF8.GetString(buffer);
            Utf16Text = Encoding.Unicode.GetString(buffer);
            Utf32Text = Encoding.UTF32.GetString(buffer);
            ShiftJisText = Encoding.GetEncoding(932).GetString(buffer);
        }
    }
}
