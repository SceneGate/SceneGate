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
    private readonly TaskDialog errorConversionDialog;
    private readonly TaskDialog convertingDialog;
    private Task? convertingTask;

    public AnalyzeView()
    {
        InitializeComponent();

        errorConversionDialog = new TaskDialog() {
            Header = "Error converting format",
            IconSource = new FontIconSource { Glyph = "\uf071", FontFamily = "avares://SceneGate.UI/Assets/Fonts#Symbols Nerd Font" },
            SubHeader = "There was an issue converting the node.",
            Buttons = [new TaskDialogButton("Close", TaskDialogStandardResult.Close)],
        };

        convertingDialog = new TaskDialog {
            Header = "Converting node",
            IconSource = new SymbolIconSource { Symbol = Symbol.Sync },
            Content = string.Empty,
            ShowProgressBar = true,
        };

        viewModel = new AnalyzeViewModel();
        DataContext = viewModel;

        if (Design.IsDesignMode) {
            viewModel.Nodes.Add(new TreeGridNode(NodeFactory.FromArray("File 1", new byte[100])));

            var testNodeParent = NodeFactory.CreateContainer("Parent");
            testNodeParent.Add(NodeFactory.FromMemory("Child 1"));
            testNodeParent.Add(NodeFactory.FromMemory("Child 2"));
            viewModel.Nodes.Add(new TreeGridNode(testNodeParent));
        }

        viewModel.AskUserForInputFile.RegisterHandler(SelectInputFiles);
        viewModel.AskUserForInputFolder.RegisterHandler(SelectInputFolder);
        viewModel.AskUserForFileSave.RegisterHandler(SelectOutputFile);
        viewModel.DisplayConvertingProgress.RegisterHandler(DisplayConversionStarted);
        viewModel.DisplayConversionError.RegisterHandler(DisplayConversionError);
        viewModel.NotifyConversionFinished.RegisterHandler(HideConversionDialog);
        viewModel.CopyToClipboard.RegisterHandler(CopyToClipboardAsync);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        errorConversionDialog.XamlRoot = VisualRoot as Visual;
        convertingDialog.XamlRoot = VisualRoot as Visual;

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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "False positive")]
    private void TabViewTabCloseRequested(TabView? sender, TabViewTabCloseRequestedEventArgs args)
    {
        viewModel.CloseNodeViewCommand.Execute(args.Item as NodeFormatTab);
    }

    private async void ConvertersTreeViewDoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        await viewModel.ConvertNodeCommand.ExecuteAsync(null);
    }

    private async Task<object?> DisplayConversionStarted(NodeConversionInfo info)
    {
        await Dispatcher.UIThread.InvokeAsync(() => {
            convertingDialog.Content = $"Node {info.Node.Path} with converter {info.ConverterType.Name}";
            convertingDialog.SetProgressBarState(50, TaskDialogProgressState.Indeterminate | TaskDialogProgressState.Normal);
            convertingTask = convertingDialog.ShowAsync(showHosted: true);
        });

        return null;
    }

    private async Task<object?> HideConversionDialog()
    {
        if (convertingTask is null) {
            return null;
        }

        await Dispatcher.UIThread.InvokeAsync(convertingDialog.Hide);
        await convertingTask;
        return null;
    }

    private async Task<object> DisplayConversionError(ConversionErrorViewModel errorInfo)
    {
        return await Dispatcher.UIThread.InvokeAsync(async () => {
            errorConversionDialog.Content = errorInfo;

            // windows mode false by setting showHosted to true
            return await errorConversionDialog.ShowAsync(showHosted: true).ConfigureAwait(false);
        });
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

    private async Task<object> CopyToClipboardAsync(string content)
    {
        await TopLevel.GetTopLevel(this)!
            .Clipboard!
            .SetTextAsync(content);

        return null!;
    }
}
