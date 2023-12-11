namespace SceneGate.UI.Pages.Main;

using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.UI.Windowing;
using Yarhl.Plugins;

internal class SplashScreen : IApplicationSplashScreen
{
#pragma warning disable S1075
    private const string LogoResourcePath = "avares://SceneGate.UI/Assets/logo-128.png";
#pragma warning restore S1075

    public string AppName => "SceneGate";

    public IImage AppIcon => new Bitmap(AssetLoader.Open(new Uri(LogoResourcePath)));

    public object SplashScreenContent => new SplashScreenControl();

    public int MinimumShowTime => 3_000;

    public Task RunTasks(CancellationToken cancellationToken)
    {
        TypeLocator.Default.LoadContext.TryLoadFromBaseLoadDirectory();
        return Task.CompletedTask;
    }
}
