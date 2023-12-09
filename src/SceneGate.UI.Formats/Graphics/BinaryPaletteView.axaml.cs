namespace SceneGate.UI.Formats.Graphics;

using System.Globalization;
using Avalonia.Controls;
using SceneGate.UI.Formats.Controls;

/// <summary>
/// View to see a binary stream as a palette.
/// </summary>
public partial class BinaryPaletteView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryPaletteView"/> class.
    /// </summary>
    public BinaryPaletteView()
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

    private void LengthHexCheckboxChecked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var checkbox = sender as CheckBox;
        LengthNumericBox.TextConverter = (checkbox?.IsChecked ?? false)
            ? new HexadecimalValueConverter()
            : null;
        RefreshNumericBox(LengthNumericBox);
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
