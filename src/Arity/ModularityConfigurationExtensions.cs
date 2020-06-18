using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Arity
{
    public static class ModularityConfigurationExtensions
    {
        public static IModularityConfiguration UseStaticCatalog(this IModularityConfiguration configuration,
            params Assembly[] assemblies)
        {
            configuration.ServiceCollection.AddSingleton<IAssemblyCatalog>(new StaticAssemblyCatalog(assemblies));

            return configuration;
        }

        public static IModularityConfiguration ConfigureBootstrapperOptions(this IModularityConfiguration configuration,
            Action<BootstrapperFactoryOptions> action)
        {
            configuration.ServiceCollection.Configure<BootstrapperFactoryOptions>(action);

            return configuration;
        }
    }
}
