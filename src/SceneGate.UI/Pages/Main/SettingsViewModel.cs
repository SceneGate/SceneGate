namespace SceneGate.UI.Pages.Main;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.Styling;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly FluentAvaloniaTheme themeManager;

    [ObservableProperty]
    private string applicationVersion;

    [ObservableProperty]
    private ApplicationThemeKind currentTheme;

    [ObservableProperty]
    private string license;

    public SettingsViewModel()
    {
        AvailableThemes = Enum.GetValues<ApplicationThemeKind>();
        currentTheme = ApplicationThemeKind.System;
        themeManager = Application.Current?.Styles.OfType<FluentAvaloniaTheme>().FirstOrDefault()
            ?? throw new InvalidOperationException("Cannot get theme manager");

        Version version = Assembly.GetExecutingAssembly().GetName().Version!;
        ApplicationVersion = (version.Build == 0) ? "DEVELOPMENT BUILD" : $"v{version}";

        string appUri = "avares://" + typeof(App).Namespace;
        using Stream licenseStream = AssetLoader.Open(new Uri(appUri + "/Assets/LICENSE"));
        using var licenseStreamReader = new StreamReader(licenseStream);
        License = licenseStreamReader.ReadToEnd();
    }

    public ApplicationThemeKind[] AvailableThemes { get; }

    partial void OnCurrentThemeChanged(ApplicationThemeKind value)
    {
        switch (value) {
            case ApplicationThemeKind.System:
                themeManager.PreferSystemTheme = true;
                break;

            case ApplicationThemeKind.Light:
                themeManager.PreferSystemTheme = false;
                Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
                break;

            case ApplicationThemeKind.Dark:
                themeManager.PreferSystemTheme = false;
                Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
                break;
        }
    }
}
