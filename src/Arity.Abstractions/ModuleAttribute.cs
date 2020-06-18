using System;
using System.Linq;

namespace Arity
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ModuleAttribute : ModuleMetadataAttribute
    {
        public string Name { get; }

        public string[] Dependencies { get; set; }

        public ModuleAttribute(string name)
        {
            Name = name;
        }

        public override void Apply(ModuleMetadata metadata)
        {
            metadata.Name = Name;
            metadata.Dependencies = Dependencies?.ToArray();
        }
    }
}
