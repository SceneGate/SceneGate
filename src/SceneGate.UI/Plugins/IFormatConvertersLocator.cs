namespace SceneGate.UI.Plugins;
using System.Collections.Generic;
using Yarhl.FileFormat;

public interface IFormatConvertersLocator
{
    IReadOnlyList<ConverterMetadata> ConvertersMetadata { get; }
}
