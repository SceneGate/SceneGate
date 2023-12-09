namespace SceneGate.UI.Formats.Controls;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data.Converters;
using Avalonia;

public class HexadecimalValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string? str = value?.ToString();
        if (string.IsNullOrWhiteSpace(str)) {
            return AvaloniaProperty.UnsetValue;
        }

        if (int.TryParse(str, NumberStyles.HexNumber, culture, out int x)) {
            return (decimal)x;
        }

        return AvaloniaProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try {
            if (value is decimal d) {
                return $"{(long)d:X8}";
            }

            return $"Invalid type: {value.GetType()}";
        } catch (Exception ex) {
            return ex.Message;
        }
    }
}
