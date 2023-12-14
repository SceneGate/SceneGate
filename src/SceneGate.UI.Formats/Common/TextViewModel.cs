namespace SceneGate.UI.Formats.Common;

using System;
using System.Buffers;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Yarhl.IO;

public partial class TextViewModel : ObservableObject, IFormatViewModel
{
    private const int MaxBufferLength = 100 * 1024;

    private readonly Stream stream;
    private readonly byte[] buffer;
    private readonly int bufferLength;

    private Encoding encoding;

    [ObservableProperty]
    private string text;

    [ObservableProperty]
    private string encodingName;

    public TextViewModel(IBinary binary)
    {
        ArgumentNullException.ThrowIfNull(binary);

        stream = binary.Stream;
        bufferLength = stream.Length > MaxBufferLength ? MaxBufferLength : (int)stream.Length;
        buffer = ArrayPool<byte>.Shared.Rent(bufferLength);
        ReadBuffer();

        text = string.Empty;
        encodingName = "UTF-8";
        encoding = Encoding.UTF8;
        DecodeText();
    }

    partial void OnEncodingNameChanging(string value)
    {
        try {
            encoding = Encoding.GetEncoding(
                value,
                EncoderFallback.ReplacementFallback,
                DecoderFallback.ReplacementFallback);
        } catch {
        }
    }

    partial void OnEncodingNameChanged(string value)
    {
        DecodeText();
    }

    private void ReadBuffer()
    {
        stream.Position = 0;
        stream.Read(buffer, 0, bufferLength);
    }

    private void DecodeText()
    {
        Text = encoding.GetString(buffer, 0, bufferLength);
    }
}
