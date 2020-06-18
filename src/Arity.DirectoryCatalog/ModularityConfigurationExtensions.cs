using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Arity
{
    [PublicAPI]
    public static class ModularityConfigurationExtensions
    {
        public static IModularityConfiguration UseDirectoryCatalog(this IModularityConfiguration configuration,
            string basePath, string[] patterns)
        {
            configuration.ServiceCollection.Configure<DirectoryAssemblyCatalogOptions>(options =>
            {
                options.BasePath = basePath;
                options.Patterns = patterns;
            });

            configuration.ServiceCollection.AddSingleton<IAssemblyCatalog, DirectoryAssemblyCatalog>();

            return configuration;
        }
    }
}
