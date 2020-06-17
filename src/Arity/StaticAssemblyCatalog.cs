using System.Collections.Generic;
using System.Reflection;

namespace Arity
{
    public class StaticAssemblyCatalog : IAssemblyCatalog
    {
        private readonly Assembly[] _assemblies;

        public StaticAssemblyCatalog(params Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        public ICollection<Assembly> GetAssemblies()
        {
            return _assemblies;
        }
    }
}
