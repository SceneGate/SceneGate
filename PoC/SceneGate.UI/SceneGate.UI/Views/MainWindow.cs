
namespace SceneGate.UI.Views
{
    using System;
    using System.Reflection;
    using Eto.Forms;
    using Eto.Drawing;

    public class MainWindow : Form
    {
        Panel contentPanel;

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

            Menu = new MenuBar
            {
                Items = {
                    new ButtonMenuItem { Text = "&File" },
                },
                ApplicationItems = {
                    new ButtonMenuItem { Text = "&Preferences..." },
                },
                QuitItem = quitCommand,
                AboutItem = aboutCommand
            };
        }

        void CreateContent()
        {
            contentPanel = new Scrollable
            {
                BackgroundColor = Colors.White,
                Border = BorderType.Line
            };

            var exploreView = new ExploreView();
            var analyzeView = new AnalyzeView();
            var automatizeView = new AutomatizeView();

            var exploreButton = new LinkButton
            {
                TextColor = Colors.Gray,
                Text = "Explore",
                Font = Fonts.Fantasy(20, FontStyle.None, FontDecoration.None)
            };

            var analyzeButton = new LinkButton
            {
                TextColor = Colors.Gray,
                Text = "Analyze",
                Font = Fonts.Cursive(20, FontStyle.None, FontDecoration.None)
            };


            var automatizeButton = new LinkButton
            {
                TextColor = Colors.Gray,
                Text = "Automatize",
                Font = Fonts.Monospace(20, FontStyle.None, FontDecoration.None)
            };

            exploreButton.Click += (sender, e) =>
            {
                exploreButton.TextColor = Colors.Blue;
                analyzeButton.TextColor = Colors.Gray;
                automatizeButton.TextColor = Colors.Gray;
                contentPanel.Content = exploreView;
            };
            analyzeButton.Click += (sender, e) =>
            {
                exploreButton.TextColor = Colors.Gray;
                analyzeButton.TextColor = Colors.Blue;
                automatizeButton.TextColor = Colors.Gray;
                contentPanel.Content = analyzeView;
            };
            automatizeButton.Click += (sender, e) =>
            {
                exploreButton.TextColor = Colors.Gray;
                analyzeButton.TextColor = Colors.Gray;
                automatizeButton.TextColor = Colors.Blue;
                contentPanel.Content = automatizeView;
            };

            var mainLayout = new DynamicLayout();
            mainLayout.BackgroundColor = Colors.White;

            mainLayout.BeginVertical(padding: new Padding(10, 10), xscale: true);
            mainLayout.AddRow(null, exploreButton, null, analyzeButton, null, automatizeButton, null);
            mainLayout.EndVertical();

            mainLayout.BeginVertical();
            mainLayout.AddRow(contentPanel);
            mainLayout.EndVertical();

            Content = mainLayout;
        }
    }
}
