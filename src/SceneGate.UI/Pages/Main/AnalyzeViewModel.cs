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
using SceneGate.UI.Formats;
using SceneGate.UI.Formats.Binary;
using SceneGate.UI.Formats.Models;
using SceneGate.UI.Formats.Graphics;
using SceneGate.UI.Formats.Mvvm;
using SceneGate.UI.Pages.Analyze;
using Texim.Palettes;
using Yarhl.FileFormat;
using Yarhl.FileSystem;
using Yarhl.IO;
using Yarhl.Plugins;
using Yarhl.Plugins.FileFormat;

public partial class AnalyzeViewModel : ViewModelBase
{
    private readonly object listLock = new object();
    private readonly List<IFormatViewModelBuilder> formatsViewModelBuilders;
    private readonly Dictionary<Type, IFormat> formatsCache;

    [ObservableProperty]
    private ObservableCollection<TreeGridNode> nodes;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenNodeViewCommand))]
    [NotifyCanExecuteChangedFor(nameof(ConvertNodeCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveBinaryNodeCommand))]
    [NotifyCanExecuteChangedFor(nameof(CopyPathCommand))]
    private TreeGridNode? selectedNode;

    [ObservableProperty]
    private ObservableCollection<NodeFormatTab> formatViewTabs;

    [ObservableProperty]
    private NodeFormatTab? selectedTab;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConvertNodeCommand))]
    [NotifyCanExecuteChangedFor(nameof(CopyConverterTypeNameCommand))]
    private TreeGridConverter? selectedConverter;

    [ObservableProperty]
    private string? nodeNameFilter;

    [ObservableProperty]
    private string? converterFilter;

    public AnalyzeViewModel()
    {
        formatsCache = new();
        formatsViewModelBuilders = TypeLocator.Default
            .FindImplementationsOf(typeof(IFormatViewModelBuilder))
            .Select(i => (IFormatViewModelBuilder)Activator.CreateInstance(i.Type)!)
            .ToList();

        AppDomain.CurrentDomain.AssemblyLoad += (sender, e) => {
            // TODO Yarhl: Scan given assembly only
            ConverterLocator.Default.ScanAssemblies();
            Dispatcher.UIThread.Post(FindNewConverterNodes);

            lock (listLock) {
                formatsViewModelBuilders.AddRange(TypeLocator.Default
                    .FindImplementationsOf(typeof(IFormatViewModelBuilder), e.LoadedAssembly)
                    .Select(i => (IFormatViewModelBuilder)Activator.CreateInstance(i.Type)!));
            }
        };

        ConverterNodes = new ObservableCollection<TreeGridConverter>();
        FindNewConverterNodes();

        nodes = new ObservableCollection<TreeGridNode>();
        formatViewTabs = new ObservableCollection<NodeFormatTab>();

        AskUserForInputFile = new AsyncInteraction<IEnumerable<IStorageFile>>();
        AskUserForInputFolder = new AsyncInteraction<IStorageFolder?>();
        AskUserForFileSave = new AsyncInteraction<string, IStorageFile?>();
        DisplayConvertingProgress = new AsyncInteraction<NodeConversionInfo, object?>();
        DisplayConversionError = new AsyncInteraction<ConversionErrorViewModel, object>();
        NotifyConversionFinished = new AsyncInteraction<object?>();
        CopyToClipboard = new AsyncInteraction<string, object>();
    }

    public ObservableCollection<TreeGridConverter> ConverterNodes { get; }

    public AsyncInteraction<IEnumerable<IStorageFile>> AskUserForInputFile { get; }

    public AsyncInteraction<IStorageFolder?> AskUserForInputFolder { get; }

    public AsyncInteraction<string, IStorageFile?> AskUserForFileSave { get; }

    public AsyncInteraction<NodeConversionInfo, object?> DisplayConvertingProgress { get; }

    public AsyncInteraction<ConversionErrorViewModel, object> DisplayConversionError { get; }

    public AsyncInteraction<object?> NotifyConversionFinished { get; }

    public AsyncInteraction<string, object> CopyToClipboard { get; }

    [RelayCommand]
    private async Task AddFileAsync()
    {
        var files = await AskUserForInputFile.HandleAsync().ConfigureAwait(false);
        var paths = files.Select(f => f.TryGetLocalPath()).Where(x => x is not null);

        foreach (string? file in paths) {
            Node node = NodeFactory.FromFile(file!);
            Nodes.Add(new TreeGridNode(node));
        }
    }

    [RelayCommand]
    private async Task AddFolderAsync()
    {
        IStorageFolder? folder = await AskUserForInputFolder.HandleAsync().ConfigureAwait(false);
        string? path = folder?.TryGetLocalPath();
        if (path is null) {
            return;
        }

        string name = Path.GetFileName(path);
        Node node = NodeFactory.FromDirectory(path, "*", name, subDirectories: true);
        Nodes.Add(new TreeGridNode(node));
    }

    [RelayCommand(CanExecute = nameof(CanOpenNodeView))]
    private void OpenNodeView()
    {
        TreeGridNode? selection = SelectedNode;
        if (selection is null) {
            return;
        }

        Node node = selection.Node;
        IFormat? format = node.Format;
        if (format is null) {
            return;
        }

        // Already opened - select it
        NodeFormatTab? existingTab = FormatViewTabs.FirstOrDefault(x => x.Node == node);
        if (existingTab is not null) {
            SelectedTab = existingTab;
            return;
        }

        // We get the view model from our plugins locator (custom assembly scanner)
        IFormatViewModelBuilder? vmBuilder;
        lock (listLock) {
            vmBuilder = formatsViewModelBuilders.Find(b => b.CanShow(format, formatsCache.Keys));
        }

        // The plugin will build the view model
        IFormatViewModel formatViewModel = (vmBuilder is not null)
            ? vmBuilder.Build(format, formatsCache)
            : new ObjectViewModel(format);

        // We bind the view model to the content of the tab.
        // This will trigger in Avalonia the usage of all the ViewLocators (IDataTemplate)
        // to find its best view / worst case it prints a not found in a textblock.
        var tab = new NodeFormatTab(node, selection.Kind, formatViewModel);
        FormatViewTabs.Add(tab);
        UpdateFormatsCache(node);

        SelectedTab = tab;
    }

    private bool CanOpenNodeView()
    {
        return SelectedNode is not null;
    }

    private void OpenNodeViewWith(IFormatViewModel formatViewModel)
    {
        TreeGridNode? selection = SelectedNode;
        if (selection is null) {
            return;
        }

        var tab = new NodeFormatTab(selection.Node, selection.Kind, formatViewModel);
        FormatViewTabs.Add(tab);
        UpdateFormatsCache(selection.Node);

        SelectedTab = tab;
    }

    [RelayCommand]
    private void CloseNodeView(NodeFormatTab tab)
    {
        if (tab is null) {
            return;
        }

        FormatViewTabs.Remove(tab);

        // It seems there is a bug when closing not-the-last-tab, force a refresh
        NodeFormatTab? newTab = SelectedTab;
        SelectedTab = null;
        SelectedTab = newTab;
    }

    [RelayCommand(CanExecute = nameof(CanConvertNode))]
    private async Task ConvertNodeAsync()
    {
        TreeGridNode? node = SelectedNode;
        if (node is null || SelectedConverter?.Converter is null) {
            return;
        }

        Type converterType = SelectedConverter.Converter.Type;
        var conversionInfo = new NodeConversionInfo(node.Node, converterType, null);
        try {
            await DisplayConvertingProgress.HandleAsync(conversionInfo).ConfigureAwait(false);

            // In case some converter doesn't do it...
            if (node.Node.Format is IBinary binaryFormat) {
                binaryFormat.Stream.Position = 0;
            }

            await node.TransformAsync(converterType).ConfigureAwait(false);
            UpdateFormatsCache(node.Node);

            Dispatcher.UIThread.Post(() => {
                // We can't keep it open as the previous format may have been disposed
                // and keeping all of them may consume a lot of memory.
                NodeFormatTab? nodeTab = FormatViewTabs.FirstOrDefault(x => x.Node == node.Node);
                if (nodeTab is not null) {
                    CloseNodeView(nodeTab);
                }

                UpdateCompatibleConverters();
            });
        } catch (Exception ex) {
            var errorInfo = new ConversionErrorViewModel(node.Node, converterType, null, ex);
            _ = await DisplayConversionError.HandleAsync(errorInfo).ConfigureAwait(false);
        }

        await NotifyConversionFinished.HandleAsync().ConfigureAwait(false);
    }

    private bool CanConvertNode()
    {
        return SelectedNode is not null && SelectedConverter?.Converter is not null;
    }

    [RelayCommand(CanExecute = nameof(CanSaveBinaryNode))]
    private async Task SaveBinaryNodeAsync()
    {
        Node? node = SelectedNode?.Node;
        if (node?.Stream is null) {
            return;
        }

        IStorageFile? file = await AskUserForFileSave.HandleAsync(node.Name).ConfigureAwait(false);
        string? outputPath = file?.TryGetLocalPath();
        if (outputPath is null) {
            return;
        }

        node.Stream.WriteTo(outputPath);
    }

    private bool CanSaveBinaryNode()
    {
        return SelectedNode?.Node.Format is IBinary;
    }

    [RelayCommand(CanExecute = nameof(CanCopyPath))]
    private async Task CopyPath()
    {
        string? path = SelectedNode?.Node?.Path;
        if (path is null) {
            return;
        }

        await CopyToClipboard.HandleAsync(path);
    }

    private bool CanCopyPath() => SelectedNode is not null;

    [RelayCommand(CanExecute = nameof(CanCopyConverterTypeName))]
    private async Task CopyConverterTypeName()
    {
        Type? type = SelectedConverter?.Converter?.Type;
        if (type is null) {
            return;
        }

        await CopyToClipboard.HandleAsync(type.FullName!);
    }

    private bool CanCopyConverterTypeName() =>
        SelectedConverter is { Kind: TreeGridConverterKind.Converter };

    [RelayCommand(CanExecute = nameof(CanOpenDecompiledConverter))]
    private void OpenDecompiledConverter()
    {
        ConverterTypeInfo? converter = SelectedConverter?.Converter;
        if (converter is null) {
            return;
        }

        var tab = new NodeFormatTab(
            new Node(converter.Type.Name),
            NodeFormatKind.Unknown,
            new DecompileViewModel(converter.Type));

        FormatViewTabs.Add(tab);
        SelectedTab = tab;
    }

    private bool CanOpenDecompiledConverter() =>
        SelectedConverter is { Kind: TreeGridConverterKind.Converter };


    [RelayCommand(CanExecute = nameof(CanOpenAsObject))]
    private void OpenAsObject()
    {
        if (SelectedNode?.Node.Format is not IFormat format) {
            return;
        }

        var viewModel = new ObjectViewModel(format);
        OpenNodeViewWith(viewModel);
    }

    private bool CanOpenAsObject() => SelectedNode is not null;

    [RelayCommand(CanExecute = nameof(CanOpenWithHexViewer))]
    private void OpenWithHexViewer()
    {
        if (SelectedNode?.Node.Format is not IBinary format) {
            return;
        }

        var viewModel = new HexViewerViewModel(format);
        OpenNodeViewWith(viewModel);
    }

    private bool CanOpenWithHexViewer() => SelectedNode?.Node.Format is IBinary;

    [RelayCommand(CanExecute = nameof(CanOpenAsRawPalette))]
    private void OpenAsRawPalette()
    {
        if (SelectedNode?.Node.Format is not IBinary format) {
            return;
        }

        var viewModel = new BinaryPaletteViewModel(format);
        OpenNodeViewWith(viewModel);
    }

    private bool CanOpenAsRawPalette() => SelectedNode?.Node.Format is IBinary;

    [RelayCommand(CanExecute = nameof(CanOpenAsRawImage))]
    private void OpenAsRawImage()
    {
        if (SelectedNode?.Node.Format is not IBinary format) {
            return;
        }

        if (formatsCache[typeof(IPaletteCollection)] is not IPaletteCollection palettes) {
            return;
        }

        var viewModel = new ImageViewModel(format, palettes);
        OpenNodeViewWith(viewModel);
    }

    private bool CanOpenAsRawImage() =>
        SelectedNode?.Node.Format is IBinary && formatsCache.ContainsKey(typeof(IPaletteCollection));

    partial void OnNodeNameFilterChanged(string? value)
    {
        foreach (TreeGridNode node in Nodes) {
            node.UpdateVisibility(NodeNameFilter);
        }

        if (SelectedNode is { IsVisible: false }) {
            SelectedNode = null;
        }
    }

    partial void OnConverterFilterChanged(string? value)
    {
        UpdateCompatibleConverters();
    }

    partial void OnSelectedNodeChanged(TreeGridNode? value)
    {
        UpdateCompatibleConverters();
    }

    private void FindNewConverterNodes()
    {
        var converters = ConverterLocator.Default.Converters.ToList();
        foreach (ConverterTypeInfo converter in converters) {
            TreeGridConverter.InsertConverterHierarchy(converter, ConverterNodes, groupByNamespace: false);
        }
    }

    private void UpdateCompatibleConverters()
    {
        foreach (TreeGridConverter node in ConverterNodes) {
            node.UpdateVisibility(ConverterFilter, SelectedNode?.Node.Format?.GetType());
        }

        if (SelectedConverter is { IsCompatible: false }) {
            SelectedConverter = null;
        }
    }

    private void UpdateFormatsCache(Node node)
    {
        IFormat? format = node.Format;
        if (format is null) {
            return;
        }

        // TODO: remove before converting the old formats, we will need to keep refs of nodes
        // TODO: remove if the node disappear (after packing)
        // TODO: refactor out of this class
        Type formatType = format.GetType();
        formatsCache[formatType] = format;
        foreach (Type formatInterface in formatType.GetInterfaces()) {
            formatsCache[formatInterface] = format;
        }
    }
}
