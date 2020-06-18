using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Arity
{
    public abstract class RegisterAssemblyTypesListener : IRegisterAssemblyTypesListener
    {
        private readonly HashSet<AssemblyMarker> _assemblyMarkers = new HashSet<AssemblyMarker>();

        public void OnLoad(ModuleLoadPhase value)
        {
            var (serviceCollection, assembly, modules, phase) = value;

            var marker = new AssemblyMarker(assembly.FullName, GetType(), phase);

            if (_assemblyMarkers.Contains(marker))
            {
                return;
            }

            _assemblyMarkers.Add(marker);

            Register(serviceCollection, assembly, modules, phase);
        }

        protected virtual void Register(IServiceCollection serviceCollection, Assembly assembly, ModuleMetadata[] modules,
            string phase)
        {
            if (phase == ModuleLoadPhase.PreBuild)
            {
                RegisterPreBuildPhase(serviceCollection, assembly, modules);
            }

            if (phase == ModuleLoadPhase.Build)
            {
                RegisterBuildPhase(serviceCollection, assembly, modules);
            }
        }

        protected virtual void RegisterPreBuildPhase(IServiceCollection serviceCollection, Assembly assembly, ModuleMetadata[] modules)
        {
        }

        protected virtual void RegisterBuildPhase(IServiceCollection serviceCollection, Assembly assembly, ModuleMetadata[] modules)
        {
        }

        private class AssemblyMarker
        {
            public string AssemblyName { get; }

            public Type RegistrarType { get; }

            public string Phase { get; }

            public AssemblyMarker(string assemblyName, Type registrarType, string phase)
            {
                AssemblyName = assemblyName;
                RegistrarType = registrarType;
                Phase = phase;
            }

            private bool Equals(AssemblyMarker other)
            {
                return string.Equals(AssemblyName, other.AssemblyName) && RegistrarType == other.RegistrarType && string.Equals(Phase, other.Phase);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;

                if (ReferenceEquals(this, obj))
                    return true;

                if (obj.GetType() != GetType())
                    return false;

                return Equals((AssemblyMarker)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = AssemblyName != null ? AssemblyName.GetHashCode() : 0;
                    hashCode = (hashCode * 397) ^ (RegistrarType != null ? RegistrarType.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Phase != null ? Phase.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }
    }
}
