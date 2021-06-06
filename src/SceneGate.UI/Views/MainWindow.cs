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
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using SceneGate.UI.Resources;
using SceneGate.UI.ViewModels;

namespace SceneGate.UI.Views
{
    public sealed class MainWindow : Form
    {
        private readonly MainViewModel viewModel;
        private AnalyzeView analyzeView;
        private Panel settingsView;

        public MainWindow()
        {
            viewModel = new MainViewModel();
            DataContext = viewModel;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Title = $"SceneGate ~~ {version}";
            Icon = Icon.FromResource(ResourcesName.Icon);
            ClientSize = new Size(800, 600);

            Menu = CreateMenuBar();
            Content = CreateContent();
        }

        private MenuBar CreateMenuBar()
        {
            return new MenuBar {
                Items = {
                    new ButtonMenuItem {
                        Text = L10n.Get("&File"),
                    },
                },
                ApplicationItems = {
                    new Command {
                        MenuText = L10n.Get("Toggle action panel"),
                        Shortcut = Application.Instance.CommonModifier | Keys.T,
                        DelegatedCommand = viewModel.ToggleActionPanelCommand,
                    },
                    new Command {
                        MenuText = L10n.Get("&Settings"),
                        Shortcut = Application.Instance.CommonModifier | Keys.Comma,
                        DelegatedCommand = viewModel.OpenSettingsCommand,
                    },
                },
                QuitItem = new Command {
                    MenuText = L10n.Get("&Quit"),
                    Shortcut = Application.Instance.CommonModifier | Keys.Q,
                    DelegatedCommand = viewModel.QuitCommand,
                },
                AboutItem = new Command {
                    MenuText = L10n.Get("About..."),
                    DelegatedCommand = viewModel.AboutCommand,
                },
            };
        }

        private Control CreateContent()
        {
            analyzeView = new AnalyzeView();
            settingsView = new Panel();

            var viewModeBar = CreateViewModeBar();
            var contentPanel = new Scrollable {
                Border = BorderType.Line,
            };
            contentPanel.BindDataContext(
                s => s.Content,
                Binding.Property((MainViewModel vm) => vm.ViewKind).Convert(GetView));

            var mainLayout = new DynamicLayout();
            mainLayout.BeginVertical(yscale: true, xscale: true);
            mainLayout.AddRow(viewModeBar, contentPanel);
            mainLayout.EndVertical();

            return mainLayout;
        }

        private Control CreateViewModeBar()
        {
            var analyzeButton = new ToggleButton {
                Text = "\uE50A",
                Font = new Font("Material Icons", 24),
                ToolTip = L10n.Get("Analyze"),
                Command = viewModel.OpenAnalyzeCommand,
            };
            analyzeButton.Bind(
                b => b.Checked,
                Binding.Property(viewModel, vm => vm.ViewKind).Convert(k => k == ViewKind.Analyze));

            var settingsButton = new ToggleButton {
                Text = "\uE8B8",
                Font = new Font("Material Icons", 24),
                ToolTip = L10n.Get("Settings"),
                Command = viewModel.OpenSettingsCommand,
            };
            settingsButton.Bind(
                b => b.Checked,
                Binding.Property(viewModel, vm => vm.ViewKind).Convert(k => k == ViewKind.Settings));

            var viewModeBar = new StackLayout {
                Orientation = Orientation.Vertical,
            };
            viewModeBar.Items.Add(analyzeButton);
            viewModeBar.Items.Add(new StackLayoutItem(null, true));
            viewModeBar.Items.Add(new StackLayoutItem(settingsButton, VerticalAlignment.Bottom));

            return viewModeBar;
        }

        private Control GetView(ViewKind kind) =>
            kind switch {
                ViewKind.Analyze => analyzeView,
                ViewKind.Settings => settingsView,
                _ => null,
            };
    }
}
