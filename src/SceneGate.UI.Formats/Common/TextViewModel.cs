namespace SceneGate.UI.Formats.Common;

using System;
using System.Buffers;
using System.IO;
using System.Text;
using Avalonia.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using Yarhl.IO;

/// <summary>
/// View model for <see cref="TextView"/>.
/// </summary>
public partial class TextViewModel : ObservableObject, IFormatViewModel
{
    private const int MaxBufferLength = 10 * 1024;

    private readonly Stream stream;
    private readonly byte[] buffer;
    private readonly int bufferLength;

    private Encoding encoding;

    [ObservableProperty]
    private string text;

    [ObservableProperty]
    private string encodingName;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextViewModel"/> class.
    /// </summary>
    /// <param name="binary">Binary content to display.</param>
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

    partial void OnEncodingNameChanged(string value)
    {
        try {
            encoding = Encoding.GetEncoding(
                value,
                EncoderFallback.ReplacementFallback,
                DecoderFallback.ReplacementFallback);

            DecodeText();
        } catch (ArgumentException) {
            throw new DataValidationException("Invalid encoding name");
        }
    }

    private void ReadBuffer()
    {
        stream.Position = 0;
        _ = stream.Read(buffer, 0, bufferLength);
    }

    private void DecodeText()
    {
        Text = encoding.GetString(buffer, 0, bufferLength);
    }
}
