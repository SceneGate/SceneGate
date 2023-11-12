namespace SceneGate.UI.Pages;
using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using SceneGate.UI.ControlsData;

public class NodeFormatToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is NodeFormatKind formatKind) {
            return formatKind switch {
                _ => new SymbolIconSource() { Symbol = Symbol.SyncFolder },
            };
        }

        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
