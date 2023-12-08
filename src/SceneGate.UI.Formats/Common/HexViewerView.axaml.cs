namespace SceneGate.UI.Formats.Common;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

/// <summary>
/// Hexadecimal viewer for binary content.
/// </summary>
public partial class HexViewerView : UserControl
{
    private readonly int lineHeight;

    /// <summary>
    /// Initializes a new instance of the <see cref="HexViewerView"/> class.
    /// </summary>
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
        ViewModel.VisibleTextRows = (int)(e.NewSize.Height / lineHeight) - 1;
    }

    private void ViewsPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.Delta.X != 0) {
            return;
        }

        ViewModel.CurrentScroll += (e.Delta.Y > 0) ? -1 : 1;
    }

    private void ViewsKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down) {
            // Should work with ascii or hex cursors as they are synched
            int asciiCharsPerLine = HexViewerViewModel.BytesPerRow * 2;
            int y = ViewModel.AsciiCursorPos / asciiCharsPerLine;
            if (y + 1 >= ViewModel.VisibleTextRows) {
                ViewModel.CurrentScroll++;
            }
        } else if (e.Key == Key.Up) {
            int asciiCharsPerLine = HexViewerViewModel.BytesPerRow * 2;
            int y = ViewModel.AsciiCursorPos / asciiCharsPerLine;
            if (y == 0) {
                ViewModel.CurrentScroll--;
            }
        }
    }

    private void ViewsKeyUp(object? sender, KeyEventArgs e)
    {
        // PageDown and PageUp doesn't work with KeyDown event :/
        if (e.Key == Key.PageDown) {
            ViewModel.CurrentScroll += ViewModel.VisibleTextRows - 2;
        } else if (e.Key == Key.PageUp) {
            ViewModel.CurrentScroll -= ViewModel.VisibleTextRows - 2;
        }
    }
}
