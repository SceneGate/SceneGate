namespace SceneGate.UI.Formats.Graphics;

using System;
using Yarhl.IO;

/// <summary>
/// Design model for <see cref="RawImageOptionsViewModel"/>.
/// </summary>
public class DesignRawImageOptionsViewModel : RawImageOptionsViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DesignRawImageOptionsViewModel"/> class.
    /// </summary>
    public DesignRawImageOptionsViewModel()
        : base(CreateRandomBinaryImage())
    {
    }

    private static IBinary CreateRandomBinaryImage()
    {
        byte[] bytes = new byte[256 * 192 * 4];
        var random = new Random(42);
        random.NextBytes(bytes);

        return new BinaryFormat(DataStreamFactory.FromArray(bytes));
    }
}
