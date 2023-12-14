namespace SceneGate.UI.Formats.Binary;

using System;
using Yarhl.IO;

public sealed class DesignBinaryViewModel : BinaryViewModel
{
    public DesignBinaryViewModel()
        : base(CreateRandomText())
    {
    }

    private static IBinary CreateRandomText()
    {
        var random = new Random(42);
        byte[] buffer = new byte[1024];
        for (int i = 0; i < buffer.Length; i++) {
            buffer[i] = (byte)random.Next(0x30, 0x7F);
        }

        return new BinaryFormat(DataStreamFactory.FromArray(buffer));
    }
}
