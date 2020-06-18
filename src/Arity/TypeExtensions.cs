using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Arity
{
    public static class TypeExtensions
    {
        public static IEnumerable<TypeWithAttributes<TAttribute>> GetTypesWithAttribute<TAttribute>(this Assembly assembly, bool inherit = false)
            where TAttribute : Attribute
        {
            return assembly.GetTypes().Select(x =>
            {
                var attributes = x.GetTypeInfo().GetCustomAttributes(typeof(TAttribute), inherit).OfType<TAttribute>().ToArray();

                return new TypeWithAttributes<TAttribute>(x, attributes);
            }).Where(x => x.Attributes.Length > 0).ToArray();
        }

        public class TypeWithAttributes<TAttribute>
        {
            public Type Type { get; }

            public TAttribute[] Attributes { get; }

            public TypeWithAttributes(Type type, TAttribute[] attributes)
            {
                Type = type;
                Attributes = attributes;
            }
        }
    }
}
