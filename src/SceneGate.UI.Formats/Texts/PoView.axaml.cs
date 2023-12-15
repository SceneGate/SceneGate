namespace SceneGate.UI.Formats.Texts;

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

/// <summary>
/// View for PO models.
/// </summary>
public partial class PoView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PoView"/> class.
    /// </summary>
    public PoView()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is null) {
            return;
        }

        var viewModel = (DataContext as PoViewModel)!;
        viewModel.AskOutputFile.RegisterHandler(AskOutputFileAsync);
    }

    private async Task<IStorageFile?> AskOutputFileAsync()
    {
        var options = new FilePickerSaveOptions {
            Title = "Select where to save the file",
            ShowOverwritePrompt = true,
            FileTypeChoices = new[] {
                new FilePickerFileType("GNU gettext PO") {
                    Patterns = ["*.po", "*.pot"],
                    MimeTypes = [ "application/x-gettext" ]
                },
                FilePickerFileTypes.All,
            },
        };

        return await TopLevel.GetTopLevel(this)!
            .StorageProvider
            .SaveFilePickerAsync(options)
            .ConfigureAwait(false);
    }
}
