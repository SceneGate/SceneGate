namespace SceneGate.UI.Formats.Binary;

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SceneGate.UI.Formats.Common;
using Yarhl.IO;

/// <summary>
/// View model for <see cref="BinaryView"/>.
/// </summary>
public partial class BinaryViewModel : ObservableObject, IFormatViewModel
{
    [ObservableProperty]
    private HexViewerViewModel hexadecimal;

    [ObservableProperty]
    private TextViewModel text;

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryViewModel"/> class.
    /// </summary>
    /// <param name="binary">The binary content to display.</param>
    public BinaryViewModel(IBinary binary)
    {
        ArgumentNullException.ThrowIfNull(binary);

        hexadecimal = new HexViewerViewModel(binary);
        text = new TextViewModel(binary);
    }
}
