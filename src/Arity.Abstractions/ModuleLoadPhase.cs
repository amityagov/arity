using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Arity
{
    public class ModuleLoadPhase
    {
        public const string Build = "build";

        public const string PreBuild = "prebuild";

        public IServiceCollection Collection { get; }

        public Assembly Assembly { get; }

        public ModuleMetadata[] Modules { get; }

        public string Phase { get; }

        public ModuleLoadPhase(IServiceCollection collection, Assembly assembly, ModuleMetadata[] modules, string phase)
        {
            Collection = collection;
            Assembly = assembly;
            Modules = modules;
            Phase = phase;
        }

        public void Deconstruct(out IServiceCollection collection, out Assembly assembly, out ModuleMetadata[] modules, out string phase)
        {
            collection = Collection;
            assembly = Assembly;
            modules = Modules;
            phase = Phase;
        }
    }
}
