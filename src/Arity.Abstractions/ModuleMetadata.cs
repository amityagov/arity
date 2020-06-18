using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Arity
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class ModuleMetadata
    {
        public Assembly Assembly { get; }

        public Type Type { get; }

        public string Name { get; set; }

        public string[] Dependencies { get; set; }

        public IDictionary<string, object> Items { get; }

        public ModuleMetadata(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Type = type;
            Assembly = type.Assembly;
            Items = new Dictionary<string, object>();
        }

        public ModuleMetadata(Type type, string name, string[] dependencies, IDictionary<string, object> items)
            : this(type)
        {
            Name = name;
            Items = items;
            Dependencies = dependencies ?? Array.Empty<string>();
        }

        private sealed class NameEqualityComparer : IEqualityComparer<ModuleMetadata>
        {
            public bool Equals(ModuleMetadata x, ModuleMetadata y)
            {
                if (ReferenceEquals(x, y))
                    return true;
                if (ReferenceEquals(x, null))
                    return false;
                if (ReferenceEquals(y, null))
                    return false;
                if (x.GetType() != y.GetType())
                    return false;
                return string.Equals(x.Name, y.Name);
            }

            public int GetHashCode(ModuleMetadata obj)
            {
                return (obj.Name != null ? obj.Name.GetHashCode() : 0);
            }
        }

        public static IEqualityComparer<ModuleMetadata> ByName { get; } = new NameEqualityComparer();
    }
}
