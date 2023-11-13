namespace SceneGate.UI.ControlsData;
using CommunityToolkit.Mvvm.ComponentModel;
using SceneGate.UI.Formats;
using Yarhl.FileSystem;

public partial class NodeFormatTab : ObservableObject
{
    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private NodeFormatKind kind;

    public NodeFormatTab(Node node, NodeFormatKind kind, IFormatViewModel content)
    {
        Node = node;
        Name = node.Name;
        Kind = kind;
        Content = content;
    }

    public Node Node { get; }

    public IFormatViewModel Content { get; }
}
