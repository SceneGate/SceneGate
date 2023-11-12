using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using SceneGate.UI.ControlsData;

namespace SceneGate.UI.Pages.Main;

public partial class AnalyzeView : UserControl
{
    private readonly AnalyzeViewModel viewModel;

    public AnalyzeView()
    {
        InitializeComponent();

        viewModel = new AnalyzeViewModel();
        DataContext = viewModel;

        viewModel.AskUserForFile.RegisterHandler(SelectInputFiles);
        viewModel.AskUserForFolder.RegisterHandler(SelectInputFolder);
    }

    private void NodeTreeViewDoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        viewModel.OpenNodeViewCommand.Execute(null);
    }

    private void TabViewTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        viewModel.CloseNodeViewCommand.Execute(args.Item as NodeFormatTab);
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
}
