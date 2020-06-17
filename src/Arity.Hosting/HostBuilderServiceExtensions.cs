using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

namespace Arity.Hosting
{
    [PublicAPI]
    public static class HostBuilderServiceExtensions
    {
        public static IHostBuilder UseBootstrapperFactory(this IHostBuilder builder,
            BootstrapOptions bootstrapOptions, IAssemblyCatalog assemblyCatalog)
        {
            return builder.UseServiceProviderFactory(context =>
            {
                var moduleLoader = new ModuleLoader(bootstrapOptions.Validators);

                return BootstrapperFactory.Create(context.Configuration, moduleLoader,
                    assemblyCatalog, new BootstrapperFactoryOptions
                    {
                        EntryModule = bootstrapOptions.EntryModule
                    });
            });
        }
    }
}
