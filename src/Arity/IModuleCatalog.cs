using System.Collections.Generic;

namespace Arity
{
    public interface IModuleCatalog
    {
        IReadOnlyCollection<ModuleMetadata> GetModuleMetadata();
    }
}
