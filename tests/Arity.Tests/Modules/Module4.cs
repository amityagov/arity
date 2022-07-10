using Microsoft.Extensions.DependencyInjection;

namespace Arity.Tests.Modules
{
    [Module(nameof(Module4))]
    public class Module4 : IServiceCollectionModule
    {
        public static bool Called;

        public void Build(IServiceCollection collection)
        {
            Called = true;
        }
    }
}
