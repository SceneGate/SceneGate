// From: https://github.com/AvaloniaUI/AvaloniaEdit/wiki/MVVM#create-behaviour
namespace SceneGate.UI.Formats.Controls;

using System;
using Avalonia;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;

/// <summary>
/// UI behavior to implement binding with the text property in AvaloniaEdit.
/// </summary>
public class DocumentTextBindingBehavior : Behavior<TextEditor>
{
    private TextEditor? _textEditor = null;

    /// <summary>
    /// Binding with the text property.
    /// </summary>
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<DocumentTextBindingBehavior, string>(nameof(Text));

    /// <summary>
    /// Gets or sets the text property through binding.
    /// </summary>
    public string Text {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Attach to the text property.
    /// </summary>
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is TextEditor textEditor) {
            _textEditor = textEditor;
            _textEditor.TextChanged += TextChanged;
            this.GetObservable(TextProperty).Subscribe(TextPropertyChanged);
        }
    }

    /// <summary>
    /// Deattach the text property.
    /// </summary>
    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (_textEditor != null) {
            _textEditor.TextChanged -= TextChanged;
        }
    }

    private void TextChanged(object? sender, EventArgs eventArgs)
    {
        if (_textEditor != null && _textEditor.Document != null) {
            Text = _textEditor.Document.Text;
        }
    }

    private void TextPropertyChanged(string text)
    {
        if (_textEditor != null && _textEditor.Document != null && text != null) {
            var caretOffset = _textEditor.CaretOffset;
            _textEditor.Document.Text = text;
            _textEditor.CaretOffset = caretOffset;
        }
    }
}
