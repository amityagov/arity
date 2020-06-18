using Microsoft.Extensions.DependencyInjection;

namespace Arity
{
    internal class ModularityConfiguration : IModularityConfiguration
    {
        public IServiceCollection ServiceCollection { get; }

        public ModularityConfiguration(IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection;
        }
    }
}
