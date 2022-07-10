using Microsoft.Extensions.DependencyInjection;

namespace Arity.Tests.Modules
{
    [Module(nameof(Module3), Dependencies = new[] { nameof(Module4) })]
    public class Module3 : IServiceCollectionModule
    {
        public static bool Called;

        public void Build(IServiceCollection collection)
        {
            Called = true;
        }
    }
}