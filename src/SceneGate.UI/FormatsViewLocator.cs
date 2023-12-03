namespace SceneGate.UI;

using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SceneGate.UI.Formats;

/// <summary>
/// Locator of Avalonia views given the view model to present by using MVVM conventions.
/// </summary>
/// <remarks>
/// Following Avalonia design, views are found automatically by the framework
/// by using IDataTemplate, when the "Content" property of a control is not a view.
/// It follows the tree upto finding the registered IDataTemplates (in App.axaml)
/// then calls Match to see if it can use that IDataTemplate to find the view,
/// if so, it calls Build to get the view.
/// This ViewLocator is based on the MVVM conventions. It finds the view which
/// type name is the same as the ViewModel but with View instead.
/// </remarks>
public class FormatsViewLocator : IDataTemplate
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
        // This view locator is only for views of the IFormatViewModel VM.
        return data is IFormatViewModel;
    }
}
