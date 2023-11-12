using System;
using System.Reflection;
using FluentAvalonia.UI.Windowing;

namespace SceneGate.UI;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        InitializeComponent();

        Version version = Assembly.GetExecutingAssembly().GetName().Version!;
        string versionText = (version.Build == 0) ? "DEVELOPMENT BUILD" : $"v{version}";
        Title = $"SceneGate ~~ {versionText}";
    }
}
