using System.Collections.Generic;
using System.Linq;

namespace Arity
{
    public class ModuleCatalog : IModuleCatalog
    {
        private readonly ModuleMetadata[] _metadata;

        public ModuleCatalog(ICollection<ModuleMetadata> metadata)
        {
            _metadata = metadata.ToArray();
        }

        public IReadOnlyCollection<ModuleMetadata> GetModuleMetadata()
        {
            return _metadata;
        }
    }
}
