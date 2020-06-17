using System.Collections.Generic;

namespace Arity
{
    public abstract class ModuleMetadataValidator
    {
        public abstract void Validate(ModuleMetadata metadata, IDictionary<string, ModuleMetadata> descriptorMap,
            ICollection<string> errors);
    }
}
