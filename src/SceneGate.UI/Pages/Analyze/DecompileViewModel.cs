namespace SceneGate.UI.Pages.Analyze;

using System;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.TypeSystem;
using System.Diagnostics.CodeAnalysis;

public partial class DecompileViewModel : ViewModelBase
{
    [ObservableProperty]
    private string repositoryName;

    [ObservableProperty]
    private string repositoryUrl;

    [ObservableProperty]
    private string decompiledType;

    [ObservableProperty]
    private string typeName;

    [ObservableProperty]
    private string assemblyName;

    [ObservableProperty]
    private string assemblyVersion;

    [ObservableProperty]
    private string assemblyCopyright;

    [ObservableProperty]
    private string assemblyLocation;

    public DecompileViewModel(Type targetType)
    {
        ArgumentNullException.ThrowIfNull(targetType);

        repositoryName = "Cannot determine";
        repositoryUrl = string.Empty;
        decompiledType = string.Empty;

        TypeName = targetType.FullName ?? "Unknown";

        GetAssemblyInfo(targetType);
        GetRepositoryInfo(targetType);
        DecompileType(targetType);
    }

#pragma warning disable MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
    [MemberNotNull(nameof(assemblyName), nameof(assemblyVersion))]
    [MemberNotNull(nameof(assemblyLocation), nameof(assemblyCopyright))]
#pragma warning restore MVVMTK0034
    private void GetAssemblyInfo(Type type)
    {
        Assembly assembly = type.Assembly;
        var name = new AssemblyName(assembly.FullName!);
        AssemblyName = name.Name ?? "Unknown";

        AssemblyLocation = assembly.Location;

        var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
        AssemblyCopyright = copyright?.Copyright ?? "(not set)";

        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (version is not null) {
            AssemblyVersion = version.InformationalVersion;
            if (AssemblyVersion.Contains('+')) {
                AssemblyVersion = AssemblyVersion[..AssemblyVersion.IndexOf('+')];
            }
        } else {
            AssemblyVersion = name.Version?.ToString() ?? "(not set)";
        }
    }

    private void GetRepositoryInfo(Type type)
    {
        Assembly assembly = type.Assembly;

        var assemblyMetadata = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
        var repositoryInfo = assemblyMetadata.FirstOrDefault(x => x.Key == "RepositoryUrl");
        if (repositoryInfo is not null) {
            RepositoryUrl = repositoryInfo.Value ?? string.Empty;
            RepositoryName = new Uri(RepositoryUrl).AbsolutePath;
            if (RepositoryName.Length > 0) {
                RepositoryName = RepositoryName[1..]; // remove leading '/'
            }
        }
    }

    private void DecompileType(Type type)
    {
        string assemblyPath = type.Assembly.Location;
        var decompiler = new CSharpDecompiler(assemblyPath, new DecompilerSettings());
        try {
            var ilspyTypeName = new FullTypeName(type.FullName);
            DecompiledType = decompiler.DecompileTypeAsString(ilspyTypeName);
        } catch (Exception ex) {
            DecompiledType = ex.ToString();
        }
    }
}
