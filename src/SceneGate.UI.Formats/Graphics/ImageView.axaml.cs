namespace SceneGate.UI.Formats.Graphics;

using Avalonia.Controls;

/// <summary>
/// View for Yarhl images.
/// </summary>
public partial class ImageView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageView"/> class.
    /// </summary>
    public ImageView()
    {
        InitializeComponent();
    }

    private ImageViewModel? ViewModel => DataContext as ImageViewModel;

    private void ScrollViewer_PointerWheelChanged(object? sender, Avalonia.Input.PointerWheelEventArgs e)
    {
        if (e.Delta.X != 0) {
            return;
        }

        if (e.Delta.Y > 0 && (ViewModel?.CanZoomIn() ?? false)) {
            ViewModel.ZoomIn();
        }

        if (e.Delta.Y < 0 && (ViewModel?.CanZoomOut() ?? false)) {
            ViewModel.ZoomOut();
        }
    }
}
