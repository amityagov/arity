using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Arity
{
    [PublicAPI]
    public static class ServiceCollectionServiceExtensions
    {
        public static IModularityConfiguration AddBootstrapper([NotNull] this IServiceCollection collection,
            [NotNull] IEnumerable<string> entryModules,
            IEnumerable<ModuleMetadataValidator> validators = null)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (entryModules == null)
                throw new ArgumentNullException(nameof(entryModules));
            var entryModulesArray = entryModules.ToArray();

            if (!entryModulesArray.Any())
                throw new ArgumentNullException(nameof(entryModules));

            collection.Configure<BootstrapperOptions>(x =>
            {
                x.EntryModules = entryModulesArray;
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
