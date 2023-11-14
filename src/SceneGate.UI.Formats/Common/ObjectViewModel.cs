namespace SceneGate.UI.Formats.Common;
using System;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/// <summary>
/// View model to display any kind of .NET object.
/// </summary>
public class ObjectViewModel : ObservableObject, IFormatViewModel
{
    private readonly ISerializer yamlSerializer;
    private readonly JsonSerializerOptions jsonSerializerOptions;
    private bool showYaml;
    private bool showJson;
    private bool showText;
    private bool showPropertyGrid;
    private object format;
    private string text;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectViewModel" /> class.
    /// </summary>
    public ObjectViewModel(object model)
    {
        ShowPropertyGrid = true;
        Text = string.Empty;

        yamlSerializer = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.Preserve)
            .IgnoreFields()
            .WithIndentedSequences()
            .Build();

        jsonSerializerOptions = new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };

        Format = model;
    }

    /// <summary>
    /// Gets or sets a value indicating whether it should show the object
    /// as YAML.
    /// </summary>
    public bool ShowYaml {
        get => showYaml;
        set {
            SetProperty(ref showYaml, value);
            ShowText = ShowJson || value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether it should show the object
    /// as JSON.
    /// </summary>
    public bool ShowJson {
        get => showJson;
        set {
            SetProperty(ref showJson, value);
            ShowText = ShowYaml || value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether it should show its text
    /// representation.
    /// </summary>
    public bool ShowText {
        get => showText;
        set {
            SetProperty(ref showText, value);
            UpdateText();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether it should show the object
    /// in a property grid.
    /// </summary>
    public bool ShowPropertyGrid {
        get => showPropertyGrid;
        set => SetProperty(ref showPropertyGrid, value);
    }

    /// <summary>
    /// Gets or sets the text representation of the object.
    /// </summary>
    public string Text {
        get => text;
        set => SetProperty(ref text, value);
    }

    /// <summary>
    /// Gets or sets the format object.
    /// </summary>
    public object Format {
        get => format;
        set => SetProperty(ref format, value);
    }

    private void UpdateText()
    {
        try {
            if (ShowYaml) {
                Text = yamlSerializer.Serialize(format);
            } else if (ShowJson) {
                Text = JsonSerializer.Serialize(format, format.GetType(), jsonSerializerOptions);
            }
        } catch (Exception ex) {
            Text = ex.ToString();
        }
    }
}
