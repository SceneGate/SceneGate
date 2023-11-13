namespace SceneGate.UI.Pages;
using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using SceneGate.UI.ControlsData;

public class TreeGridConverterKindToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TreeGridConverterKind kind) {
            return kind switch {
                TreeGridConverterKind.Assesmbly => Symbol.ZipFolder,
                TreeGridConverterKind.Namespace => Symbol.Code,
                TreeGridConverterKind.Converter => Symbol.Sync,
                _ => Symbol.Help,
            };
        }

        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
