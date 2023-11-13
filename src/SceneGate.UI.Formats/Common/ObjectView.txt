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

namespace SceneGate.UI.Formats.Common
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
            var propertyButton = new RadioButton() {
                Text = "Show as property grid",
            };
            propertyButton.BindDataContext(r => r.Checked, (ObjectViewModel vm) => vm.ShowPropertyGrid);

            var yamlButton = new RadioButton(propertyButton) {
                Text = "Show as YAML",
            };
            yamlButton.BindDataContext(r => r.Checked, (ObjectViewModel vm) => vm.ShowYaml);

            var jsonButton = new RadioButton(propertyButton) {
                Text = "Show as JSON",
            };
            jsonButton.BindDataContext(r => r.Checked, (ObjectViewModel vm) => vm.ShowJson);

            var buttonsLayout = new DynamicLayout {
                Spacing = new Size(5, 5),
            };
            buttonsLayout.AddRow(propertyButton, yamlButton, jsonButton);

            var textView = new TextArea {
                ReadOnly = true,
                Font = Fonts.Monospace(10),
                SpellCheck = false,
                Wrap = false,
            };
            textView.TextBinding.BindDataContext((ObjectViewModel vm) => vm.Text);

            var propertyView = new PropertyGrid {
                ShowCategories = false,
                ShowDescription = false,
            };
            propertyView.BindDataContext(v => v.SelectedObject, (ObjectViewModel vm) => vm.Format);

            var contentPanel = new Panel();
            contentPanel.BindDataContext(
                p => p.Content,
                Binding.Property((ObjectViewModel vm) => vm.ShowText).Convert<Control>(v => v ? textView : propertyView));

            var mainLayout = new DynamicLayout();
            mainLayout.AddCentered(buttonsLayout);
            mainLayout.AddRow(contentPanel);
            Content = mainLayout;
        }
    }
}
