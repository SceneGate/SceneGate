namespace SceneGate.UI.Formats.Binary;

using System;
using Yarhl.IO;

/// <summary>
/// Design teste data for <see cref="BinaryViewModel"/>.
/// </summary>
public sealed class DesignBinaryViewModel : BinaryViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DesignBinaryViewModel"/> class.
    /// </summary>
    public DesignBinaryViewModel()
        : base(CreateRandomText())
    {
    }

    private static BinaryFormat CreateRandomText()
    {
        var random = new Random(42);
        byte[] buffer = new byte[1024];
        for (int i = 0; i < buffer.Length; i++) {
            buffer[i] = (byte)random.Next(0x30, 0x7F);
        }

        return new BinaryFormat(DataStreamFactory.FromArray(buffer));
    }
}
