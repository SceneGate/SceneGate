namespace SceneGate.UI.Plugins;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using SceneGate.UI.Formats;
using Yarhl.FileFormat;

/// <summary>
/// Locate implementations of SceneGate plugin types.
/// </summary>
/// <remarks>
/// It scans all the assemblies in the executable directory. Then it loads them
/// and finds the interface IFormatViewModelBuilder which is used to creates
/// view models.
/// This is a simplified way to implement plugins that doesn't require IoC.
/// NOTE that SECURITY is not in the design. Any assembly in the executable
/// directory will be load and run.
/// </remarks>
public sealed class PluginsLocator : IFormatsViewModelLocator, IFormatConvertersLocator
{
    private static readonly string[] IgnoredLibraries = {
        "System.",
        "Microsoft.",
        "netstandard",
        "nuget",
        "Avalonia",
    };

    private static readonly object LockObj = new object();
    private static PluginsLocator? singleInstance;

    private readonly List<IFormatViewModelBuilder> viewModelBuilders;
    private readonly List<ConverterMetadata> convertersMetadata;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginsLocator" /> class.
    /// </summary>
    private PluginsLocator()
    {
        viewModelBuilders = new List<IFormatViewModelBuilder>();
        convertersMetadata = new List<ConverterMetadata>();
        ViewModelBuilders = viewModelBuilders;
        ConvertersMetadata = convertersMetadata;

        InitializeContainer();
    }

    /// <summary>
    /// Gets the name of the plugins directory.
    /// </summary>
    public static string PluginDirectory => "Plugins";

    /// <summary>
    /// Gets the plugin manager instance.
    /// </summary>
    /// <remarks><para>Lazy initialization.</para></remarks>
    public static PluginsLocator Instance {
        get {
            if (singleInstance is null) {
                lock (LockObj) {
                    singleInstance ??= new PluginsLocator();
                }
            }

            return singleInstance;
        }
    }

    /// <summary>
    /// Gets the collection of instances for detecting and building format
    /// view models.
    /// </summary>
    public IReadOnlyList<IFormatViewModelBuilder> ViewModelBuilders { get; }

    public IReadOnlyList<ConverterMetadata> ConvertersMetadata { get; }

    private static IEnumerable<Assembly> FilterAndLoadAssemblies(IEnumerable<string> paths)
    {
        // Skip libraries that match the ignored libraries to prevent loading dependencies.
        paths = paths
            .Select(p => new { Name = Path.GetFileName(p), Path = p })
            .Where(p => !IgnoredLibraries.Any(
                ign => p.Name.StartsWith(ign, StringComparison.OrdinalIgnoreCase)))
            .Select(p => p.Path);
        return LoadAssemblies(paths);
    }

    private static IEnumerable<Assembly> LoadAssemblies(IEnumerable<string> paths)
    {
        foreach (string path in paths) {
            Assembly? assembly = null;
            try {
                assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            } catch (BadImageFormatException) {
                // native library most likely - ignore
            }

            if (assembly is not null) {
                yield return assembly;
            }
        }
    }

    private void InitializeContainer()
    {
        // Assemblies from the program directory (including this one).
        string programDir = AppDomain.CurrentDomain.BaseDirectory;
        string[] libraryAssemblies = Directory.GetFiles(programDir, "*.dll");
        ScanAssemblies(FilterAndLoadAssemblies(libraryAssemblies));

        string[] programAssembly = Directory.GetFiles(programDir, "*.exe");
        ScanAssemblies(FilterAndLoadAssemblies(programAssembly));

        // Assemblies from the Plugin directory and subfolders
        string pluginDir = Path.Combine(programDir, PluginDirectory);
        if (Directory.Exists(pluginDir)) {
            string[] pluginFiles = Directory.GetFiles(pluginDir, "*.dll", SearchOption.AllDirectories);
            ScanAssemblies(FilterAndLoadAssemblies(pluginFiles));
        }
    }

    private void ScanAssemblies(IEnumerable<Assembly> assemblies)
    {
        foreach (Assembly assembly in assemblies) {
            ScanAssemblyForViewModelBuilders(assembly);
            ScanAssemblyForConverters(assembly);
        }
    }

    private void ScanAssemblyForViewModelBuilders(Assembly assembly)
    {
        IEnumerable<Type> viewModelBuildersTypes = assembly.ExportedTypes
            .Where(t => typeof(IFormatViewModelBuilder).IsAssignableFrom(t))
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetConstructor(Type.EmptyTypes) is not null);
        foreach (Type type in viewModelBuildersTypes) {
            var instance = Activator.CreateInstance(type)! as IFormatViewModelBuilder;
            viewModelBuilders.Add(instance!);
        }
    }

    private void ScanAssemblyForConverters(Assembly assembly)
    {
        static bool ValidConverterInterface(Type interfaceType) =>
            interfaceType.IsGenericType
            && interfaceType.GetGenericTypeDefinition().IsEquivalentTo(typeof(IConverter<,>));

        IEnumerable <Type> converterTypes = assembly.ExportedTypes
            .Where(t => t.GetInterfaces().Any(ValidConverterInterface))
            .Where(t => t.IsClass && !t.IsAbstract);
        foreach (Type type in converterTypes) {
            // A converter class may implement the IConverter interface several
            // times (e.g. serialize and deserialize).
            var sourceTypes = new List<Type>();
            var destTypes = new List<Type>();
            IEnumerable<Type[]> converterInterfaces = type.GetInterfaces()
                .Where(ValidConverterInterface)
                .Select(i => i.GenericTypeArguments);
            foreach (Type[] genericTypes in converterInterfaces) {
                sourceTypes.Add(genericTypes[0]);
                destTypes.Add(genericTypes[1]);
            }

            var metadata = new ConverterMetadata {
                Name = type.FullName!,
                Type = type,
                InternalSources = sourceTypes.ToArray(),
                InternalDestinations = destTypes.ToArray(),
            };
            convertersMetadata.Add(metadata);
        }
    }
}
