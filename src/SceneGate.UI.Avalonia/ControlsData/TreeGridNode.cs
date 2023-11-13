namespace SceneGate.UI.ControlsData;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Yarhl.FileSystem;
using Yarhl.IO;
using Yarhl.Media.Text;

public partial class TreeGridNode : ObservableObject
{
    [ObservableProperty]
    private NodeFormatKind kind;

    public TreeGridNode(Node node)
    {
        Node = node;
        UpdateKind();

        Children = new ObservableCollection<TreeGridNode>();
        UpdateChildren();
    }

    public Node Node { get; }

    public ObservableCollection<TreeGridNode> Children { get; }

    public string Name => Node.Name;

    public void Add(Node node)
    {
        var child = new TreeGridNode(node);
        Children.Add(child);
        Node.Add(node);
    }

    public async Task TransformAsync(Type converterType)
    {
        _ = await Task.Run(() => Node.TransformWith(converterType)).ConfigureAwait(false);

        await Dispatcher.UIThread.InvokeAsync(() => {
            UpdateKind();
            UpdateChildren();
        });
    }

    private void UpdateChildren()
    {
        Children.Clear();

        var children = Node.Children
            .OrderBy(c => !c.IsContainer)
            .ThenBy(c => c.Name)
            .Select(n => new TreeGridNode(n));
        foreach (TreeGridNode child in children) {
            Children.Add(child);
        }
    }

    private void UpdateKind()
    {
        Kind = Node.Format switch {
            NodeContainerFormat => NodeFormatKind.Folder,
            null => NodeFormatKind.Folder,

            IBinary => NodeFormatKind.Binary,
            Po => NodeFormatKind.Translation,

            _ => NodeFormatKind.Unknown,
        };
    }
}
