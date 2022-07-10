using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Arity
{
    [PublicAPI]
    public static class ServiceCollectionServiceExtensions
    {
        public static IModularityConfiguration AddBootstrapper([NotNull] this IServiceCollection collection,
            IEnumerable<string> entryModules = null,
            IEnumerable<ModuleMetadataValidator> validators = null)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (entryModules != null)
            {
                var entryModulesArray = entryModules.ToArray();

                if (entryModulesArray.Length > 0)
                {
                    collection.Configure<BootstrapperOptions>(x => { x.EntryModules = entryModulesArray; });
                }
            }

            collection.AddSingleton<BootstrapperFactory>();
            collection.AddSingleton<ModuleLoader>();

            foreach (var validator in validators ?? Array.Empty<ModuleMetadataValidator>())
            {
                collection.AddSingleton<ModuleMetadataValidator>(validator);
            }

            return new ModularityConfiguration(collection);
        }

        public static IServiceCollection ConfigureBootstrapper([NotNull] this IServiceCollection collection,
            string sectionName = "Modularity")
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            collection.Configure<Options>(x => { x.SectionName = sectionName; });
            collection.ConfigureOptions<BootstrapperOptionsConfiguration>();

            return collection;
        }

        private class Options
        {
            public string SectionName { get; set; }
        }

        private class BootstrapperOptionsConfiguration : IConfigureOptions<BootstrapperOptions>
        {
            private readonly IConfiguration _configuration;
            private readonly IOptions<Options> _options;

            public BootstrapperOptionsConfiguration(IConfiguration configuration, IOptions<Options> options)
            {
                _configuration = configuration;
                _options = options;
            }

            public void Configure(BootstrapperOptions options)
            {
                var sectionName = _options.Value.SectionName;
                IConfigurationSection section = _configuration.GetSection(sectionName);
                var optionsToTest = new BootstrapperOptions();

                section.Bind(optionsToTest);

                if (optionsToTest.EntryModules != null && optionsToTest.EntryModules.Count > 0)
                {
                    options.EntryModules = null;
                }

                section.Bind(options);
            }
        }
    }
}
