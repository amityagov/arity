using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arity.Hosting
{
    [PublicAPI]
    public static class HostBuilderServiceExtensions
    {
        public static IHostBuilder UseBootstrapperFactory(this IHostBuilder builder, IAssemblyCatalog assemblyCatalog,
            BootstrapperOptions options, IEnumerable<ModuleMetadataValidator> validators = null)
        {
            return builder.UseServiceProviderFactory(context =>
            {
                var moduleLoader = new ModuleLoader(validators);

                options.ConfigureBuildTimeServices.Add(collection => collection.AddSingleton(context.HostingEnvironment));
                options.ConfigureBuildTimeServices.Add(collection => collection.AddSingleton(context.Configuration));

                return BootstrapperFactory.Create(moduleLoader, assemblyCatalog, options);
            });
        }
    }
}
