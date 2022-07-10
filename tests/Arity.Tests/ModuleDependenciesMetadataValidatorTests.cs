using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace Arity.Tests
{
    public class ModuleDependenciesMetadataValidatorTests
    {
        [Theory]
        [MemberData(nameof(GetParameters))]
        public void IsValidModuleDependencies(Type moduleType, bool valid)
        {
            var dependenciesValidator = new ModuleDependenciesMetadataValidator();

            var errors = new List<string>();

            dependenciesValidator.Validate(new ModuleMetadata(moduleType),
                new Dictionary<string, ModuleMetadata>(), errors);

            Assert.Equal(valid, errors.Count == 0);
        }

        public static IEnumerable<object[]> GetParameters()
        {
            yield return new object[]
            {
                typeof(ValidModuleWithModuleOptions),
                true
            };

            yield return new object[]
            {
                typeof(ValidModuleWithConfiguration),
                true
            };

            yield return new object[]
            {
                typeof(ValidModuleWithHostEnvironment),
                true
            };

            yield return new object[]
            {
                typeof(InvalidModuleWithMicrosoftOptions),
                false
            };
        }

        public class Options
        {
        }

        private class ValidModuleWithConfiguration
        {
            public ValidModuleWithConfiguration(IConfiguration configuration)
            {
                _ = configuration;
            }
        }

        private class ValidModuleWithHostEnvironment
        {
            public ValidModuleWithHostEnvironment(IHostEnvironment environment)
            {
                _ = environment;
            }
        }

        private class ValidModuleWithModuleOptions
        {
            public ValidModuleWithModuleOptions(IModuleOptions<Options> options)
            {
                _ = options;
            }
        }

        private class InvalidModuleWithMicrosoftOptions
        {
            public InvalidModuleWithMicrosoftOptions(IOptions<Options> options)
            {
                _ = options;
            }
        }
    }
}
