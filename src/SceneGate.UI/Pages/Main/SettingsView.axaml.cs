namespace SceneGate.UI.Pages.Main;

using Avalonia.Controls;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();
    }
}
