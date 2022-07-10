using Microsoft.Extensions.DependencyInjection;

namespace Arity.Tests.Modules
{
    [Module(nameof(Module2))]
    public class Module2 : IServiceCollectionModule
    {
        public static bool Called;

        public void Build(IServiceCollection collection)
        {
            Called = true;
        }
    }
}