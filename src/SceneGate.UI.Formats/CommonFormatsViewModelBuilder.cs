namespace SceneGate.UI.Formats;

using System;
using SceneGate.UI.Formats.Common;
using SceneGate.UI.Formats.Texts;
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
        if (format is Po poFormat) {
            return new PoViewModel(poFormat);
        }

        if (format is IBinary binaryFormat) {
            return new HexViewerViewModel(binaryFormat.Stream);
        }

        throw new NotSupportedException("Invalid format");
    }

    /// <inheritdoc/>
    public bool CanShow(IFormat format)
    {
        if (format is Po or IBinary) {
            return true;
        }

        return false;
    }
}
