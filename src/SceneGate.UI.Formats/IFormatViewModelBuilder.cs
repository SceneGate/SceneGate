namespace SceneGate.UI.Formats;

using System;
using System.Collections.Generic;
using Yarhl.FileFormat;

/// <summary>
/// Interface to locate and create view models for formats.
/// </summary>
public interface IFormatViewModelBuilder
{
    /// <summary>
    /// Returns a value indicating whether the view model support the given format.
    /// </summary>
    /// <param name="format">The format to check.</param>
    /// <returns>Value indicating if the view model supports the format.</returns>
    bool CanShow(IFormat format, IReadOnlyCollection<Type> formatsCache);

    /// <summary>
    /// Creates a new view model for the given format.
    /// </summary>
    /// <param name="format">The format to get its view model.</param>
    /// <returns>The new view model.</returns>
    IFormatViewModel Build(IFormat format, IReadOnlyDictionary<Type, IFormat> formatsCache);
}
