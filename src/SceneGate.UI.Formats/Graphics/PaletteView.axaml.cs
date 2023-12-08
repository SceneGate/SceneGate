namespace SceneGate.UI.Formats.Graphics;

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

/// <summary>
/// View of a palette of a collection of them.
/// </summary>
public partial class PaletteView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaletteView"/> class.
    /// </summary>
    public PaletteView()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        var viewModel = (DataContext as PaletteViewModel)!;
        viewModel.AskOutputFile.RegisterHandler(AskOutputFileAsync);
        viewModel.AskOutputFolder.RegisterHandler(AskOutputFolderAsync);
    }

    private async Task<IStorageFile?> AskOutputFileAsync()
    {
        var options = new FilePickerSaveOptions {
            Title = "Select where to save the file",
            ShowOverwritePrompt = true,
            FileTypeChoices = new[] {
                FilePickerFileTypes.ImagePng,
                new FilePickerFileType("RIFF palette for Gimp") { Patterns = [ "*.pal" ] },
            },
        };

        return await TopLevel.GetTopLevel(this)!
            .StorageProvider
            .SaveFilePickerAsync(options)
            .ConfigureAwait(false);
    }

    private async Task<IStorageFolder?> AskOutputFolderAsync()
    {
        var options = new FolderPickerOpenOptions {
            AllowMultiple = false,
            Title = "Select the folder to save all the palettes",
        };

        var results = await TopLevel.GetTopLevel(this)!
            .StorageProvider
            .OpenFolderPickerAsync(options)
            .ConfigureAwait(false);
        return results.Count > 0 ? results[0] : null;
    }
}
