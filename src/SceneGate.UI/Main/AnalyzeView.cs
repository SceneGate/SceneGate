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
using System.Windows.Input;
using Eto.Drawing;
using Eto.Forms;
using SceneGate.UI.Resources;
using Yarhl.FileFormat;

namespace SceneGate.UI.Main
{
    public sealed class AnalyzeView : Panel
    {
        public AnalyzeView()
        {
            ViewModel = new AnalyzeViewModel();
            DataContext = ViewModel;

            InitializeComponents();
        }

        public AnalyzeViewModel ViewModel { get; }

        private void InitializeComponents()
        {
            var contentPanel = new Panel();

            var actionPanel = new TabControl();
            actionPanel.Bind(p => p.Visible, Binding.Property(ViewModel, vm => vm.IsActionPanelVisible));
            actionPanel.Pages.Add(CreateConverterTab());

            var contentLayout = new Splitter {
                Orientation = Orientation.Vertical,
                FixedPanel = SplitterFixedPanel.Panel2,
                Panel1 = contentPanel,
                Panel2 = actionPanel,
            };

            var splitter = new Splitter {
                Orientation = Orientation.Horizontal,
                FixedPanel = SplitterFixedPanel.Panel1,
                Panel1MinimumSize = 200,
                Panel1 = CreateTreeBar(),
                Panel2 = contentLayout,
            };

            Content = splitter;
        }

        private TabPage CreateConverterTab()
        {
            var filterBox = new TextBox {
                PlaceholderText = L10n.Get("Converter name filter..."),
            };
            filterBox.TextBinding.BindDataContext((AnalyzeViewModel vm) => vm.ConverterFilter);

            var list = new ListBox();
            list.DataStore = ViewModel.CompatibleConverters;
            list.ItemKeyBinding = Binding.Property((ConverterMetadata meta) => meta.Name);
            list.ItemTextBinding = Binding.Property((ConverterMetadata meta) => meta.Name);
            list.SelectedIndexBinding.BindDataContext((AnalyzeViewModel vm) => vm.SelectedConverterIndex);
            list.MouseDoubleClick += (sender, e) => Binding.ExecuteCommand(
                list.DataContext,
                Binding.Property((AnalyzeViewModel vm) => (ICommand)vm.ConvertNodeCommand));

            var stack = new StackLayout(filterBox, new StackLayoutItem(list, true)) {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            };
            return new TabPage(stack) {
                Text = L10n.Get("Converters"),
            };
        }

        private Panel CreateTreeBar()
        {
            // Buttons over the treeview
            var addFileBtn = new Button {
                Text = "\ue89c",
                Font = new Font("Material Icons", 11),
                ToolTip = L10n.Get("Add external files"),
                Width = 32,
                Height = 32,
                Command = ViewModel.AddFileCommand,
            };
            var addFolderButton = new Button {
                Text = "\ue2cc",
                Font = new Font("Material Icons", 11),
                ToolTip = L10n.Get("Add external folders"),
                Width = 32,
                Height = 32,
                Command = ViewModel.AddFolderCommand,
            };
            var headerLayout = new StackLayout(null, addFileBtn, addFolderButton) {
                Orientation = Orientation.Horizontal,
                Padding = new Padding(5),
                Spacing = 5,
            };

            // Treeview with just one "column" with icon and name, so the icon is close to the name.
            var tree = new TreeGridView {
                ShowHeader = false,
                Border = BorderType.Line,
            };
            tree.Columns.Add(
                new GridColumn {
                    DataCell = new TextBoxCell { Binding = Binding.Property((TreeGridNode node) => node.QualifiedName) },
                    HeaderText = "name",
                    AutoSize = true,
                    Resizable = false,
                    Editable = false,
                });
            tree.DataStore = ViewModel.RootNode;
            tree.SelectedItemBinding.BindDataContext((AnalyzeViewModel vm) => vm.SelectedNode);

            // Eto doesn't implement the binding fully: https://github.com/picoe/Eto/issues/240
            ViewModel.OnNodeUpdate += (_, node) => {
                if (node is null) {
                    tree.ReloadData();
                } else {
                    tree.ReloadItem(node);
                }
            };

            var saveButton = new ButtonMenuItem {
                Text = L10n.Get("Save to file"),
                Command = ViewModel.SaveNodeCommand,
            };
            var convertButton = new ButtonMenuItem {
                Text = L10n.Get("Convert"),
                Command = ViewModel.ConvertNodeCommand,
            };
            tree.ContextMenu = new ContextMenu(saveButton, convertButton);

            var scrollableTree = new Scrollable { Content = tree };
            var stack = new DynamicLayout();
            stack.BeginHorizontal();
            stack.AddRow(headerLayout);
            stack.AddRow(scrollableTree);
            stack.EndHorizontal();

            return stack;
        }
    }
}
