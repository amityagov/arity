using Microsoft.Extensions.DependencyInjection;

namespace Arity
{
    public interface IServiceCollectionModule
    {
        void Build(IServiceCollection collection);
    }
}
