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
using Eto.Drawing;
using Eto.Forms;

namespace SceneGate.UI.Formats
{
    /// <summary>
    /// View for any .NET object.
    /// </summary>
    public class ObjectView : BaseFormatView
    {
        private readonly ObjectViewModel viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectView" /> class.
        /// </summary>
        public ObjectView()
        {
            viewModel = new ObjectViewModel();
            DataContext = viewModel;
            InitializeComponents();
        }

        /// <inheritdoc/>
        public override IFormatViewModel ViewModel => viewModel;

        private void InitializeComponents()
        {
            var yamlButton = new RadioButton {
                Text = "Show as YAML",
            };
            yamlButton.BindDataContext(r => r.Checked, (ObjectViewModel vm) => vm.ShowYaml);

            var propertyButton = new RadioButton(yamlButton) {
                Text = "Show as property grid",
            };
            propertyButton.BindDataContext(r => r.Checked, (ObjectViewModel vm) => vm.ShowPropertyGrid);

            var buttonStack = new StackLayout(yamlButton, propertyButton) {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Padding = new Padding(5),
            };

            var textView = new TextArea {
                ReadOnly = true,
                Font = Fonts.Monospace(10),
                SpellCheck = false,
                Wrap = false,
            };
            textView.TextBinding.BindDataContext((ObjectViewModel vm) => vm.Yaml);

            var propertyView = new PropertyGrid {
                ShowCategories = false,
                ShowDescription = false,
            };
            propertyView.BindDataContext(v => v.SelectedObject, (ObjectViewModel vm) => vm.Format);

            var contentPanel = new Panel();
            contentPanel.BindDataContext(
                p => p.Content,
                Binding.Property((ObjectViewModel vm) => vm.ShowYaml).Convert<Control>(v => v ? textView : propertyView));

            var mainStack = new StackLayout(buttonStack, new StackLayoutItem(contentPanel, true)) {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            };

            Content = mainStack;
        }
    }
}
