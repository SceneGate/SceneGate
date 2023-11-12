using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

namespace SceneGate.UI.Pages.Main;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        mainNavigationView.SelectionChanged += OnMainNavigationItemChange;
        mainNavigationView.SelectedItem = mainNavigationView.MenuItems[0];
    }

    private void OnMainNavigationItemChange(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.IsSettingsSelected) {
            _ = mainNavigationFrame.Navigate(typeof(SettingsView));
        } else if (e.SelectedItem is NavigationViewItem nvi) {
            string viewTypeName = typeof(App).Namespace + ".Pages." + nvi.Tag;
            Type viewType = Type.GetType(viewTypeName)
                ?? throw new InvalidOperationException($"Cannot find view Type: {viewTypeName}");

            _ = mainNavigationFrame.Navigate(viewType);
        }
    }
}
