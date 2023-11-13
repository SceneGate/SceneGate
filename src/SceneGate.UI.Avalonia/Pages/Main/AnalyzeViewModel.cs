namespace SceneGate.UI.Pages.Main;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
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
    private ObservableCollection<TreeGridConverter> compatibleConverters;

    [ObservableProperty]
    private ConverterMetadata? selectedConverter;

    [ObservableProperty]
    private string converterFilter;

    public AnalyzeViewModel()
    {
        converters = PluginManager.Instance.GetConverters().Select(x => x.Metadata).ToArray();
        converterFilter = string.Empty;

        nodes = new ObservableCollection<TreeGridNode>();
        formatViewTabs = new ObservableCollection<NodeFormatTab>();
        compatibleConverters = new ObservableCollection<TreeGridConverter>();

        AskUserForFile = new AsyncInteraction<IEnumerable<IStorageFile>>();
        AskUserForFolder = new AsyncInteraction<IStorageFolder?>();
        DisplayConversionError = new AsyncInteraction<string, object>();

        UpdateCompatibleConverters();
    }

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

        //var formatViews = UiPluginManager.Instance.GetCompatibleView(SelectedNode?.Node.Format);
        //BaseFormatView selectedView = null;

        //// We need to implement a better way to autodiscovery of views
        //// rather than skipping hard-coded views. In "LibGame" each discovery
        //// was returning a "matching' score instead of true/false.
        //var nonGenericViews = formatViews.Where(v => v is not ObjectView);
        //if (nonGenericViews.Any()) {
        //    selectedView = nonGenericViews.First();
        //} else {
        //    selectedView = formatViews.FirstOrDefault();
        //}

        //if (selectedView is not null) {
        //    selectedView.ViewModel.Show(SelectedNode?.Node.Format);
        //    Viewer = selectedView;
        //}
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
        var node = SelectedNode;
        if (node is null || SelectedConverter is null) {
            return;
        }

        try {
            // In case some converter doesn't do it...
            if (node.Node.Format is IBinary binaryFormat) {
                binaryFormat.Stream.Position = 0;
            }

            var converterType = SelectedConverter.Type;
            await Task.Run(() => node.Node.TransformWith(converterType)).ConfigureAwait(true);
        } catch (Exception ex) {
            await DisplayConversionError.HandleAsync(ex.ToString()).ConfigureAwait(true);
        }

        //node.UpdateFormatName();
        //node.UpdateChildren();
        //OnNodeUpdate?.Invoke(this, node);

        //SelectedNode = node;
    }

    [RelayCommand]
    private void SaveBinaryNode()
    {
        throw new NotImplementedException();
    }

    partial void OnConverterFilterChanged(string value)
    {
        UpdateCompatibleConverters();
    }

    partial void OnSelectedNodeChanged(TreeGridNode? value)
    {
        UpdateCompatibleConverters();
    }

    private void UpdateCompatibleConverters()
    {
        CompatibleConverters.Clear();

        var node = SelectedNode;
        IEnumerable<ConverterMetadata> compatible = (node?.Node.Format is null)
            ? converters
            : converters.Where(c => c.CanConvert(node.Node.Format.GetType()));

        if (!string.IsNullOrWhiteSpace(ConverterFilter)) {
            compatible = compatible.Where(c => c.Name.Contains(ConverterFilter, StringComparison.OrdinalIgnoreCase));
        }

        foreach (ConverterMetadata converter in compatible) {
            TreeGridConverter.InsertConverterHierarchy(converter, CompatibleConverters);
        }
    }
}
