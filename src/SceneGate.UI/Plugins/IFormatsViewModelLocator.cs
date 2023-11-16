namespace SceneGate.UI.Plugins;
using System.Collections.Generic;
using SceneGate.UI.Formats;

public interface IFormatsViewModelLocator
{
    IReadOnlyList<IFormatViewModelBuilder> ViewModelBuilders { get; }
}
