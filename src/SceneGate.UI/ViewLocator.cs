namespace SceneGate.UI;
using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SceneGate.UI.Pages;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        if (data is null) {
            return new TextBlock { Text = "Null view model" };
        }

        Type dataType = data.GetType();
        string assemblyName = dataType.Assembly.FullName!;
        string typeName = dataType.FullName!.Replace("ViewModel", "View");
        string qualifiedName = $"{typeName}, {assemblyName}";
        var type = Type.GetType(qualifiedName);

        if (type != null) {
            return (Control)Activator.CreateInstance(type)!;
        } else {
            return new TextBlock { Text = "Not Found: " + qualifiedName };
        }
    }

    public bool Match(object? data)
    {
        // This view locator is only for views of the main user interface.
        return data is ViewModelBase;
    }
}
