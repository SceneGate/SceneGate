namespace SceneGate.UI.Pages.Main;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using SceneGate.UI.ControlsData;
using Yarhl.FileSystem;

public partial class AnalyzeView : UserControl
{
    private readonly AnalyzeViewModel viewModel;

    public AnalyzeView()
    {
        InitializeComponent();

        viewModel = new AnalyzeViewModel();
        DataContext = viewModel;

        if (Design.IsDesignMode) {
            viewModel.Nodes.Add(new TreeGridNode(NodeFactory.FromMemory("File 1")));

            var testNodeParent = NodeFactory.CreateContainer("Parent");
            testNodeParent.Add(NodeFactory.FromMemory("Child 1"));
            testNodeParent.Add(NodeFactory.FromMemory("Child 2"));
            viewModel.Nodes.Add(new TreeGridNode(testNodeParent));
        }

        viewModel.AskUserForInputFile.RegisterHandler(SelectInputFiles);
        viewModel.AskUserForInputFolder.RegisterHandler(SelectInputFolder);
        viewModel.DisplayConversionError.RegisterHandler(DisplayConversionError);
        viewModel.AskUserForFileSave.RegisterHandler(SelectOutputFile);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // As converter list doesn't change we can do it once
        foreach (var item in converterTreeView.Items) {
            var itemView = converterTreeView.ContainerFromItem(item!);
            converterTreeView.ExpandSubTree((itemView as TreeViewItem)!);
        }
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

    private async Task<object> DisplayConversionError(ConversionErrorViewModel errorInfo)
    {
        return await Dispatcher.UIThread.InvokeAsync(async () => {
            var dialog = new TaskDialog() {
                Header = "Error converting format",
                IconSource = new FontIconSource { Glyph = "\uf071", FontFamily = "avares://SceneGate.UI/Assets/Fonts#Symbols Nerd Font" },
                SubHeader = "There was an issue converting the node.",
                Content = errorInfo,
                Buttons = [new TaskDialogButton("Close", TaskDialogStandardResult.Close)],
                XamlRoot = VisualRoot as Visual,
            };

            // windows mode false by setting showHosted to true
            return await dialog.ShowAsync(showHosted: true).ConfigureAwait(false);
        });
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
