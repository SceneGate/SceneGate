﻿namespace SceneGate.UI.Formats.Binary;

using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SceneGate.UI.Formats.Binary;
using SceneGate.UI.Formats.Mvvm;
using Yarhl.IO;
using Yarhl.Media.Text.Encodings;

/// <summary>
/// View model for <see cref="TextView"/>.
/// </summary>
public partial class TextViewModel : ObservableObject, IFormatViewModel
{
    private const int DefaultBufferLength = 10 * 1024;

    private readonly Stream stream;
    private readonly StringBuilder textBuilder;

    private byte[] buffer;
    private Encoding encoding;

    [ObservableProperty]
    private string text;

    [ObservableProperty]
    private string encodingName;

    [ObservableProperty]
    private long offset;

    [ObservableProperty]
    private long maximumOffset;

    [ObservableProperty]
    private int length;

    [ObservableProperty]
    private int maximumLength;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextViewModel"/> class.
    /// </summary>
    /// <param name="binary">Binary content to display.</param>
    public TextViewModel(IBinary binary)
    {
        ArgumentNullException.ThrowIfNull(binary);

        stream = binary.Stream;
        maximumLength = (int)stream.Length;
        length = stream.Length > DefaultBufferLength ? DefaultBufferLength : (int)stream.Length;
        buffer = ArrayPool<byte>.Shared.Rent(length);
        ReadBuffer();

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        encoding = Encoding.GetEncoding(932);
        encodingName = "shift-jis";

        textBuilder = new StringBuilder();
        text = string.Empty;
        DecodeText();

        AskFileOutput = new AsyncInteraction<IStorageFile?>();
    }

    /// <summary>
    /// Gets all the available encodings.
    /// </summary>
    public static string[] SuggestedEncodings => [
        "shift-jis", "utf-8", "utf-16", "utf-16be", "utf-32", "euc-jp"];

    /// <summary>
    /// Gets the interaction to ask the user for the output file.
    /// </summary>
    public AsyncInteraction<IStorageFile?> AskFileOutput { get; }

    /// <summary>
    /// Export the visible content into a file.
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task ExportAsync()
    {
        IStorageFile? file = await AskFileOutput.HandleAsync().ConfigureAwait(false);
        if (file is null) {
            return;
        }

        using Stream output = await file.OpenWriteAsync().ConfigureAwait(false);
        await output.WriteAsync(buffer, 0, Length).ConfigureAwait(false);
    }

    partial void OnEncodingNameChanged(string value)
    {
        try {
            if (value == "euc-jp") {
                // Fixed implementation in yarhl
                encoding = new EucJpEncoding(
                    DecoderFallback.ReplacementFallback,
                    EncoderFallback.ReplacementFallback);
            } else {
                encoding = Encoding.GetEncoding(
                    value,
                    EncoderFallback.ReplacementFallback,
                    DecoderFallback.ReplacementFallback);
            }

            DecodeText();
        } catch (ArgumentException) {
            throw new DataValidationException("Invalid encoding name");
        }
    }

    partial void OnOffsetChanged(long value)
    {
        MaximumLength = (int)(stream.Length - value);
        ReadBuffer();
        DecodeText();
    }

    partial void OnLengthChanged(int value)
    {
        MaximumOffset = stream.Length - value;

        if (value > buffer.Length) {
            ArrayPool<byte>.Shared.Return(buffer);
            buffer = ArrayPool<byte>.Shared.Rent(value);
        }

        ReadBuffer();
        DecodeText();
    }

    private void ReadBuffer()
    {
        stream.Position = Offset;
        _ = stream.Read(buffer, 0, Length);
    }

    private void DecodeText()
    {
        try {
            textBuilder.Clear();
            if (Offset > 0) {
                textBuilder.AppendFormat("<< There are {0} bytes before the first line >>", Offset)
                    .AppendLine();
            }

            textBuilder.Append(encoding.GetString(buffer, 0, Length));

            if (stream.Length - Offset > Length) {
                textBuilder.AppendLine()
                    .AppendFormat("<< There are {0} bytes after the last line >>", stream.Length - Offset);
            }

            Text = textBuilder.ToString();
        } catch (Exception ex) {
            Text = ex.ToString();
        }
    }
}
