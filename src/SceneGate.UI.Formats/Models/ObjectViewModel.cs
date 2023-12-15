namespace SceneGate.UI.Formats.Models;

using System;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Yarhl.IO;
using Yarhl.Media.Text;

/// <summary>
/// View model to display any kind of .NET object.
/// </summary>
public partial class ObjectViewModel : ObservableObject, IFormatViewModel
{
    [ObservableProperty]
    private object? model;

    [ObservableProperty]
    private string? jsonText;

    [ObservableProperty]
    private string? yamlText;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectViewModel"/> class.
    /// </summary>
    public ObjectViewModel()
        : this(new PoHeader())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectViewModel" /> class.
    /// </summary>
    public ObjectViewModel(object model)
    {
        Model = model;
        SetYamlText(model);
        SetJsonText(model);
    }

    private void SetYamlText(object model)
    {
        var yamlSerializer = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.Preserve)
            .IgnoreFields()
            .WithIndentedSequences()
            .Build();

        try {
            YamlText = yamlSerializer.Serialize(model);
        } catch (Exception ex) {
            YamlText = $"Problem serializing as YAML:\n{ex}";
        }
    }

    private void SetJsonText(object model)
    {
        var jsonSerializerOptions = new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };

        try {
            JsonText = JsonSerializer.Serialize(model, jsonSerializerOptions);
        } catch (Exception ex) {
            JsonText = $"Problem serializing as JSON:\n{ex}";
        }
    }
}
