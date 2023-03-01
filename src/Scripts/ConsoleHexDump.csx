using System;


byte[] randomData = new byte[1024];
Random.Shared.NextBytes(randomData);
var stream = new MemoryStream(randomData);

string hexDump = DumpStreamToHexString(stream, 0, (int)stream.Length);
Console.WriteLine(hexDump);


static string DumpStreamToHexString(Stream stream, long startPosition, int length)
{
    const int BytesPerRow = 0x10;
    const char horizontalBar = '─'; // or '|' for non-Unicode terminals
    const char verticalBar = '│'; // or '-' for non-Unicode terminals
    const char crossBar = '┼'; // or '|' for non-Unicode terminals

    byte[] buffer = new byte[length];
    int read = stream.Read(buffer);

    var content = new StringBuilder();
    var textLineBuilder = new StringBuilder();

    // Header
    content.AppendFormat("Offset   {0} ", verticalBar);
    for (int i = 0; i < BytesPerRow; i++) {
        content.AppendFormat("{0:X2} ", i);
    }

    content.AppendFormat("{0} ASCII\n", verticalBar);
    content.Append(new string(horizontalBar, 9));
    content.Append(crossBar);
    content.Append(new string(horizontalBar, 1 + (BytesPerRow * 3)));
    content.Append(crossBar);
    content.AppendLine(new string(horizontalBar, 1 + (BytesPerRow * 2)));

    // For each line: offset, hex content and text content
    content.AppendFormat("{0:X8} {1} ", startPosition, verticalBar);
    for (int i = 0; i < read; i++) {
        char ch = (buffer[i] >= 0x21 && buffer[i] <= 0x7F) ? (char)buffer[i] : '.';
        textLineBuilder.AppendFormat("{0} ", ch);

        if (i != 0 && ((i + 1) % BytesPerRow == 0)) {
            content.AppendFormat("{0:X2} {2} {1}\n", buffer[i], textLineBuilder.ToString(), verticalBar);
            content.AppendFormat("{0:X8} {1} ", startPosition + (i + 1), verticalBar);
            textLineBuilder.Clear();
        } else {
            content.AppendFormat("{0:X2} ", buffer[i]);
        }
    }

    return content.ToString();
}
