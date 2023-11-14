namespace SceneGate.UI.Formats;
using System;
using SceneGate.UI.Formats.Texts;
using Yarhl.FileFormat;
using Yarhl.Media.Text;

public class CommonFormatsViewModelBuilder : IFormatViewModelBuilder
{
    public IFormatViewModel Build(IFormat format)
    {
        if (format is Po) {
            return new PoViewModel(format as Po);
        }

        throw new NotSupportedException("Invalid format");
    }

    public bool CanShow(IFormat format)
    {
        if (format is Po) {
            return true;
        }

        return false;
    }
}
