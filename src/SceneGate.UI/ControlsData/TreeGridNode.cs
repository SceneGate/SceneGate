namespace SceneGate.UI.ControlsData;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Texim.Compressions.Nitro;
using Texim.Images;
using Texim.Palettes;
using Yarhl.FileSystem;
using Yarhl.IO;
using Yarhl.Media.Text;

public partial class TreeGridNode : ObservableObject
{
    [ObservableProperty]
    private NodeFormatKind kind;

    [ObservableProperty]
    private string humanReadableLength;

    [ObservableProperty]
    private string formatName;

    [ObservableProperty]
    private bool isVisible;

    [ObservableProperty]
    private string toolTip;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string path;

    [ObservableProperty]
    private string fullTypeName;

    [ObservableProperty]
    private string offsetHex;

    [ObservableProperty]
    private string fullLength;

    [ObservableProperty]
    private string allTags;

    public TreeGridNode(Node node)
    {
        Node = node;
        Children = new ObservableCollection<TreeGridNode>();
        name = string.Empty;
        path = string.Empty;
        fullTypeName = string.Empty;
        formatName = string.Empty;
        humanReadableLength = string.Empty;
        offsetHex = string.Empty;
        fullLength = string.Empty;
        allTags = string.Empty;

        IsVisible = true;
        ToolTip = string.Empty;

        UpdateNodeInfo();
    }

    public Node Node { get; }

    public ObservableCollection<TreeGridNode> Children { get; }

    public void Add(Node node)
    {
        var child = new TreeGridNode(node);
        Children.Add(child);
        Node.Add(node);
    }

    public async Task TransformAsync(Type converterType)
    {
        _ = await Task.Run(() => Node.TransformWith(converterType)).ConfigureAwait(false);

        await Dispatcher.UIThread.InvokeAsync(UpdateNodeInfo);
    }

    public void UpdateVisibility(string? nameFilter)
    {
        foreach (TreeGridNode child in Children) {
            child.UpdateVisibility(nameFilter);
        }

        bool match = (nameFilter is null) || Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase);
        IsVisible = match || Children.Any(c => c.IsVisible);
    }

    private void UpdateNodeInfo()
    {
        Name = Node.Name;
        Path = Node.Path;
        AllTags = string.Join(", ", Node.Tags.Select(i => $"{i.Key}={i.Value}"));
        FullTypeName = Node.Format?.GetType().FullName ?? "null";
        ToolTip = string.Format("Format: {0}", FullTypeName);

        UpdateFormatName();
        UpdateKind();
        UpdateChildren();
        UpdateLength();

        if (Node.Stream is not null) {
            OffsetHex = $"0x{Node.Stream.Offset:X8}";
            FullLength = $"{Node.Stream.Length} ({HumanReadableLength})";
        } else {
            OffsetHex = string.Empty;
            FullLength = string.Empty;
        }
    }

    private void UpdateFormatName()
    {
        if (Node.Format is null or NodeContainerFormat) {
            FormatName = string.Empty;
        } else if (Node.Format is IBinary) {
            FormatName = ReadBinaryFormatStamp();
        } else {
            FormatName = Node.Format.GetType().Name;
            if (FormatName.EndsWith("Format", StringComparison.InvariantCulture)) {
                FormatName = FormatName[..^"Format".Length];
            }
        }
    }

    private string ReadBinaryFormatStamp()
    {
        var stream = Node.Stream;
        if (stream is null) {
            return string.Empty;
        }

        stream.Position = 0;
        Span<byte> binaryStamp = stackalloc byte[4];
        int read = stream.Read(binaryStamp);
        if (read < 2) {
            return "Binary";
        }

        bool isValid = true;
        Span<char> stamp = stackalloc char[read];
        for (int i = 0; i < read && isValid; i++) {
            if (binaryStamp[i] is < 0x20 or > 0x7F) {
                isValid = false;
            } else {
                stamp[i] = (char)binaryStamp[i];
            }
        }

        return isValid ? $"[{new string(stamp)}]" : "Binary";
    }

    private void UpdateLength()
    {
        string[] Magnitudes = [string.Empty, "kB", "MB", "GB", "TB"];

        var stream = Node.Stream;
        if (stream is null) {
            HumanReadableLength = string.Empty;
            return;
        }

        double length = stream.Length;
        int magnitudeIdx = 0;
        while (length >= 1024 && magnitudeIdx < Magnitudes.Length) {
            length /= 1024;
            magnitudeIdx++;
        }

        HumanReadableLength = (magnitudeIdx == 0)
            ? $"{length} bytes"
            : $"{length:F2} {Magnitudes[magnitudeIdx]}";
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

            IPalette => NodeFormatKind.Palette,
            IPaletteCollection => NodeFormatKind.Palette,

            IFullImage => NodeFormatKind.Image,
            IIndexedImage => NodeFormatKind.Image,
            IScreenMap => NodeFormatKind.Image, // it should go in Ekona somehow?

            _ => NodeFormatKind.Unknown,
        };
    }
}
