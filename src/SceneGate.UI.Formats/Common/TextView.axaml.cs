namespace SceneGate.UI.Formats.Common;

using System.Globalization;
using Avalonia.Controls;
using SceneGate.UI.Formats.Controls;

/// <summary>
/// View for binary content as text.
/// </summary>
public partial class TextView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextView"/> class.
    /// </summary>
    public TextView()
    {
        InitializeComponent();
    }

    private void HexCheckboxChecked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var checkbox = sender as CheckBox;
        OffsetNumericBox.TextConverter = (checkbox?.IsChecked ?? false)
            ? new HexadecimalValueConverter()
            : null;
        RefreshNumericBox(OffsetNumericBox);

        LengthNumericBox.TextConverter = (checkbox?.IsChecked ?? false)
            ? new HexadecimalValueConverter()
            : null;
        RefreshNumericBox(LengthNumericBox);
    }

    private static void RefreshNumericBox(NumericUpDown box)
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
