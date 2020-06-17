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

        public string[] Modules { get; }

        public string Phase { get; }

        public ModuleLoadPhase(IServiceCollection collection, Assembly assembly, string[] modules, string phase)
        {
            Collection = collection;
            Assembly = assembly;
            Modules = modules;
            Phase = phase;
        }

        public void Deconstruct(out IServiceCollection collection, out Assembly assembly, out string[] modules, out string phase)
        {
            collection = Collection;
            assembly = Assembly;
            modules = Modules;
            phase = Phase;
        }
    }
}
