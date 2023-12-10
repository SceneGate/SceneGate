namespace SceneGate.UI.Formats.Graphics;

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

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

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (ViewModel is null) {
            return;
        }

        ViewModel.AskOutputFile.RegisterHandler(AskOutputFileAsync);
    }

    private void ZoomingPointerWheelChanged(object? sender, Avalonia.Input.PointerWheelEventArgs e)
    {
        if (e.Delta.X != 0) {
            return;
        }

        // Only with Control pressed
        if (!e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Control)) {
            return;
        }

        if (e.Delta.Y > 0 && (ViewModel?.CanZoomIn() ?? false)) {
            ViewModel.ZoomIn();

            e.PreventGestureRecognition();
        }

        if (e.Delta.Y < 0 && (ViewModel?.CanZoomOut() ?? false)) {
            ViewModel.ZoomOut();

            e.PreventGestureRecognition();
        }
    }

    private void ZoomLabelDoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (ViewModel is not null) {
            ViewModel.Zoom = 100;
        }
    }

    private async Task<IStorageFile?> AskOutputFileAsync()
    {
        var options = new FilePickerSaveOptions {
            Title = "Select where to save the file",
            ShowOverwritePrompt = true,
            FileTypeChoices = new[] {
                FilePickerFileTypes.ImagePng,
            },
        };

        return await TopLevel.GetTopLevel(this)!
            .StorageProvider
            .SaveFilePickerAsync(options)
            .ConfigureAwait(false);
    }
}
