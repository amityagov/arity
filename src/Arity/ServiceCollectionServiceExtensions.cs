using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Arity
{
    [PublicAPI]
    public static class ServiceCollectionServiceExtensions
    {
        public static IModularityConfiguration AddBootstrapper(this IServiceCollection collection, IAssemblyCatalog assemblyCatalog, string entryModule)
        {
            return AddBootstrapper(collection, assemblyCatalog, new BootstrapperOptions
            {
                EntryModule = entryModule
            });
        }

        public static IModularityConfiguration AddBootstrapper(this IServiceCollection collection,
             IAssemblyCatalog assemblyCatalog, BootstrapperOptions bootstrapperOptions)
        {
            collection.Configure<BootstrapperFactoryOptions>(x =>
            {
                x.EntryModule = bootstrapperOptions.EntryModule;
            });

            collection.AddSingleton<IAssemblyCatalog>(assemblyCatalog);
            collection.AddSingleton<BootstrapperFactory>();
            collection.AddSingleton<ModuleLoader>();

            foreach (var validator in bootstrapperOptions.Validators ?? Array.Empty<ModuleMetadataValidator>())
            {
                collection.AddSingleton<ModuleMetadataValidator>(validator);
            }

            return new ModularityConfiguration(collection);
        }
    }
}
