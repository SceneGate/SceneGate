namespace SceneGate.UI.Formats.Common;

using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

public partial class HexViewerView : UserControl
{
    private readonly int lineHeight;

    public HexViewerView()
    {
        InitializeComponent();

        var fontTypeFace = new Typeface(hexView.FontFamily);
        if (FontManager.Current.TryGetGlyphTypeface(fontTypeFace, out var glyphTypeface)) {
            double lineSpacingEm = (double)glyphTypeface.Metrics.LineSpacing / glyphTypeface.Metrics.DesignEmHeight;
            lineHeight = (int)(hexView.FontSize * lineSpacingEm);
        } else {
            lineHeight = (int)hexView.FontSize;
        }
    }

    private HexViewerViewModel ViewModel => (DataContext as HexViewerViewModel)!;

    private void HexViewSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        ViewModel.VisibleTextRows = (int)(e.NewSize.Height / lineHeight);
    }
}
