namespace SceneGate.UI.Formats.Texts;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Yarhl.Media.Text;

/// <summary>
/// View model to represent a <see cref="Yarhl.Media.Text.Po" /> format.
/// </summary>
public partial class PoViewModel : ObservableObject, IFormatViewModel
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PasteOriginalTranslationCommand))]
    private PoEntryViewModel? selectedEntry;

    /// <summary>
    /// Initializes a new instance of the <see cref="PoViewModel"/> class with
    /// test content.
    /// </summary>
    public PoViewModel()
    {
        Entries = new ObservableCollection<PoEntryViewModel>();
        Header = new ObservableCollection<PoHeaderProperty>();

        var testModel = new Po(new PoHeader("ProjectId", "admin@email.com", "es-ES"));
        testModel.Add(new PoEntry("Entry 1") { Context = "id:1", Translated = "Entrada 1" });
        testModel.Add(new PoEntry("Line 1\nLine2") {
            Context = "id:2",
            Translated = "Línea 1\nLínea 2",
            ExtractedComments = "This is an extracted comment",
            Flags = "max-size:10, language-c",
            Reference = "block:4,header:CA,FE,C0,C0",
            TranslatorComment = "The translator does not agree",
        });
        Show(testModel);

        SelectedEntry = Entries[1];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PoViewModel" /> class.
    /// </summary>
    /// <param name="model">The model to load.</param>
    public PoViewModel(Po model)
    {
        Entries = new ObservableCollection<PoEntryViewModel>();
        Header = new ObservableCollection<PoHeaderProperty>();

        Show(model);

        if (Entries.Count > 0) {
            SelectedEntry = Entries[0];
        }
    }

    /// <summary>
    /// Gets the collection of entries in the PO format.
    /// </summary>
    public ObservableCollection<PoEntryViewModel> Entries { get; }

    /// <summary>
    /// Gets the collection of properties in the PO header.
    /// </summary>
    public ObservableCollection<PoHeaderProperty> Header { get; }

    [RelayCommand(CanExecute = nameof(CanPasteOriginalTranslation))]
    private void PasteOriginalTranslation()
    {
        if (SelectedEntry is null) {
            return;
        }

        SelectedEntry.Translated = SelectedEntry.Original;
        OnPropertyChanged(nameof(SelectedEntry.Translated));
    }

    private bool CanPasteOriginalTranslation()
    {
        return SelectedEntry is not null;
    }

    private void Show(Po po)
    {
        Header.Add(new PoHeaderProperty("Project ID", po.Header.ProjectIdVersion));
        Header.Add(new PoHeaderProperty("Report bugs to", po.Header.ReportMsgidBugsTo));
        Header.Add(new PoHeaderProperty("Language", po.Header.Language));
        Header.Add(new PoHeaderProperty("Team", po.Header.LanguageTeam));
        Header.Add(new PoHeaderProperty("Creation date", po.Header.CreationDate));
        Header.Add(new PoHeaderProperty("Revision date", po.Header.RevisionDate));
        Header.Add(new PoHeaderProperty("Last translator", po.Header.LastTranslator));
        Header.Add(new PoHeaderProperty("Plural forms", po.Header.PluralForms));

        foreach (KeyValuePair<string, string> ext in po.Header.Extensions) {
            Header.Add(new PoHeaderProperty("X-" + ext.Key, ext.Value));
        }

        foreach (PoEntry entry in po.Entries) {
            Entries.Add(new PoEntryViewModel(entry));
        }
    }
}
