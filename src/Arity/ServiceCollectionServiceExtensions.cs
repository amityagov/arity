using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Arity
{
    [PublicAPI]
    public static class ServiceCollectionServiceExtensions
    {
        public static IModularityConfiguration AddBootstrapper(this IServiceCollection collection, string entryModule)
        {
            return AddBootstrapper(collection, new BootstrapperOptions
            {
                EntryModule = entryModule
            });
        }

        public static IModularityConfiguration AddBootstrapper(this IServiceCollection collection,
            BootstrapperOptions bootstrapperOptions)
        {
            collection.Configure<BootstrapperOptions>(x =>
            {
                x.EntryModule = bootstrapperOptions.EntryModule;
            });

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
