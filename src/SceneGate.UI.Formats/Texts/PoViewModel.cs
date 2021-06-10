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
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Yarhl.FileFormat;
using Yarhl.Media.Text;

namespace SceneGate.UI.Formats.Texts
{
    /// <summary>
    /// View model to represent a <see cref="Yarhl.Media.Text.Po" /> format.
    /// </summary>
    public class PoViewModel : ObservableObject, IFormatViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PoViewModel" /> class.
        /// </summary>
        public PoViewModel()
        {
            Entries = new ObservableCollection<PoEntry>();
            Header = new ObservableCollection<HeaderProperty>();
        }

        /// <summary>
        /// Gets the collection of entries in the PO format.
        /// </summary>
        public ObservableCollection<PoEntry> Entries { get; }

        /// <summary>
        /// Gets the collection of properties in the PO header.
        /// </summary>
        public ObservableCollection<HeaderProperty> Header { get; }

        /// <inheritdoc/>
        public bool CanShow(IFormat format) => format is Po;

        /// <inheritdoc/>
        public void Show(IFormat format)
        {
            if (format is not Po po) {
                return;
            }

            Header.Clear();
            Header.Add(new HeaderProperty("Project ID", po.Header.ProjectIdVersion));
            Header.Add(new HeaderProperty("Report bugs to", po.Header.ReportMsgidBugsTo));
            Header.Add(new HeaderProperty("Language", po.Header.Language));
            Header.Add(new HeaderProperty("Team", po.Header.LanguageTeam));
            Header.Add(new HeaderProperty("Creation date", po.Header.CreationDate));
            Header.Add(new HeaderProperty("Revision date", po.Header.RevisionDate));
            Header.Add(new HeaderProperty("Last translator", po.Header.LastTranslator));
            Header.Add(new HeaderProperty("Plural forms", po.Header.PluralForms));

            foreach (var ext in po.Header.Extensions) {
                Header.Add(new HeaderProperty("X-" + ext.Key, ext.Value));
            }

            Entries.Clear();
            foreach (var entry in po.Entries) {
                Entries.Add(entry);
            }
        }

        /// <summary>
        /// Property of the PO header.
        /// </summary>
        [SuppressMessage("", "SA1313", Justification = "Record properties in camel case")]
        public record HeaderProperty(string Key, string Value);
    }
}
