namespace SceneGate.UI.Pages.Main;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using SceneGate.UI.ControlsData;

public partial class AnalyzeView : UserControl
{
    private readonly AnalyzeViewModel viewModel;

    public AnalyzeView()
    {
        InitializeComponent();

        viewModel = new AnalyzeViewModel();
        DataContext = viewModel;

        viewModel.AskUserForInputFile.RegisterHandler(SelectInputFiles);
        viewModel.AskUserForInputFolder.RegisterHandler(SelectInputFolder);
        viewModel.DisplayConversionError.RegisterHandler(DisplayConversionError);
        viewModel.AskUserForFileSave.RegisterHandler(SelectOutputFile);
    }

    private void NodeTreeViewDoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        viewModel.OpenNodeViewCommand.Execute(null);
    }

    private void TabViewTabCloseRequested(TabView? sender, TabViewTabCloseRequestedEventArgs args)
    {
        viewModel.CloseNodeViewCommand.Execute(args.Item as NodeFormatTab);
    }

    private void ConvertersTreeViewDoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        viewModel.ConvertNodeCommand.Execute(null);
    }

    private async Task<IEnumerable<IStorageFile>> SelectInputFiles()
    {
        var options = new FilePickerOpenOptions {
            AllowMultiple = true,
            Title = "Select files to analyze",
            FileTypeFilter = new FilePickerFileType[] { FilePickerFileTypes.All },
        };

        return await TopLevel.GetTopLevel(this)!
            .StorageProvider
            .OpenFilePickerAsync(options)
            .ConfigureAwait(false);
    }

    private async Task<IStorageFolder?> SelectInputFolder()
    {
        var options = new FolderPickerOpenOptions {
            AllowMultiple = false,
            Title = "Select the folder with files to analyze",
        };

        var results = await TopLevel.GetTopLevel(this)!
            .StorageProvider
            .OpenFolderPickerAsync(options)
            .ConfigureAwait(false);
        return results.FirstOrDefault();
    }

    private async Task<object> DisplayConversionError(string message)
    {
        var dialog = new ContentDialog() {
            Title = "Error converting format",
            Content = message,
            IsPrimaryButtonEnabled = false,
            IsSecondaryButtonEnabled = false,
            SecondaryButtonText = string.Empty,
        };

        _ = await dialog.ShowAsync().ConfigureAwait(false);
        return null!;
    }

    private async Task<IStorageFile?> SelectOutputFile(string name)
    {
        var options = new FilePickerSaveOptions {
            Title = "Select where to save the file",
            SuggestedFileName = name,
        };

        return await TopLevel.GetTopLevel(this)!
            .StorageProvider
            .SaveFilePickerAsync(options)
            .ConfigureAwait(false);
    }
}
