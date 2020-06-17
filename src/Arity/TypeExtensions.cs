using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Arity
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns true if this type is in the <paramref name="namespace" /> namespace
        /// or one of its sub-namespaces.
        /// </summary>
        /// <param name="this">The type to test.</param>
        /// <param name="namespace">The namespace to test.</param>
        /// <returns>True if this type is in the <paramref name="namespace" /> namespace
        /// or one of its sub-namespaces; otherwise, false.</returns>
        public static bool IsInNamespace(this Type @this, string @namespace)
        {
            if ((object)@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@namespace == null)
                throw new ArgumentNullException(nameof(@namespace));
            if (@this.Namespace == null)
                return false;
            if (@this.Namespace != @namespace)
                return @this.Namespace.StartsWith(@namespace + ".", StringComparison.Ordinal);

            return true;
        }

        /// <summary>
        /// Returns true if this type is in the same namespace as <typeparamref name="T" />
        /// or one of its sub-namespaces.
        /// </summary>
        /// <param name="this">The type to test.</param>
        /// <returns>True if this type is in the same namespace as <typeparamref name="T" />
        /// or one of its sub-namespaces; otherwise, false.</returns>
        public static bool IsInNamespaceOf<T>(this Type @this)
        {
            if ((object)@this == null)
                throw new ArgumentNullException(nameof(@this));
            return @this.IsInNamespace(typeof(T).Namespace);
        }

        /// <summary>
        /// Determines whether the candidate type supports any base or
        /// interface that closes the provided generic type.
        /// </summary>
        /// <param name="this">The type to test.</param>
        /// <param name="openGeneric">The open generic against which the type should be tested.</param>
        public static bool IsClosedTypeOf(this Type @this, Type openGeneric)
        {
            if ((object)@this == null)
                throw new ArgumentNullException(nameof(@this));
            if ((object)openGeneric == null)
                throw new ArgumentNullException(nameof(openGeneric));
            if (!openGeneric.GetTypeInfo().IsGenericTypeDefinition &&
                !openGeneric.GetTypeInfo().ContainsGenericParameters)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Not Open Generic Type"));
            }

            return @this.GetTypesThatClose(openGeneric).Any();
        }

        /// <summary>
        /// Determines whether this type is assignable to <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type to test assignability to.</typeparam>
        /// <param name="this">The type to test.</param>
        /// <returns>True if this type is assignable to references of type
        /// <typeparamref name="T" />; otherwise, False.</returns>
        public static bool IsAssignableTo<T>(this Type @this)
        {
            if ((object)@this == null)
                throw new ArgumentNullException(nameof(@this));
            return typeof(T).GetTypeInfo().IsAssignableFrom(@this.GetTypeInfo());
        }

        /// <summary>
        /// Finds a constructor with the matching type parameters.
        /// </summary>
        /// <param name="type">The type being tested.</param>
        /// <param name="constructorParameterTypes">The types of the contractor to find.</param>
        /// <returns>The <see cref="T:System.Reflection.ConstructorInfo" /> is a match is found; otherwise, <c>null</c>.</returns>
        public static ConstructorInfo GetMatchingConstructor(
          this Type type,
          Type[] constructorParameterTypes)
        {
            return type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(constructorParameterTypes));
        }

        private static readonly ConcurrentDictionary<Type, bool> IsGenericEnumerableInterfaceCache = new ConcurrentDictionary<Type, bool>();
        private static readonly ConcurrentDictionary<Type, bool> IsGenericListOrCollectionInterfaceTypeCache = new ConcurrentDictionary<Type, bool>();
        private static readonly ConcurrentDictionary<Tuple<Type, Type>, bool> IsGenericTypeDefinedByCache = new ConcurrentDictionary<Tuple<Type, Type>, bool>();

        public static Type FunctionReturnType(this Type type)
        {
            var declaredMethod = type.GetTypeInfo().GetDeclaredMethod("Invoke");

            if (declaredMethod == null)
                throw new ArgumentNullException(nameof(declaredMethod));

            return declaredMethod.ReturnType;
        }

        /// <summary>Returns the first concrete interface supported by the candidate type that
        /// closes the provided open generic service type.</summary>
        /// <param name="this">The type that is being checked for the interface.</param>
        /// <param name="openGeneric">The open generic type to locate.</param>
        /// <returns>The type of the interface.</returns>
        public static IEnumerable<Type> GetTypesThatClose(
          this Type @this,
          Type openGeneric)
        {
            return FindAssignableTypesThatClose(@this, openGeneric);
        }

        public static bool IsCompilerGenerated(this Type type)
        {
            return type.GetTypeInfo().GetCustomAttributes<CompilerGeneratedAttribute>().Any();
        }

        public static bool IsDelegate(this Type type)
        {
            return type.GetTypeInfo().IsSubclassOf(typeof(Delegate));
        }

        public static bool IsGenericEnumerableInterfaceType(this Type type)
        {
            return IsGenericEnumerableInterfaceCache.GetOrAdd(type, t =>
            {
                if (!type.IsGenericTypeDefinedBy(typeof(IEnumerable<>)))
                    return type.IsGenericListOrCollectionInterfaceType();
                return true;
            });
        }

        public static bool IsGenericListOrCollectionInterfaceType(this Type type)
        {
            return IsGenericListOrCollectionInterfaceTypeCache.GetOrAdd(type, t =>
            {
                if (!t.IsGenericTypeDefinedBy(typeof(IList<>)) && !t.IsGenericTypeDefinedBy(typeof(ICollection<>)) && !t.IsGenericTypeDefinedBy(typeof(IReadOnlyCollection<>)))
                    return t.IsGenericTypeDefinedBy(typeof(IReadOnlyList<>));
                return true;
            });
        }

        public static bool IsGenericTypeDefinedBy(this Type @this, Type openGeneric)
        {
            return IsGenericTypeDefinedByCache.GetOrAdd(Tuple.Create(@this, openGeneric), key =>
            {
                if (!key.Item1.GetTypeInfo().ContainsGenericParameters && key.Item1.GetTypeInfo().IsGenericType)
                    return key.Item1.GetGenericTypeDefinition() == key.Item2;
                return false;
            });
        }

        /// <summary>
        /// Looks for an interface on the candidate type that closes the provided open generic interface type.
        /// </summary>
        /// <param name="candidateType">The type that is being checked for the interface.</param>
        /// <param name="openGenericServiceType">The open generic service type to locate.</param>
        /// <returns>True if a closed implementation was found; otherwise false.</returns>
        private static IEnumerable<Type> FindAssignableTypesThatClose(
          Type candidateType,
          Type openGenericServiceType)
        {
            return TypesAssignableFrom(candidateType).Where(t => t.IsClosedTypeOf2(openGenericServiceType));
        }

        public static bool IsClosedTypeOf2(this Type @this, Type openGeneric)
        {
            return TypesAssignableFrom(@this).Any(t =>
            {
                if (t.GetTypeInfo().IsGenericType && !@this.GetTypeInfo().ContainsGenericParameters)
                    return t.GetGenericTypeDefinition() == openGeneric;

                return false;
            });
        }

        private static Type SubstituteGenericParameterConstraint(Type[] parameters, Type constraint)
        {
            if (!constraint.IsGenericParameter)
                return constraint;
            return parameters[constraint.GenericParameterPosition];
        }

        private static bool ParameterCompatibleWithTypeConstraint(Type parameter, Type constraint)
        {
            if (!constraint.GetTypeInfo().IsAssignableFrom(parameter.GetTypeInfo()))
                return Across(parameter, p => p.GetTypeInfo().BaseType).Concat(parameter.GetTypeInfo().ImplementedInterfaces).Any(p => ParameterEqualsConstraint(p, constraint));
            return true;
        }

        public static IEnumerable<Type> DerivedFromGeneric(this Assembly assembly, Type type)
        {
            return
                assembly.GetTypes()
                    .Where(t => t.GetTypeInfo().BaseType != null && t.GetTypeInfo().BaseType.GetTypeInfo().IsGenericType &&
                                t.GetTypeInfo().BaseType.GetGenericTypeDefinition() == type);
        }

        public static TAttribute GetAttribute<TAttribute>(this Type type)
        {
            return type.GetTypeInfo().GetCustomAttributes(typeof(TAttribute), false).OfType<TAttribute>().FirstOrDefault();
        }

        public static IEnumerable<Type> GetTypesImplements(this Assembly assembly, Type type)
        {
            return assembly.GetTypes().Where(x => x.GetInterfaces().Contains(type));
        }

        public static IEnumerable<TypeWithAttributes<TAttribute>> GetTypesWithAttribute<TAttribute>(this IEnumerable<Assembly> assemblies)
            where TAttribute : Attribute
        {
            return assemblies.SelectMany(x => x.GetTypesWithAttribute<TAttribute>());
        }

        public static IEnumerable<TypeWithAttributes<TAttribute>> GetTypesWithAttribute<TAttribute>(this Assembly assembly, bool inherit = false)
            where TAttribute : Attribute
        {
            return assembly.GetTypes().Select(x =>
            {
                var attributes = x.GetTypeInfo().GetCustomAttributes(typeof(TAttribute), inherit).OfType<TAttribute>().ToArray();

                return new TypeWithAttributes<TAttribute>(x, attributes);
            }).Where(x => x.Attributes.Length > 0).ToArray();
        }

        public readonly struct TypeWithAttributes<TAttribute>
        {
            public Type Type { get; }

            public TAttribute[] Attributes { get; }

            public TypeWithAttributes(Type type, TAttribute[] attributes)
            {
                Type = type;
                Attributes = attributes;
            }
        }

        private static bool ParameterEqualsConstraint(Type parameter, Type constraint)
        {
            var genericTypeArguments1 = parameter.GetTypeInfo().GenericTypeArguments;
            if (genericTypeArguments1.Length != 0 && constraint.GetTypeInfo().IsGenericType)
            {
                var genericTypeDefinition = constraint.GetGenericTypeDefinition();
                if (genericTypeDefinition.GetTypeInfo().GenericTypeParameters.Length == genericTypeArguments1.Length)
                {
                    try
                    {
                        var type = genericTypeDefinition.MakeGenericType(genericTypeArguments1);
                        var genericTypeArguments2 = constraint.GetTypeInfo().GenericTypeArguments;
                        for (var index = 0; index < genericTypeArguments2.Length; ++index)
                        {
                            var typeInfo = genericTypeArguments2[index].GetTypeInfo();
                            if (!typeInfo.IsGenericParameter && !typeInfo.IsAssignableFrom(genericTypeArguments1[index].GetTypeInfo()))
                                return false;
                        }

                        return type == (object)parameter;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private static IEnumerable<Type> TypesAssignableFrom(Type candidateType)
        {
            return candidateType.GetTypeInfo().ImplementedInterfaces.Concat(Across(candidateType, t => t.GetTypeInfo().BaseType));
        }

        public static IEnumerable<T> Across<T>(T first, Func<T, T> next)
            where T : class
        {
            for (T item = first; (object)item != null; item = next(item))
                yield return item;
        }
    }
}
