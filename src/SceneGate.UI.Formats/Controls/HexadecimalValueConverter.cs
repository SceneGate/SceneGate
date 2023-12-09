namespace SceneGate.UI.Formats.Controls;

using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

/// <summary>
/// Value converter for a number and its hexadecimal string representation
/// for the NumericUpDown control.
/// </summary>
public class HexadecimalValueConverter : IValueConverter
{
    /// <summary>
    /// Converts a string value with an hexadecimal number into a target type.
    /// </summary>
    /// <param name="value">The hexadecimal string value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">Not used.</param>
    /// <param name="culture">The culture to parse the string.</param>
    /// <returns>The integer representation of the hexadecimal string.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string? str = value?.ToString();
        if (string.IsNullOrWhiteSpace(str)) {
            return AvaloniaProperty.UnsetValue;
        }

        if (long.TryParse(str, NumberStyles.HexNumber, culture, out long x)) {
            return (decimal)x;
        }

        return AvaloniaProperty.UnsetValue;
    }

    /// <summary>
    /// Converts a decimal number into an hexadecimal string representation.
    /// </summary>
    /// <param name="value">The number to convert.</param>
    /// <param name="targetType">The string type.</param>
    /// <param name="parameter">Not used.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns>Its hexadecimal string representation.</returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) {
            return AvaloniaProperty.UnsetValue;
        }

        try {
            if (value is decimal d) {
                return string.Format(culture, "{0:X8}", (long)d);
            }

            return $"Invalid type: {value.GetType()}";
        } catch (Exception ex) {
            return ex.Message;
        }
    }
}
