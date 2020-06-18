using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arity
{
    public class DirectoryAssemblyCatalog : IAssemblyCatalog
    {
        private const string ViewsAssemblySuffix = ".Views.dll";

        private readonly IOptions<DirectoryAssemblyCatalogOptions> _options;

        private readonly ILogger<DirectoryAssemblyCatalog> _logger;

        private readonly Lazy<ICollection<Assembly>> _factory;

        public DirectoryAssemblyCatalog(IOptions<DirectoryAssemblyCatalogOptions> options, ILogger<DirectoryAssemblyCatalog> logger)
        {
            _options = options;
            _logger = logger;

            _factory = new Lazy<ICollection<Assembly>>(EnumerateAssemblies);
        }

        private static void EnsureDirectoryExists([NotNull] string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"{path} does not exist.");
            }
        }

        private class AssemblyByLocationComparer : IEqualityComparer<Assembly>
        {
            public bool Equals(Assembly x, Assembly y)
            {
                return x != null && y != null && x.Location == y.Location;
            }

            public int GetHashCode(Assembly obj)
            {
                return obj.Location.GetHashCode();
            }
        }

        private ICollection<Assembly> EnumerateAssemblies()
        {
            var catalogOptions = _options;
            var path = catalogOptions.Value.BasePath;

            string[] patterns = catalogOptions.Value.Patterns ?? throw new ArgumentNullException(nameof(patterns));

            EnsureDirectoryExists(path);

            var directory = new DirectoryInfo(path);

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.IsDynamic == false)
                .Distinct(new AssemblyByLocationComparer())
                .ToDictionary(x => x.Location);

            var assemblies = new List<Assembly>();

            var assemblyFileNames = patterns.SelectMany(x => directory.GetFiles(x)).Select(x => x.FullName);

            foreach (var assemblyFileName in assemblyFileNames)
            {
                if (loadedAssemblies.ContainsKey(assemblyFileName))
                {
                    assemblies.Add(loadedAssemblies[assemblyFileName]);
                }
                else
                {
                    try
                    {
                        var assemblyName = AssemblyName.GetAssemblyName(assemblyFileName);
                        var assembly = AppDomain.CurrentDomain.Load(assemblyName);
                        assemblies.Add(assembly);
                    }
                    catch (BadImageFormatException)
                    {
                        _logger?.LogDebug("Can't load assembly {assembly}.", assemblyFileName);
                    }
                    catch (FileNotFoundException)
                    {
                        if (assemblyFileName.EndsWith(ViewsAssemblySuffix))
                        {
                            continue;
                        }

                        throw;
                    }
                }
            }

            return assemblies;
        }

        public ICollection<Assembly> GetAssemblies()
        {
            return _factory.Value;
        }
    }
}
