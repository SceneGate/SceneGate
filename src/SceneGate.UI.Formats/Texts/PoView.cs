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
using Eto.Forms;
using Yarhl.Media.Text;

namespace SceneGate.UI.Formats.Texts
{
    /// <summary>
    /// View for the <see cref="Yarhl.Media.Text.Po" /> format.
    /// </summary>
    public class PoView : BaseFormatView
    {
        private readonly PoViewModel viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="PoView" /> class.
        /// </summary>
        public PoView()
        {
            viewModel = new PoViewModel();
            DataContext = viewModel;
            InitializeComponents();
        }

        /// <inheritdoc/>
        public override IFormatViewModel ViewModel => viewModel;

        private static void AddLabelBox(
            DynamicLayout layout,
            string text,
            Expression<Func<PoViewModel, string>> binding)
        {
            var label = new Label {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var box = new TextBox {
                ReadOnly = true,
            };
            box.TextBinding.BindDataContext<PoViewModel>(binding);

            layout.AddRow(label, box);
        }

        private void InitializeComponents()
        {
            var list = new ListBox();
            list.DataStore = viewModel.Entries;
            list.ItemTextBinding = Binding.Property((PoEntry entry) => $"[{entry.Context}]\n[{entry.ExtractedComments}]\n{entry.Text}");

            var headerView = CreateHeaderView();

            var mainView = new Splitter {
                Orientation = Orientation.Horizontal,
                Panel1 = list,
                Panel2 = headerView,
                FixedPanel = SplitterFixedPanel.Panel2,
            };

            Content = mainView;
        }

        private Control CreateHeaderView()
        {
            var grid = new GridView {
                ShowHeader = true,
                GridLines = GridLines.Horizontal,
            };
            grid.Columns.Add(new GridColumn {
                DataCell = new TextBoxCell { Binding = Binding.Property((PoViewModel.HeaderProperty p) => p.Key) },
                HeaderText = "Name",
                Editable = false,
                AutoSize = true,
                Resizable = false,
            });
            grid.Columns.Add(new GridColumn {
                DataCell = new TextBoxCell { Binding = Binding.Property((PoViewModel.HeaderProperty p) => p.Value) },
                HeaderText = "Value",
                Editable = false,
                AutoSize = false,
                Resizable = true,
                Width = 150,
            });

            grid.DataStore = viewModel.Header;

            return grid;
        }
    }
}
