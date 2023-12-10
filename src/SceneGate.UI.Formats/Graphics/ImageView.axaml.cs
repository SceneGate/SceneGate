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

    private void ZoomLabelDoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        ZoomImageBorder.ResetMatrix();
    }

    private void ZoomInClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ZoomImageBorder.ZoomIn();
    }

    private void ZoomOutClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ZoomImageBorder.ZoomOut();
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
