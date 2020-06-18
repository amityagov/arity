using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arity.Hosting
{
    [PublicAPI]
    public static class HostBuilderServiceExtensions
    {
        public static IHostBuilder UseBootstrapperFactory(this IHostBuilder builder, IAssemblyCatalog assemblyCatalog,
            BootstrapperOptions bootstrapperOptions)
        {
            return builder.UseServiceProviderFactory(context =>
            {
                var moduleLoader = new ModuleLoader(bootstrapperOptions.Validators);

                var options = new BootstrapperFactoryOptions
                {
                    EntryModule = bootstrapperOptions.EntryModule
                };

                options.ConfigureBuildTimeServices.Add(collection => collection.AddSingleton(context.HostingEnvironment));
                options.ConfigureBuildTimeServices.Add(collection => collection.AddSingleton(context.Configuration));

                return BootstrapperFactory.Create(moduleLoader, assemblyCatalog, options);
            });
        }
    }
}
