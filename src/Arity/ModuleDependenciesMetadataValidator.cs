using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Arity
{
    public class ModuleDependenciesMetadataValidator : ModuleMetadataValidator
    {
        private readonly IDictionary<Type, bool> _moduleTypeValidResultMap = new Dictionary<Type, bool>();

        private readonly ISet<Type> _validParameterTypes = new HashSet<Type>();

        public ModuleDependenciesMetadataValidator()
        {
            _validParameterTypes.Add(typeof(IModuleOptions<>));
            _validParameterTypes.Add(typeof(IConfiguration));
            _validParameterTypes.Add(typeof(IHostEnvironment));
        }

        public override void Validate(ModuleMetadata metadata, IDictionary<string, ModuleMetadata> descriptorMap,
            ICollection<string> errors)
        {
            if (!IsValidModuleDependencies(metadata.Type))
            {
                errors.Add($"Module {metadata.Name} has invalid constructor dependencies");
            }
        }

        private bool IsValidModuleDependencies(Type moduleType)
        {
            if (_moduleTypeValidResultMap.TryGetValue(moduleType, out var valid))
            {
                return valid;
            }

            valid = IsValidModuleDependenciesInternal(moduleType);
            _moduleTypeValidResultMap.Add(moduleType, valid);

            return valid;
        }

        private bool IsValidModuleDependenciesInternal(Type moduleType)
        {
            var constructors = moduleType.GetConstructors();

            foreach (ConstructorInfo constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                foreach (ParameterInfo parameter in parameters)
                {
                    Type parameterType = parameter.ParameterType;
                    if (_validParameterTypes.Contains(parameterType))
                    {
                        continue;
                    }

                    if (parameterType.IsGenericType)
                    {
                        var genericParameterType = parameterType.GetGenericTypeDefinition();
                        if (_validParameterTypes.Contains(genericParameterType))
                        {
                            continue;
                        }
                    }

                    return false;
                }
            }

            return true;
        }
    }
}
