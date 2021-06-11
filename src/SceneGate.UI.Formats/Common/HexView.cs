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
using System.Linq.Expressions;
using Eto.Drawing;
using Eto.Forms;

namespace SceneGate.UI.Formats.Common
{
    /// <summary>
    /// Binary hexadecimal view.
    /// </summary>
    public class HexView : BaseFormatView
    {
        private readonly HexViewModel viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="HexView" /> class.
        /// </summary>
        public HexView()
        {
            viewModel = new HexViewModel();
            DataContext = viewModel;
            InitializeComponents();
        }

        /// <inheritdoc/>
        public override IFormatViewModel ViewModel => viewModel;

        private static void AddLabelBox(
            DynamicLayout layout,
            string text,
            Expression<Func<HexViewModel, string>> binding)
        {
            var label = new Label {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                Font = Fonts.Sans(10),
            };

            var box = new TextBox {
                ReadOnly = true,
                Font = Fonts.Monospace(10),
            };
            box.TextBinding.BindDataContext<HexViewModel>(binding);

            layout.AddRow(label, box);
        }

        private void InitializeComponents()
        {
            var font = Fonts.Monospace(12);
            int lineHeight = (int)font.MeasureString("0").Height;
            int textPadding = 10;

            var offsetView = new TextArea {
                Font = font,
                Width = (int)font.MeasureString(new string('0', 4 * 2)).Width + textPadding,
                ReadOnly = true,
                Enabled = false,
            };
            offsetView.TextBinding.BindDataContext<HexViewModel>(vm => vm.OffsetsText);

            var hexView = new TextArea {
                Font = font,
                Width = (int)font.MeasureString(new string('0', viewModel.BytesPerRow * 3)).Width + textPadding,
                ReadOnly = true,
            };
            hexView.TextBinding.BindDataContext<HexViewModel>(vm => vm.HexText);
            hexView.BindDataContext(t => t.CaretIndex, (HexViewModel vm) => vm.HexCursorPos);

            var asciiView = new TextArea {
                Font = font,
                Width = (int)font.MeasureString(new string('0', viewModel.BytesPerRow * 2)).Width + textPadding,
                ReadOnly = true,
            };
            asciiView.TextBinding.BindDataContext<HexViewModel>(vm => vm.AsciiText);
            asciiView.BindDataContext(t => t.CaretIndex, (HexViewModel vm) => vm.AsciiCursorPos);

            var scrolls = new Slider {
                MinValue = 0,
                Orientation = Orientation.Vertical,
            };
            scrolls.BindDataContext(s => s.MaxValue, (HexViewModel vm) => vm.MaximumScroll);
            scrolls.BindDataContext(s => s.Value, (HexViewModel vm) => vm.CurrentScroll);
            hexView.MouseWheel += (sender, e) => scrolls.Value += e.Delta.Height > 0 ? -1 : 1;
            asciiView.MouseWheel += (sender, e) => scrolls.Value += e.Delta.Height > 0 ? -1 : 1;

            var typesView = CreateDataTypeInspectorView();

            var mainStack = new StackLayout(offsetView, hexView, asciiView, scrolls, new StackLayoutItem(typesView, true)) {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Spacing = 5,
            };
            mainStack.SizeChanged += (sender, e) =>
                viewModel.VisibleTextRows = mainStack.Size.Height / lineHeight;

            Content = mainStack;
        }

        private Control CreateDataTypeInspectorView()
        {
            var layout = new DynamicLayout {
                Spacing = new Size(5, 5),
                Padding = new Padding(10),
            };

            AddLabelBox(layout, "8 bits: ", vm => vm.BitsText);
            AddLabelBox(layout, "unsigned 16 bits (ushort): ", vm => vm.UshortText);
            AddLabelBox(layout, "signed 16 bits (short): ", vm => vm.ShortText);
            AddLabelBox(layout, "unsigned 32 bits (uint): ", vm => vm.UintText);
            AddLabelBox(layout, "signed 32 bits (int): ", vm => vm.IntText);
            AddLabelBox(layout, "unsigned 64 bits (ulong): ", vm => vm.UlongText);
            AddLabelBox(layout, "signed 64 bits (long): ", vm => vm.LongText);
            AddLabelBox(layout, "float 32 bits (single): ", vm => vm.FloatText);
            AddLabelBox(layout, "double 64 bits: ", vm => vm.DoubleText);
            AddLabelBox(layout, "UTF-8: ", vm => vm.Utf8Text);
            AddLabelBox(layout, "UTF-16: ", vm => vm.Utf16Text);
            AddLabelBox(layout, "UTF-32: ", vm => vm.Utf32Text);
            AddLabelBox(layout, "Shift-JIS: ", vm => vm.ShiftJisText);

            layout.Add(null);

            return layout;
        }
    }
}
