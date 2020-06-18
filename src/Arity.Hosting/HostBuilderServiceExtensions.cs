using JetBrains.Annotations;
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

                return BootstrapperFactory.Create(context.Configuration, moduleLoader,
                    assemblyCatalog, new BootstrapperFactoryOptions
                    {
                        EntryModule = bootstrapperOptions.EntryModule
                    });
            });
        }
    }
}
