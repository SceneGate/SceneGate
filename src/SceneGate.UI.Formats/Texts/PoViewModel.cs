namespace SceneGate.UI.Formats.Texts;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SceneGate.UI.Formats.Mvvm;
using Yarhl.IO;
using Yarhl.Media.Text;

/// <summary>
/// View model to represent a <see cref="Yarhl.Media.Text.Po" /> format.
/// </summary>
public partial class PoViewModel : ObservableObject, IFormatViewModel
{
    private readonly Po po;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PasteOriginalTranslationCommand))]
    private PoEntryViewModel? selectedEntry;

    /// <summary>
    /// Initializes a new instance of the <see cref="PoViewModel" /> class.
    /// </summary>
    /// <param name="model">The model to load.</param>
    public PoViewModel(Po model)
    {
        Entries = new ObservableCollection<PoEntryViewModel>();
        Header = new ObservableCollection<PoHeaderProperty>();
        AskOutputFile = new AsyncInteraction<IStorageFile?>();

        po = model;
        Show();

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

    /// <summary>
    /// Gets the interaction to ask the user for the output file.
    /// </summary>
    public AsyncInteraction<IStorageFile?> AskOutputFile { get; }

    /// <summary>
    /// Export the PO model into a text file on disk.
    /// </summary>
    /// <returns>Asynchrnous task.</returns>
    [RelayCommand]
    public async Task ExportAsync()
    {
        IStorageFile? file = await AskOutputFile.HandleAsync().ConfigureAwait(false);
        if (file is null) {
            return;
        }

        using BinaryFormat binary = new Po2Binary().Convert(po);

        using Stream output = await file.OpenWriteAsync().ConfigureAwait(false);
        binary.Stream.WriteTo(output);
    }

    /// <summary>
    /// Copy the source text as translation.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanPasteOriginalTranslation))]
    public void PasteOriginalTranslation()
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

    private void Show()
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
