namespace SceneGate.UI.Pages.Main;

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Yarhl.FileSystem;

public partial class ConversionErrorViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? nodePath;

    [ObservableProperty]
    private string? formatName;

    [ObservableProperty]
    private string? converterName;

    [ObservableProperty]
    private object? conversionParameters;

    [ObservableProperty]
    private bool hasParameters;

    [ObservableProperty]
    private string? exceptionMessage;

    public ConversionErrorViewModel()
    {
    }

    public ConversionErrorViewModel(Node node, Type converterType, object? conversionParmeters, Exception exception)
    {
        NodePath = node.Path;
        FormatName = node.Format?.GetType().FullName ?? "NULL format";
        ConverterName = converterType.FullName!;
        ConversionParameters = conversionParmeters;
        HasParameters = conversionParmeters is not null;
        ExceptionMessage = exception.ToString();
    }
}
