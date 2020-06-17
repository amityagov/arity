using System;

namespace Arity
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class ModuleMetadataAttribute : Attribute
    {
        public abstract void Apply(ModuleMetadata metadata);
    }
}
