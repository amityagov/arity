using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Arity
{
    [PublicAPI]
    public static class ServiceCollectionServiceExtensions
    {
        public static IModularityConfiguration AddBootstrapper([NotNull] this IServiceCollection collection,
            [NotNull] string entryModule,
            IEnumerable<ModuleMetadataValidator> validators = null)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (entryModule == null)
                throw new ArgumentNullException(nameof(entryModule));

            collection.Configure<BootstrapperOptions>(x =>
            {
                x.EntryModule = entryModule;
            });

            collection.AddSingleton<BootstrapperFactory>();
            collection.AddSingleton<ModuleLoader>();

            foreach (var validator in validators ?? Array.Empty<ModuleMetadataValidator>())
            {
                collection.AddSingleton<ModuleMetadataValidator>(validator);
            }

            return new ModularityConfiguration(collection);
        }
    }
}
