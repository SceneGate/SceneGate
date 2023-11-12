using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    private NodeFormatTab selectedTab;

    public AnalyzeViewModel()
    {
        nodes = new ObservableCollection<TreeGridNode>();
        openedNodes = new ObservableCollection<NodeFormatTab>();

        AskUserForFile = new AsyncInteraction<IEnumerable<IStorageFile>>();
    }

    public AsyncInteraction<IEnumerable<IStorageFile>> AskUserForFile { get; }

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
    private void OpenNodeView()
    {
        if (SelectedNode is null) {
            return;
        }

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
