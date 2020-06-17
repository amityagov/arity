using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arity
{
    public class Bootstrapper
    {
        private readonly ModuleLoader _moduleLoader;

        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _serviceCollection;
        private readonly IAssemblyCatalog _assemblyCatalog;
        private readonly string _entryModule;

        private readonly Type _lifecycleListenerType = typeof(ILifecycleListener<ModuleLoadPhase>);

        public Bootstrapper(ModuleLoader moduleLoader,
            IConfiguration configuration, IServiceCollection serviceCollection,
            IAssemblyCatalog assemblyCatalog, string entryModule)
        {
            if (entryModule == null)
                throw new ArgumentNullException(nameof(entryModule));

            _moduleLoader = moduleLoader;
            _configuration = configuration;
            _serviceCollection = serviceCollection;
            _assemblyCatalog = assemblyCatalog;
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

            buildTimeServicesCollection.AddSingleton(provider => _configuration);
            buildTimeServicesCollection.AddOptions();

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
            var preBuildLifecycleListeners = serviceProvider.GetServices<ILifecycleListener<ModuleLoadPhase>>().ToArray();

            foreach (var descriptors in modules.GroupBy(x => x.Type.Assembly))
            {
                var assembly = descriptors.Key;
                var modulesInAssembly = descriptors.Select(x => x.Name).ToArray();

                foreach (var lifecycleListener in preBuildLifecycleListeners)
                {
                    lifecycleListener.OnCreated(new ModuleLoadPhase(targetServiceCollection, assembly, modulesInAssembly, phase));
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
