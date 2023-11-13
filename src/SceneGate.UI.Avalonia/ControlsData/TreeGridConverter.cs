namespace SceneGate.UI.ControlsData;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Yarhl.FileFormat;

public sealed class TreeGridConverter
{
    private TreeGridConverter(ConverterMetadata converter)
    {
        Converter = converter;
        Children = new ObservableCollection<TreeGridConverter>();
        Id = converter.Type.FullName!;
        DisplayName = converter.Type.Name;
        Kind = TreeGridConverterKind.Converter;
    }

    private TreeGridConverter(AssemblyName assembly)
    {
        Children = new ObservableCollection<TreeGridConverter>();
        Id = assembly.FullName!;
        DisplayName = $"{assembly.Name} (v{assembly.Version})";
        Kind = TreeGridConverterKind.Assesmbly;
    }

    private TreeGridConverter(string namespaceName)
    {
        Children = new ObservableCollection<TreeGridConverter>();
        Id = namespaceName;
        DisplayName = namespaceName;
        Kind = TreeGridConverterKind.Namespace;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public TreeGridConverterKind Kind { get; }

    public ConverterMetadata? Converter { get; }

    public ObservableCollection<TreeGridConverter> Children { get; }

    public static void InsertConverterHierarchy(ConverterMetadata converter, ICollection<TreeGridConverter> list)
    {
        AssemblyName assembly = new AssemblyName(converter.Type.Assembly.FullName!);
        string assemblyName = assembly.Name!;
        string typeNamespace = converter.Type.Namespace!;
        string[] namespaceList = typeNamespace[assemblyName.Length..].Split('.', StringSplitOptions.RemoveEmptyEntries);

        TreeGridConverter? current = list.FirstOrDefault(x => x.Id == assembly.FullName);
        if (current is null) {
            current = new TreeGridConverter(assembly);
            list.Add(current);
        }

        foreach (string n in namespaceList) {
            current = current.GetOrAddChildNamespace(n);
        }

        if (!current.Children.Any(x => x.Converter == converter)) {
            current.Children.Add(new TreeGridConverter(converter));
        }
    }

    private TreeGridConverter GetOrAddChildNamespace(string name)
    {
        TreeGridConverter? child = Children.FirstOrDefault(x => x.DisplayName == name);
        if (child is null) {
            child = new TreeGridConverter(name);
            Children.Add(child);
            return child;
        }

        return child;
    }
}
