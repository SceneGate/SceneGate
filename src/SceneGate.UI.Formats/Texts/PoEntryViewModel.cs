namespace SceneGate.UI.Formats.Texts;

using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Yarhl.Media.Text;

public partial class PoEntryViewModel : ObservableObject
{
    private readonly PoEntry entry;

    [ObservableProperty]
    private string context;

    [ObservableProperty]
    private string original;

    [ObservableProperty]
    private string translated;

    [ObservableProperty]
    private string flags;

    [ObservableProperty]
    private bool isFuzzy;

    [ObservableProperty]
    private string extractedComments;

    public PoEntryViewModel(PoEntry entry)
    {
        this.entry = entry;

        Context = entry.Context;
        Original = entry.Original;
        Translated = entry.Translated;
        Flags = entry.Flags;
        IsFuzzy = entry.Flags.Contains("fuzzy");
        ExtractedComments = entry.ExtractedComments;
    }

    partial void OnIsFuzzyChanged(bool value)
    {
        if (IsFuzzy && !entry.Flags.Contains("fuzzy")) {
            entry.Flags += ",fuzzy";
        } else if (!IsFuzzy && entry.Flags.Contains("fuzzy")) {
            entry.Flags = string.Join(
                ',',
                entry.Flags.Split(',').Where(x => x != "fuzzy"));
        }

        Flags = entry.Flags;
    }

    partial void OnTranslatedChanged(string value)
    {
        entry.Translated = value;
    }
}
