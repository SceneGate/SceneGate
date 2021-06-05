
namespace SceneGate.UI.Views
{
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

            CreateMenu();
            CreateContent();
        }

        void CreateMenu()
        {
            var quitCommand = new Command
            {
                MenuText = "Quit",
                Shortcut = Application.Instance.CommonModifier | Keys.Q,
            };
            quitCommand.Executed += (sender, e) => Application.Instance.Quit();

            var aboutCommand = new Command
            {
                MenuText = "About..."
            };
            aboutCommand.Executed += (sender, e) => new AboutDialog().ShowDialog(this);

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
                    new ButtonMenuItem { Text = "&Preferences..." },
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
            contentPanel = new Scrollable
            {
                BackgroundColor = Colors.White,
                Border = BorderType.Line
            };

            var exploreView = new ExploreView();
            analyzeView = new AnalyzeView();
            var automatizeView = new AutomatizeView();

            var exploreButton = new ToggleButton {
                Image = Icon.FromResource("SceneGate.UI.Resources.Shutter.png").WithSize(32, 32),
                //Visible = false,
            };
            var analyzeButton = new ToggleButton {
                Image = Icon.FromResource("SceneGate.UI.Resources.Script.png").WithSize(32, 32),
            };
            var automatizeButton = new ToggleButton {
                Image = Icon.FromResource("SceneGate.UI.Resources.Carpool.png").WithSize(32, 32),
                //Visible = false,
            };
            var settingsButton = new ToggleButton {
                Image = Icon.FromResource("SceneGate.UI.Resources.Gears.png").WithSize(32, 32),
            };

            exploreButton.Click += (sender, e) =>
            {
                exploreButton.Checked = true;
                analyzeButton.Checked = false;
                automatizeButton.Checked = false;
                settingsButton.Checked = false;
                contentPanel.Content = exploreView;
            };

            analyzeButton.Click += (sender, e) =>
            {
                exploreButton.Checked = false;
                analyzeButton.Checked = true;
                automatizeButton.Checked = false;
                settingsButton.Checked = false;
                contentPanel.Content = analyzeView;
            };
            automatizeButton.Click += (sender, e) =>
            {
                exploreButton.Checked = false;
                analyzeButton.Checked = false;
                automatizeButton.Checked = true;
                settingsButton.Checked = false;
                contentPanel.Content = automatizeView;
            };
            settingsButton.Click += (sender, e) =>
            {
                exploreButton.Checked = false;
                analyzeButton.Checked = false;
                automatizeButton.Checked = false;
                settingsButton.Checked = true;
                contentPanel.Content = new TextArea { Text = "Not implemented" };
            };

            analyzeButton.PerformClick();

            var viewModeBar = new StackLayout
            {
                Orientation = Orientation.Vertical,
                BackgroundColor = Colors.Gray,
            };
            viewModeBar.Items.Add(exploreButton);
            viewModeBar.Items.Add(analyzeButton);
            viewModeBar.Items.Add(automatizeButton);
            viewModeBar.Items.Add(new StackLayoutItem(null, true));
            viewModeBar.Items.Add(new StackLayoutItem(settingsButton, VerticalAlignment.Bottom));

            var mainLayout = new DynamicLayout();
            mainLayout.BackgroundColor = Colors.White;

            mainLayout.BeginVertical(yscale: true, xscale: true);
            mainLayout.AddRow(viewModeBar, contentPanel);
            mainLayout.EndVertical();

            Content = mainLayout;
        }
    }
}
