namespace SceneGate.UI.Formats.Graphics;

/// <summary>
/// Kind of pixel swizzling for images.
/// </summary>
public enum SwizzlingKind
{
    /// <summary>
    /// No swizzling (lineal).
    /// </summary>
    None,

    /// <summary>
    /// Tiles of a given size read by row.
    /// </summary>
    TiledHorizontal,
}
