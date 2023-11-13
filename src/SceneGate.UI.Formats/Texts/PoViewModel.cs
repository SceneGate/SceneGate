namespace SceneGate.UI.Formats.Texts;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Yarhl.FileFormat;
using Yarhl.Media.Text;

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
    public record HeaderProperty(string Key, string Value);
}
