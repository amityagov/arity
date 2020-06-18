using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Arity
{
    public class Bootstrapper
    {
        private readonly ModuleLoader _moduleLoader;

        private readonly IServiceCollection _serviceCollection;
        private readonly IAssemblyCatalog _assemblyCatalog;
        private readonly BootstrapperOptions _options;
        private readonly string _entryModule;

        private readonly Type _lifecycleListenerType = typeof(IRegisterAssemblyTypesListener);

        public Bootstrapper(ModuleLoader moduleLoader, IServiceCollection serviceCollection,
            IAssemblyCatalog assemblyCatalog, BootstrapperOptions options)
        {
            var entryModule = options.EntryModule;

            if (entryModule == null)
                throw new ArgumentNullException(nameof(entryModule));

            _moduleLoader = moduleLoader;
            _serviceCollection = serviceCollection;
            _assemblyCatalog = assemblyCatalog;
            _options = options;
            _entryModule = entryModule;
        }

        public IServiceProvider Start()
        {
            var assemblies = _assemblyCatalog.GetAssemblies();

            var modules = _moduleLoader.GetSortedModules(assemblies, _entryModule);

            return StartInternal(_assemblyCatalog, modules);
        }

        private IServiceProvider StartInternal(IAssemblyCatalog assemblyCatalog, ICollection<ModuleMetadata> modules)
        {
            var buildTimeServicesCollection = new ServiceCollection();

            foreach (var action in _options.ConfigureBuildTimeServices)
            {
                action(buildTimeServicesCollection);
            }

            var lifecycleListenerTypes = LoadLifecycleListeners(modules.Select(x => x.Type.Assembly).Distinct().ToArray());

            foreach (var module in modules)
            {
                buildTimeServicesCollection.AddSingleton(module.Type);
            }

            foreach (var lifecycleListenerType in lifecycleListenerTypes)
            {
                buildTimeServicesCollection.AddSingleton(_lifecycleListenerType, lifecycleListenerType);
            }

            buildTimeServicesCollection.AddSingleton(assemblyCatalog);

            var preBuildServices = buildTimeServicesCollection.BuildServiceProvider();

            using (preBuildServices)
            {
                RunRegistrations(buildTimeServicesCollection, modules, preBuildServices, ModuleLoadPhase.PreBuild);
            }

            var buildTimeServices = buildTimeServicesCollection.BuildServiceProvider();

            using (buildTimeServices)
            {
                RunRegistrations(_serviceCollection, modules, buildTimeServices, ModuleLoadPhase.Build);

                foreach (var module in modules)
                {
                    var moduleInstance = buildTimeServices.GetRequiredService(module.Type);
                    (moduleInstance as IServiceCollectionModule)?.Build(_serviceCollection);
                }
            }

            _serviceCollection.AddSingleton(assemblyCatalog);
            _serviceCollection.AddSingleton<IModuleCatalog>(provider =>
                new ModuleCatalog(modules));

            return _serviceCollection.BuildServiceProvider(validateScopes: true);
        }

        private static void RunRegistrations(IServiceCollection targetServiceCollection, ICollection<ModuleMetadata> modules,
            IServiceProvider serviceProvider, string phase)
        {
            var preBuildLifecycleListeners = serviceProvider.GetServices<IRegisterAssemblyTypesListener>().ToArray();

            foreach (var descriptors in modules.GroupBy(x => x.Type.Assembly))
            {
                var assembly = descriptors.Key;
                var assemblyModules = descriptors.ToArray();

                foreach (var lifecycleListener in preBuildLifecycleListeners)
                {
                    lifecycleListener.OnLoad(new ModuleLoadPhase(targetServiceCollection, assembly, assemblyModules, phase));
                }
            }
        }

        private ICollection<Type> LoadLifecycleListeners(IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(x => x.GetTypes())
                .Where(x => _lifecycleListenerType.IsAssignableFrom(x) && x.IsClass && !x.IsAbstract)
                .ToArray();
        }
    }
}
