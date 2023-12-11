namespace SceneGate.UI;

using System.Diagnostics;
using System.Reflection;
using FluentAvalonia.UI.Windowing;
using SceneGate.UI.Pages.Main;

public partial class MainWindow : AppWindow
{
    private const string DevVersion = "0.0.0-dev";

    public MainWindow()
    {
        InitializeComponent();

        // Get assembly with the build metadata (except commit number)
        string assemblyPath = Assembly.GetExecutingAssembly().Location;
        string version = FileVersionInfo.GetVersionInfo(assemblyPath).ProductVersion
            ?? DevVersion;
        if (version.Contains('+')) {
            version = version[..version.IndexOf('+')];
        }

        string versionText = (version == DevVersion) ? "DEVELOPMENT BUILD" : $"v{version}";
        Title = $"SceneGate ~~ {versionText}";

        SplashScreen = new SplashScreen();
    }
}
