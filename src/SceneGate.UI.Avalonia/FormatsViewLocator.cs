namespace SceneGate.UI;

using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SceneGate.UI.Formats;

public class FormatsViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        if (data is null) {
            return new TextBlock { Text = "Null view model" };
        }

        string name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null) {
            return (Control)Activator.CreateInstance(type)!;
        } else {
            return new TextBlock { Text = "Not Found: " + name };
        }
    }

    public bool Match(object? data)
    {
        // This view locator is only for views of the IFormatViewModel VM.
        return data is IFormatViewModel;
    }
}
