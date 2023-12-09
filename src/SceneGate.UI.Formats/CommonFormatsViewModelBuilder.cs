namespace SceneGate.UI.Formats;

using System;
using SceneGate.UI.Formats.Common;
using SceneGate.UI.Formats.Graphics;
using SceneGate.UI.Formats.Texts;
using Texim.Palettes;
using Yarhl.FileFormat;
using Yarhl.IO;
using Yarhl.Media.Text;

/// <summary>
/// Formats view model locator for common formats.
/// </summary>
public class CommonFormatsViewModelBuilder : IFormatViewModelBuilder
{
    /// <inheritdoc/>
    public IFormatViewModel Build(IFormat format)
    {
        return format switch {
            IBinary binaryFormat => new HexViewerViewModel(binaryFormat.Stream),

            Po poFormat => new PoViewModel(poFormat),

            IPalette palette => new PaletteViewModel(palette),
            IPaletteCollection palettes => new PaletteViewModel(palettes),

            _ => throw new NotSupportedException("Invalid format"),
        };
    }

    /// <inheritdoc/>
    public bool CanShow(IFormat format)
    {
        return format is IBinary or Po or IPalette or IPaletteCollection;
    }
}
