using System.Collections.Generic;

namespace Arity
{
    public class BootstrapperOptions
    {
        public string EntryModule { get; set; }

        public ICollection<ModuleMetadataValidator> Validators { get; set; }
    }
}
