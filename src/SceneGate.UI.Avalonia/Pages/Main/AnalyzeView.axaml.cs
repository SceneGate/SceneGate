using System.Collections.Generic;
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
}
