﻿namespace SceneGate.UI.Formats;

using System;
using System.Collections.Generic;
using System.Linq;
using SceneGate.UI.Formats.Common;
using SceneGate.UI.Formats.Graphics;
using SceneGate.UI.Formats.Texts;
using Texim.Images;
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
    public IFormatViewModel Build(IFormat format, IReadOnlyDictionary<Type, IFormat> formatsCache)
    {
        return format switch {
            IBinary binaryFormat => new HexViewerViewModel(binaryFormat.Stream),

            Po poFormat => new PoViewModel(poFormat),

            IPalette palette => new PaletteViewModel(palette),
            IPaletteCollection palettes => new PaletteViewModel(palettes),

            IFullImage fullImage => new ImageViewModel(fullImage),
            IIndexedImage indexedImage => new ImageViewModel(
                indexedImage,
                (IPaletteCollection)formatsCache[typeof(IPaletteCollection)]),

            _ => throw new NotSupportedException("Invalid format"),
        };
    }

    /// <inheritdoc/>
    public bool CanShow(IFormat format, IReadOnlyCollection<Type> formatsCache)
    {
        if (format is IIndexedImage && formatsCache.Contains(typeof(IPaletteCollection))) {
            return true;
        }

        return format is IBinary or Po or IPalette or IPaletteCollection or IFullImage;
    }
}
