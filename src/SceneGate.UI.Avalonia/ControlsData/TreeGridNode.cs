using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Yarhl.FileSystem;
using Yarhl.IO;
using Yarhl.Media.Text;

namespace SceneGate.UI.ControlsData;

public class TreeGridNode
{
    public TreeGridNode(Node node)
    {
        Node = node;
        Kind = GetKind(node);

        Children = new ObservableCollection<TreeGridNode>(
            node.Children
            .OrderBy(c => !c.IsContainer)
            .ThenBy(c => c.Name)
            .Select(n => new TreeGridNode(n))
            .ToList());
    }

    public Node Node { get; }

    public ObservableCollection<TreeGridNode> Children { get; }

    public NodeFormatKind Kind { get; }

    public string Name => Node.Name;

    public void Add(Node node)
    {
        var child = new TreeGridNode(node);
        Children.Add(child);
        Node.Add(node);
    }

    private static NodeFormatKind GetKind(Node node)
    {
        return node.Format switch {
            NodeContainerFormat => NodeFormatKind.Folder,
            null => NodeFormatKind.Folder,

            IBinary => NodeFormatKind.Binary,
            Po => NodeFormatKind.Translation,

            _ => NodeFormatKind.Unknown,
        };
    }
}
