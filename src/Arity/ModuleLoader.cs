using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Arity
{
    public class ModuleLoader
    {
        private readonly ICollection<ModuleMetadataValidator> _validators;

        public ModuleLoader(IEnumerable<ModuleMetadataValidator> validators)
        {
            _validators = validators?.ToArray() ?? Array.Empty<ModuleMetadataValidator>();
        }

        public ICollection<ModuleMetadata> GetSortedModules(ICollection<Assembly> assemblies,
            ICollection<string> entryModules)
        {
            if (entryModules == null || entryModules.Count == 0)
                throw new ArgumentNullException(nameof(entryModules));

            ModuleMetadata startModuleMetadata = null;
            string entryModule;

            if (entryModules.Count > 1)
            {
                startModuleMetadata = new ModuleMetadata(typeof(EntryModule), nameof(EntryModule),
                    entryModules.ToArray(), new Dictionary<string, object>());

                entryModule = startModuleMetadata.Name;
            }
            else
            {
                entryModule = entryModules.First();
            }

            var moduleMetadataCollection = new List<ModuleMetadata>();

            var names = new HashSet<string>();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var modules = assembly.GetTypesWithAttribute<ModuleMetadataAttribute>(true);

                    foreach (var module in modules)
                    {
                        var metadata = new ModuleMetadata(module.Type);

                        foreach (var moduleAttribute in module.Attributes)
                        {
                            moduleAttribute.Apply(metadata);
                        }

                        if (metadata.Name == null)
                        {
                            throw new InvalidOperationException(
                                $"Failed to resolve module name for module type {module.Type.FullName}");
                        }

                        if (names.Contains(metadata.Name))
                        {
                            throw new InvalidOperationException(
                                $"Module with name \"{metadata.Name}\" already exists.");
                        }

                        names.Add(metadata.Name);

                        if (startModuleMetadata == null && entryModule == metadata.Name)
                        {
                            startModuleMetadata = metadata;
                        }

                        moduleMetadataCollection.Add(metadata);
                    }
                }
                catch (ReflectionTypeLoadException e)
                {
                    var messages = string.Join(Environment.NewLine, e.LoaderExceptions.Select(x => x.ToString()));

                    Console.WriteLine("Failed to load module: {0}", messages);

                    throw;
                }
            }

            if (startModuleMetadata == null)
            {
                throw new InvalidOperationException("Can't detect main application module.");
            }

            var visitor = new SortingModuleMetadataVisitor(moduleMetadataCollection);

            var moduleDescriptors = visitor.Visit(startModuleMetadata);

            var sortedModules = new HashSet<ModuleMetadata>(moduleDescriptors, ModuleMetadata.ByName);

            if (_validators.Count > 0)
            {
                var errors = new List<string>();

                var modulesMap = sortedModules.ToDictionary(x => x.Name);

                foreach (var descriptor in sortedModules)
                {
                    foreach (var validator in _validators)
                    {
                        validator.Validate(descriptor, modulesMap, errors);
                    }
                }

                if (errors.Count > 0)
                {
                    throw new InvalidOperationException("Error on module loading: " + Environment.NewLine +
                                                        string.Join(Environment.NewLine, errors));
                }
            }

            return sortedModules;
        }

        private class SortingModuleMetadataVisitor
        {
            private readonly IDictionary<string, ModuleMetadata> _moduleDescriptors;

            public SortingModuleMetadataVisitor(ICollection<ModuleMetadata> descriptors)
            {
                _moduleDescriptors = descriptors.ToDictionary(x => x.Name);
            }

            public ICollection<ModuleMetadata> Visit(ModuleMetadata moduleMetadata)
            {
                var sorted = new List<ModuleMetadata>();

                var visited = new Dictionary<string, bool>();

                VisitInternal(sorted, visited, moduleMetadata);

                return sorted;
            }

            private void VisitInternal(List<ModuleMetadata> result, IDictionary<string, bool> visited,
                ModuleMetadata metadata)
            {
                if (metadata == null)
                {
                    throw new ArgumentNullException(nameof(metadata));
                }

                var alreadyVisited = visited.TryGetValue(metadata.Name, out var inProcess);

                if (alreadyVisited)
                {
                    if (inProcess)
                    {
                        throw new ArgumentException($"Cyclic dependency found: {metadata.Name}.");
                    }
                }
                else
                {
                    visited[metadata.Name] = true;

                    var dependencies = metadata.Dependencies ?? Array.Empty<string>();

                    foreach (var dependency in dependencies)
                    {
                        if (_moduleDescriptors.ContainsKey(dependency))
                        {
                            VisitInternal(result, visited, _moduleDescriptors[dependency]);
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"Dependency {dependency} not found for {metadata.Name}.");
                        }
                    }

                    visited[metadata.Name] = false;

                    result.Add(metadata);
                }
            }
        }

        private class EntryModule
        {
        }
    }
}
