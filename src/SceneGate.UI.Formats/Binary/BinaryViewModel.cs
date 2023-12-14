namespace SceneGate.UI.Formats.Binary;

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SceneGate.UI.Formats.Common;
using Yarhl.IO;

public partial class BinaryViewModel : ObservableObject, IFormatViewModel
{
    [ObservableProperty]
    private HexViewerViewModel hexadecimal;

    [ObservableProperty]
    private TextViewModel text;

    public BinaryViewModel(IBinary binary)
    {
        ArgumentNullException.ThrowIfNull(binary);

        hexadecimal = new HexViewerViewModel(binary);
        text = new TextViewModel(binary);
    }
}
