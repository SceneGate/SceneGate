// Copyright (c) 2021 SceneGate

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Text.Json;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Yarhl.FileFormat;

namespace SceneGate.UI.Formats.Common
{
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
        private IFormat format;
        private string text;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectViewModel" /> class.
        /// </summary>
        public ObjectViewModel()
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
        public IFormat Format {
            get => format;
            set => SetProperty(ref format, value);
        }

        /// <inheritdoc/>
        public bool CanShow(IFormat format)
        {
            return true;
        }

        /// <inheritdoc/>
        public void Show(IFormat format)
        {
            Format = format;
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
}
