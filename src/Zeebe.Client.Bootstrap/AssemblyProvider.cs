using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap
{
    public class AssemblyProvider : IAssemblyProvider
    {
        private readonly string[] assembliesStartsWith;
        private List<Assembly> assemblies;

        public AssemblyProvider(params string[] assembliesStartsWith)
        {            
            this.assembliesStartsWith = assembliesStartsWith ?? new string[] { };
        }

        public IEnumerable<Assembly> Assemblies         
        {
            get
            {
                if(this.assemblies != null)
                    return this.assemblies;

                this.assemblies = GetAssemblies(this.assembliesStartsWith, AppDomain.CurrentDomain.BaseDirectory);                    
                return this.assemblies;
            }
        }

        private static List<Assembly> GetAssemblies(string[] assembliesStartsWith, string appDllsDirectory) {
            var loadedAssemblies = new List<Assembly>();
            var assembliesToBeLoaded = GetAssemblyPaths(assembliesStartsWith, appDllsDirectory);

            foreach (string path in assembliesToBeLoaded)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(path);
                    loadedAssemblies.Add(assembly);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return loadedAssemblies;
        }

        private static string[] GetAssemblyPaths(string[] assembliesStartsWith, string appDllsDirectory)
        {

            var paths = GetAllAssemblyPaths(appDllsDirectory);
            if (assembliesStartsWith.Length == 0)
                return paths;

            return paths
                .Where(path => assembliesStartsWith.Any(startsWith => AssemblyFromPathStartsWith(path, startsWith)))
                .ToArray();
        }

        private static bool AssemblyFromPathStartsWith(string path, string startsWith)
        {
            return Path.GetFileName(path).StartsWith(startsWith);
        }

        private static string[] GetAllAssemblyPaths(string appDllsDirectory)
        {
            var assembliesToBeLoaded = new List<string>();
            var assemblyPaths = Directory.GetFiles(appDllsDirectory, "*.dll");
            return assemblyPaths;
        }
    }
}