namespace SceneGate.UI.Formats.Texts;

using Yarhl.Media.Text;

/// <summary>
/// Design test data for <see cref="PoViewModel"/>.
/// </summary>
public sealed class DesignPoViewModel : PoViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DesignPoViewModel"/> class.
    /// </summary>
    public DesignPoViewModel()
        : base(CreateTestModel())
    {
    }

    private static Po CreateTestModel()
    {
        var po = new Po(new PoHeader("ProjectId", "admin@email.com", "es-ES"));
        po.Add(new PoEntry("Entry 1") { Context = "id:1", Translated = "Entrada 1" });
        po.Add(new PoEntry("Line 1\nLine2") {
            Context = "id:2",
            Translated = "Línea 1\nLínea 2",
            ExtractedComments = "This is an extracted comment",
            Flags = "max-size:10, language-c",
            Reference = "block:4,header:CA,FE,C0,C0",
            TranslatorComment = "The translator does not agree",
        });
        return po;
    }
}
