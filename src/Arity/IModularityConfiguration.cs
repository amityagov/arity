using Microsoft.Extensions.DependencyInjection;

namespace Arity
{
    public interface IModularityConfiguration
    {
        IServiceCollection ServiceCollection { get; }
    }
}
