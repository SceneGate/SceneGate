
namespace SceneGate.UI.Views
{
    using System;
    using System.IO;
    using System.Reflection;
    using Eto.Forms;
    using Eto.Drawing;

    public class MainWindow : Form
    {
        Panel contentPanel;
        AnalyzeView analyzeView;

        public MainWindow()
        {
            CreateControls();
        }

        void CreateControls()
        {
            Title = $"SceneGate ~~ {Assembly.GetExecutingAssembly().GetName().Version}";
            ClientSize = new Size(1000, 600);
            Icon = Icon.FromResource("SceneGate.UI.Resources.Icon.png");

            CreateMenu();
            CreateContent();
        }

        void CreateMenu()
        {
            var quitCommand = new Command
            {
                MenuText = "&Quit",
                Shortcut = Application.Instance.CommonModifier | Keys.Q,
            };
            quitCommand.Executed += (sender, e) => Application.Instance.Quit();

            var about = new AboutDialog {
                Logo = Bitmap.FromResource("SceneGate.UI.Resources.Icon.png"),
                WebsiteLabel = "SceneGate website",
                Website = new Uri("https://scenegate.github.io/SceneGate/"),
                Developers = new[] { "SceneGate team and contributors" },
                License = "MIT License",
                ProgramName = "SceneGate",
                ProgramDescription = "Tool for reverse engineering, file format analysis, modding and localization.",
            };
            var aboutCommand = new Command
            {
                MenuText = "About..."
            };
            aboutCommand.Executed += (sender, e) => about.ShowDialog(this);

            var toggleAction = new Command
            {
                MenuText = "Toggle action panel",
                Shortcut = Application.Instance.CommonModifier | Keys.T,
            };
            toggleAction.Executed += (sender, e) => analyzeView.ToggleActionPanel();

            Menu = new MenuBar
            {
                Items = {
                    new ButtonMenuItem { Text = "&File" },
                },
                ApplicationItems = {
                    new ButtonMenuItem { Text = "&Settings" },
                    toggleAction,
                },
                QuitItem = quitCommand,
                AboutItem = aboutCommand
            };
        }

        static Stream GetResource(string name)
        {
            return typeof(MainWindow).Assembly.GetManifestResourceStream("SceneGate.UI.Resources." + name);
        }

        void CreateContent()
        {
            analyzeView = new AnalyzeView();

            contentPanel = new Scrollable
            {
                Border = BorderType.Line
            };

            var analyzeButton = new ToggleButton {
                Text= "\uE50A",
                Font = new Font("Material Icons", 24),
                ToolTip = "Analyze",
            };
            var settingsButton = new ToggleButton {
                Text= "\uE8B8",
                Font = new Font("Material Icons", 24),
                ToolTip = "Settings",
            };

            analyzeButton.Click += (sender, e) => {
                analyzeButton.Checked = true;
                settingsButton.Checked = false;
                contentPanel.Content = analyzeView;
            };
            settingsButton.Click += (sender, e) => {
                analyzeButton.Checked = false;
                settingsButton.Checked = true;
                contentPanel.Content = new Panel();
            };

            var viewModeBar = new StackLayout {
                Orientation = Orientation.Vertical,
            };
            viewModeBar.Items.Add(analyzeButton);
            viewModeBar.Items.Add(new StackLayoutItem(null, true));
            viewModeBar.Items.Add(new StackLayoutItem(settingsButton, VerticalAlignment.Bottom));

            var mainLayout = new DynamicLayout();

            mainLayout.BeginVertical(yscale: true, xscale: true);
            mainLayout.AddRow(viewModeBar, contentPanel);
            mainLayout.EndVertical();

            Content = mainLayout;

            analyzeButton.Checked = true;
            contentPanel.Content = analyzeView;
        }
    }
}
