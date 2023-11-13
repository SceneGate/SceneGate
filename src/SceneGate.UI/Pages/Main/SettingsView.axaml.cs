using Avalonia.Controls;

namespace SceneGate.UI.Pages.Main;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();
    }
}
