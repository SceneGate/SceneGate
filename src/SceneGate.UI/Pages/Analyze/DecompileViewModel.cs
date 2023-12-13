namespace SceneGate.UI.Pages.Analyze;

using System;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.TypeSystem;

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
    private string assemblyLocation;

    public DecompileViewModel(Type targetType)
    {
        ArgumentNullException.ThrowIfNull(targetType);

        repositoryName = "Cannot determine";
        repositoryUrl = string.Empty;
        decompiledType = string.Empty;

        TypeName = targetType.FullName ?? "Unknown";
        AssemblyName = targetType.Assembly.FullName ?? "Unknown";
        AssemblyLocation = targetType.Assembly.Location;

        GetRepositoryInfo(targetType);
        DecompileType(targetType);
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
            var typeName = new FullTypeName(type.FullName);
            DecompiledType = decompiler.DecompileTypeAsString(typeName);
        } catch (Exception ex) {
            DecompiledType = ex.ToString();
        }
    }
}
