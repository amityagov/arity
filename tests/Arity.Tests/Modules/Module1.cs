using Microsoft.Extensions.DependencyInjection;

namespace Arity.Tests.Modules
{
    [Module(nameof(Module1), Dependencies = new[] { nameof(Module2) })]
    public class Module1 : IServiceCollectionModule
    {
        public static bool Called;

        public void Build(IServiceCollection collection)
        {
            Called = true;
        }
    }
}
