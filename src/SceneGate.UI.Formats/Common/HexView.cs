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
using System.Linq;
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

        private void InitializeComponents()
        {
            var font = Fonts.Monospace(10);
            int lineHeight = (int)font.MeasureString("0").Height;
            int textPadding = 5;

            var columnPositions = new Label {
                Font = font,
                Text = string.Join(' ', Enumerable.Range(0, viewModel.BytesPerRow).Select(i => $"{i:X2}")),
            };

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

            var mainLayout = new DynamicLayout {
                Spacing = new Size(5, 5),
            };
            mainLayout.AddRow(null, columnPositions);
            mainLayout.BeginHorizontal(true);
            mainLayout.Add(offsetView, false);
            mainLayout.AddRange(hexView, asciiView, scrolls);
            mainLayout.Add(typesView, true);
            mainLayout.EndHorizontal();

            mainLayout.SizeChanged += (sender, e) =>
                viewModel.VisibleTextRows = hexView.Height / lineHeight;

            Content = mainLayout;
        }

        private Control CreateDataTypeInspectorView()
        {
            var grid = new GridView {
                ShowHeader = true,
                GridLines = GridLines.Horizontal,
            };
            grid.Columns.Add(new GridColumn {
                DataCell = new TextBoxCell {
                    Binding = Binding.Property((HexViewModel.DataTypeItem i) => i.Description),
                },
                HeaderText = "Type",
                Editable = false,
                AutoSize = true,
                Resizable = false,
            });
            grid.Columns.Add(new GridColumn {
                DataCell = new TextBoxCell {
                    Binding = Binding.Property((HexViewModel.DataTypeItem i) => i.Value),
                },
                HeaderText = "Value",
                Editable = false,
                AutoSize = true,
                Resizable = true,
            });

            grid.DataStore = viewModel.DataTypes;

            // GridView doesn't implement MVVM binding.
            // https://github.com/picoe/Eto/issues/530
            viewModel.OnDataTypesUpdate += (_, _) =>
                grid.ReloadData(Eto.Forms.Range.FromLength(0, viewModel.DataTypes.Count));

            var littleEndian = new RadioButton {
                Text = "Little endian",
                Checked = true,
            };
            var bigEndian = new RadioButton(littleEndian) {
                Text = "Big endian",
            };
            bigEndian.BindDataContext(r => r.Checked, (HexViewModel vm) => vm.IsBigEndian);
            var endianLayout = new StackLayout(littleEndian, bigEndian) {
                Spacing = 5,
                Orientation = Orientation.Horizontal,
            };

            var encodingNameLabel = new Label {
                Text = "Custom encoding name:",
                VerticalAlignment = VerticalAlignment.Center,
            };
            var encodingNameBox = new TextBox {
                PlaceholderText = "code page name",
            };
            encodingNameBox.TextBinding.BindDataContext((HexViewModel vm) => vm.CustomEncodingName);
            var encodingLayout = new StackLayout(encodingNameLabel, new StackLayoutItem(encodingNameBox, true)) {
                Spacing = 5,
                Orientation = Orientation.Horizontal,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Center,
            };

            var stackLayout = new StackLayout(grid, endianLayout, encodingLayout) {
                Spacing = 15,
                Orientation = Orientation.Vertical,
            };

            return stackLayout;
        }
    }
}
