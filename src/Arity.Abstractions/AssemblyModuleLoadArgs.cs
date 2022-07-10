using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Arity
{
    public class AssemblyModuleLoadArgs
    {
        public IServiceCollection Collection { get; }

        public Assembly Assembly { get; }

        public ModuleMetadata[] Modules { get; }

        public AssemblyModuleLoadArgs(IServiceCollection collection, Assembly assembly, ModuleMetadata[] modules)
        {
            Collection = collection;
            Assembly = assembly;
            Modules = modules;
        }

        public void Deconstruct(out IServiceCollection collection, out Assembly assembly, out ModuleMetadata[] modules)
        {
            collection = Collection;
            assembly = Assembly;
            modules = Modules;
        }
    }
}
