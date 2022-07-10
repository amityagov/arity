using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Arity
{
    [PublicAPI]
    public abstract class RegisterAssemblyTypesListener : IRegisterAssemblyTypesListener
    {
        private readonly HashSet<AssemblyMarker> _assemblyMarkers = new HashSet<AssemblyMarker>();

        public void OnLoad(AssemblyModuleLoadArgs value)
        {
            var (serviceCollection, assembly, modules) = value;

            var marker = new AssemblyMarker(assembly.FullName, GetType());

            if (_assemblyMarkers.Contains(marker))
            {
                return;
            }

            _assemblyMarkers.Add(marker);

            Register(serviceCollection, assembly, modules);
        }

        protected virtual void Register(IServiceCollection serviceCollection, Assembly assembly,
            ModuleMetadata[] modules)
        {
        }

        private class AssemblyMarker
        {
            private readonly string _assemblyName;

            private readonly Type _registrarType;

            public AssemblyMarker(string assemblyName, Type registrarType)
            {
                _assemblyName = assemblyName;
                _registrarType = registrarType;
            }

            private bool Equals(AssemblyMarker other)
            {
                return _assemblyName == other._assemblyName && _registrarType == other._registrarType;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((AssemblyMarker)obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(_assemblyName, _registrarType);
            }
        }
    }
}
