namespace SceneGate.UI.ControlsData;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Yarhl.FileFormat;

public sealed partial class TreeGridConverter : ObservableObject
{
    [ObservableProperty]
    private bool isCompatible;

    private TreeGridConverter(ConverterMetadata converter)
    {
        Converter = converter;
        Children = new ObservableCollection<TreeGridConverter>();
        Id = converter.Type.FullName!;
        DisplayName = converter.Type.Name;
        Kind = TreeGridConverterKind.Converter;
        IsCompatible = true;
    }

    private TreeGridConverter(AssemblyName assembly)
    {
        Children = new ObservableCollection<TreeGridConverter>();
        Id = assembly.FullName!;
        DisplayName = $"{assembly.Name} (v{assembly.Version})";
        Kind = TreeGridConverterKind.Assesmbly;
        IsCompatible = true;
    }

    private TreeGridConverter(string namespaceName)
    {
        Children = new ObservableCollection<TreeGridConverter>();
        Id = namespaceName;
        DisplayName = namespaceName;
        Kind = TreeGridConverterKind.Namespace;
        IsCompatible = true;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public TreeGridConverterKind Kind { get; }

    public ConverterMetadata? Converter { get; }

    public ObservableCollection<TreeGridConverter> Children { get; }

    public static void InsertConverterHierarchy(ConverterMetadata converter, ICollection<TreeGridConverter> list)
    {
        var assembly = new AssemblyName(converter.Type.Assembly.FullName!);
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

    public void UpdateVisibility(string? nameFilter, Type? sourceType)
    {
        if (Converter is not null) {
            bool matchingName = nameFilter is null
                || Converter.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase);

            bool compatibleType = sourceType is null || Converter.CanConvert(sourceType);

            IsCompatible = matchingName && compatibleType;
        } else {
            foreach (TreeGridConverter child in Children) {
                child.UpdateVisibility(nameFilter, sourceType);
            }

            IsCompatible = Children.Any(c => c.IsCompatible);
        }
    }

    public TreeGridConverter? SearchConverter(ConverterMetadata converter)
    {
        if (Converter == converter) {
            return this;
        }

        foreach (TreeGridConverter node in Children) {
            TreeGridConverter? found = node.SearchConverter(converter);
            if (found is not null) {
                return found;
            }
        }

        return null;
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
