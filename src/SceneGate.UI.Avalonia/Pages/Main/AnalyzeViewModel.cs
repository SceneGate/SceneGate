namespace SceneGate.UI.Pages.Main;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SceneGate.UI.ControlsData;
using SceneGate.UI.Mvvm;
using Yarhl;
using Yarhl.FileFormat;
using Yarhl.FileSystem;
using Yarhl.IO;

public partial class AnalyzeViewModel : ViewModelBase
{
    private readonly ConverterMetadata[] converters;

    [ObservableProperty]
    private ObservableCollection<TreeGridNode> nodes;

    [ObservableProperty]
    private TreeGridNode? selectedNode;

    [ObservableProperty]
    private ObservableCollection<NodeFormatTab> formatViewTabs;

    [ObservableProperty]
    private NodeFormatTab? selectedTab;

    [ObservableProperty]
    private TreeGridConverter? selectedConverter;

    [ObservableProperty]
    private string? converterFilter;

    public AnalyzeViewModel()
    {
        converters = PluginManager.Instance.GetConverters().Select(x => x.Metadata).ToArray();
        ConverterNodes = new ObservableCollection<TreeGridConverter>();
        CreateConverterNodes();

        nodes = new ObservableCollection<TreeGridNode>();
        formatViewTabs = new ObservableCollection<NodeFormatTab>();

        AskUserForFile = new AsyncInteraction<IEnumerable<IStorageFile>>();
        AskUserForFolder = new AsyncInteraction<IStorageFolder?>();
        DisplayConversionError = new AsyncInteraction<string, object>();
    }

    public ObservableCollection<TreeGridConverter> ConverterNodes { get; }

    public AsyncInteraction<IEnumerable<IStorageFile>> AskUserForFile { get; }

    public AsyncInteraction<IStorageFolder?> AskUserForFolder { get; }

    public AsyncInteraction<string, object> DisplayConversionError { get; }

    [RelayCommand]
    private async Task AddFileAsync()
    {
        var files = await AskUserForFile.HandleAsync().ConfigureAwait(false);
        var paths = files.Select(f => f.TryGetLocalPath()).Where(x => x is not null);

        foreach (string? file in paths) {
            Node node = NodeFactory.FromFile(file!);
            Nodes.Add(new TreeGridNode(node));
        }
    }

    [RelayCommand]
    private async Task AddFolderAsync()
    {
        IStorageFolder? folder = await AskUserForFolder.HandleAsync().ConfigureAwait(false);
        string? path = folder?.TryGetLocalPath();
        if (path is null) {
            return;
        }

        string name = Path.GetFileName(path);
        Node node = NodeFactory.FromDirectory(path, "*", name, subDirectories: true);
        Nodes.Add(new TreeGridNode(node));
    }

    [RelayCommand]
    private void OpenNodeView()
    {
        if (SelectedNode is null || SelectedNode.Node.IsContainer) {
            return;
        }

        // Already opened
        if (FormatViewTabs.Any(x => x.Node == SelectedNode.Node)) {
            return;
        }

        // TODO: get format view from plugins
        var tab = new NodeFormatTab(SelectedNode.Node, SelectedNode.Kind, "TODO");
        FormatViewTabs.Add(tab);
        SelectedTab = tab;
    }

    [RelayCommand]
    private void CloseNodeView(NodeFormatTab tab)
    {
        if (tab is null) {
            return;
        }

        FormatViewTabs.Remove(tab);
    }

    [RelayCommand]
    private async Task ConvertNodeAsync()
    {
        TreeGridNode? node = SelectedNode;
        if (node is null || SelectedConverter?.Converter is null) {
            return;
        }

        try {
            // In case some converter doesn't do it...
            if (node.Node.Format is IBinary binaryFormat) {
                binaryFormat.Stream.Position = 0;
            }

            Type converterType = SelectedConverter.Converter.Type;
            await node.TransformAsync(converterType).ConfigureAwait(true);

            Dispatcher.UIThread.Post(UpdateCompatibleConverters);
        } catch (Exception ex) {
            _ = await DisplayConversionError.HandleAsync(ex.ToString()).ConfigureAwait(true);
        }
    }

    [RelayCommand]
    private void SaveBinaryNode()
    {
        throw new NotImplementedException();
    }

    partial void OnConverterFilterChanged(string? value)
    {
        UpdateCompatibleConverters();
    }

    partial void OnSelectedNodeChanged(TreeGridNode? value)
    {
        UpdateCompatibleConverters();
    }

    private void CreateConverterNodes()
    {
        foreach (ConverterMetadata converter in converters) {
            TreeGridConverter.InsertConverterHierarchy(converter, ConverterNodes);
        }
    }

    private void UpdateCompatibleConverters()
    {
        foreach (TreeGridConverter node in ConverterNodes) {
            node.UpdateVisibility(ConverterFilter, SelectedNode?.Node.Format?.GetType());
        }
    }
}
