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
using Yarhl.FileSystem;

namespace SceneGate.UI.Pages.Main;

public partial class AnalyzeViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<TreeGridNode> nodes;

    [ObservableProperty]
    private TreeGridNode? selectedNode;

    [ObservableProperty]
    private ObservableCollection<NodeFormatTab> openedNodes;

    [ObservableProperty]
    private NodeFormatTab? selectedTab;

    public AnalyzeViewModel()
    {
        nodes = new ObservableCollection<TreeGridNode>();
        openedNodes = new ObservableCollection<NodeFormatTab>();

        AskUserForFile = new AsyncInteraction<IEnumerable<IStorageFile>>();
        AskUserForFolder = new AsyncInteraction<IStorageFolder?>();
    }

    public AsyncInteraction<IEnumerable<IStorageFile>> AskUserForFile { get; }

    public AsyncInteraction<IStorageFolder?> AskUserForFolder { get; }

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
        if (OpenedNodes.Any(x => x.Node == SelectedNode.Node)) {
            return;
        }

        // TODO: get format view from plugins
        var tab = new NodeFormatTab(SelectedNode.Node, SelectedNode.Kind, "TODO");
        OpenedNodes.Add(tab);
        SelectedTab = tab;
    }

    [RelayCommand]
    private void CloseNodeView(NodeFormatTab tab)
    {
        if (tab is null) {
            return;
        }

        OpenedNodes.Remove(tab);
    }
}
