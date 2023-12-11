namespace SceneGate.UI.Formats.Graphics;

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
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

        ShowPaletteButton.IsChecked = ViewModel.IsSinglePalette; // initial value, then binded
        ViewModel.AskOutputFile.RegisterHandler(AskOutputFileAsync);
        ViewModel.CopyImageToClipboard.RegisterHandler(CopyImageToClipboardAsync);
    }

    private void ZoomLabelDoubleTapped(object? sender, TappedEventArgs e)
    {
        ZoomImageBorder.ResetMatrix();
    }

    private void ZoomInClick(object? sender, RoutedEventArgs e)
    {
        ZoomImageBorder.ZoomIn();
    }

    private void ZoomOutClick(object? sender, RoutedEventArgs e)
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

    private Task<object?> CopyImageToClipboardAsync(Bitmap image)
    {
        throw new NotSupportedException("https://github.com/AvaloniaUI/Avalonia/issues/3588");
    }
}
