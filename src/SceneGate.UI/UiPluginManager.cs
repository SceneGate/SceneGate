// Copyright (c) 2021 SceneGate

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using SceneGate.UI.Formats;
using Yarhl.FileFormat;

namespace SceneGate.UI
{
    public sealed class UiPluginManager
    {
       static readonly string[] IgnoredLibraries = {
            "System.",
            "Microsoft.",
            "netstandard",
            "nuget",
            "nunit",
            "testhost",
        };

        static readonly object LockObj = new object();
        static UiPluginManager singleInstance;

        CompositionHost container;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        UiPluginManager()
        {
            InitializeContainer();
        }

        /// <summary>
        /// Gets the name of the plugins directory.
        /// </summary>
        public static string PluginDirectory => "Plugins";

        /// <summary>
        /// Gets the plugin manager instance.
        /// </summary>
        /// <remarks><para>It initializes the manager if needed.</para></remarks>
        public static UiPluginManager Instance {
            get {
                if (singleInstance == null) {
                    lock (LockObj) {
                        if (singleInstance == null)
                            singleInstance = new UiPluginManager();
                    }
                }

                return singleInstance;
            }
        }

        public IEnumerable<BaseFormatView> GetFormatViews()
        {
            return container.GetExports<BaseFormatView>();
        }

        public IEnumerable<BaseFormatView> GetCompatibleView(IFormat format)
        {
            var views = GetFormatViews();
            return views.Where(v => v.ViewModel.CanShow(format));
        }

        static void DefineFormatViewsConventions(ConventionBuilder conventions)
        {
            conventions
                .ForTypesDerivedFrom<BaseFormatView>()
                .Export<BaseFormatView>()
                .SelectConstructor(ctors =>
                    ctors.OrderBy(ctor => ctor.GetParameters().Length)
                    .First());

            conventions
                .ForTypesDerivedFrom<IFormatViewModel>()
                .Export<IFormatViewModel>()
                .SelectConstructor(ctors =>
                    ctors.OrderBy(ctor => ctor.GetParameters().Length)
                    .First());
        }

        static IEnumerable<Assembly> FilterAndLoadAssemblies(IEnumerable<string> paths)
        {
            // Skip libraries that match the ignored libraries because
            // MEF would try to load its dependencies.
            return paths
                .Select(p => new { Name = Path.GetFileName(p), Path = p })
                .Where(p => !IgnoredLibraries.Any(
                    ign => p.Name.StartsWith(ign, StringComparison.OrdinalIgnoreCase)))
                .Select(p => p.Path)
                .Select(LoadAssemblies);
        }

       static Assembly LoadAssemblies(string path)
        {
            try {
                return AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            } catch (BadImageFormatException) {
                // Bad IL. Skip.
                return null;
            }
        }

        void InitializeContainer()
        {
            var conventions = new ConventionBuilder();
            DefineFormatViewsConventions(conventions);

            var containerConfig = new ContainerConfiguration()
                .WithDefaultConventions(conventions);

            // Assemblies from the program directory (including this one).
            var programDir = AppDomain.CurrentDomain.BaseDirectory;
            var libraryAssemblies = Directory.GetFiles(programDir, "*.dll");
            var programAssembly = Directory.GetFiles(programDir, "*.exe");
            containerConfig
                .WithAssemblies(FilterAndLoadAssemblies(libraryAssemblies))
                .WithAssemblies(FilterAndLoadAssemblies(programAssembly));

            // Assemblies from the Plugin directory and subfolders
            string pluginDir = Path.Combine(programDir, PluginDirectory);
            if (Directory.Exists(pluginDir)) {
                var pluginFiles = Directory.GetFiles(
                    pluginDir,
                    "*.dll",
                    SearchOption.AllDirectories);
                containerConfig.WithAssemblies(FilterAndLoadAssemblies(pluginFiles));
            }

            container = containerConfig.CreateContainer();
        }
    }
}
