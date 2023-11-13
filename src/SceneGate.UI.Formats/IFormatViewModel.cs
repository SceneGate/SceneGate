namespace SceneGate.UI.Formats;

using Yarhl.FileFormat;

/// <summary>
/// View model for a format view.
/// </summary>
public interface IFormatViewModel
{
    /// <summary>
    /// Returns a value indicating whether the view model support the given format.
    /// </summary>
    /// <param name="format">The format to check.</param>
    /// <returns>Value indicating if the view model supports the format.</returns>
    bool CanShow(IFormat format);

    /// <summary>
    /// Show the given format.
    /// </summary>
    /// <param name="format">The format to show.</param>
    void Show(IFormat format);
}
