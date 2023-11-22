namespace SceneGate.UI.Pages.Main;

using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

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
        if (e.SelectedItem is not NavigationViewItem nvi) {
            return;
        }

        string viewTypeName = typeof(App).Namespace + nvi.Tag;
        Type viewType = Type.GetType(viewTypeName)
            ?? throw new InvalidOperationException($"Cannot find view Type: {viewTypeName}");

        _ = mainNavigationFrame.Navigate(viewType);
    }
}
