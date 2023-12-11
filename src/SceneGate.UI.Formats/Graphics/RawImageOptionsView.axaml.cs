namespace SceneGate.UI.Formats.Graphics;

using System.Globalization;
using Avalonia.Controls;
using SceneGate.UI.Formats.Controls;

/// <summary>
/// User control to define the options of a raw image.
/// </summary>
public partial class RawImageOptionsView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RawImageOptionsView"/> class.
    /// </summary>
    public RawImageOptionsView()
    {
        InitializeComponent();
    }

    private void OffsetHexCheckboxChecked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var checkbox = sender as CheckBox;
        OffsetNumericBox.TextConverter = (checkbox?.IsChecked ?? false)
            ? new HexadecimalValueConverter()
            : null;
        RefreshNumericBox(OffsetNumericBox);
    }

    private void RefreshNumericBox(NumericUpDown box)
    {
        // Due to a bug in Avalonia, it doesn't refresh the text when the converter changes
        // We do it manually
        if (box.TextConverter is null) {
            box.Text = box.Value?.ToString(box.FormatString, box.NumberFormat);
        } else {
            box.Text = box.TextConverter.ConvertBack(box.Value, typeof(string), null, CultureInfo.CurrentCulture)?.ToString();
        }
    }
}
