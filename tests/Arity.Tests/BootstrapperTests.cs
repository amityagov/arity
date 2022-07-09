using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Arity.Tests
{
    public class BootstrapperTests
    {
        [Fact]
        public void SingleEntryModule()
        {
            Module1.Called = false;
            Module2.Called = false;
            Module3.Called = false;
            Module4.Called = false;

            var moduleLoader = new ModuleLoader(Array.Empty<ModuleMetadataValidator>());
            var serviceCollection = new ServiceCollection();
            var assemblyCatalog = new StaticAssemblyCatalog(typeof(BootstrapperTests).Assembly);

            var options = new OptionsWrapper<BootstrapperOptions>(new BootstrapperOptions
            {
                EntryModules = new List<string>
                {
                    nameof(Module1)
                }
            });

            var bootstrapper = new Bootstrapper(moduleLoader, serviceCollection, assemblyCatalog, options);

            bootstrapper.Start();

            Assert.True(Module1.Called);
            Assert.True(Module2.Called);
            Assert.False(Module3.Called);
        }

        [Fact]
        public void MultiplyEntryModule()
        {
            Module1.Called = false;
            Module2.Called = false;
            Module3.Called = false;
            Module4.Called = false;

            var moduleLoader = new ModuleLoader(Array.Empty<ModuleMetadataValidator>());
            var serviceCollection = new ServiceCollection();
            var assemblyCatalog = new StaticAssemblyCatalog(typeof(BootstrapperTests).Assembly);

            var options = new OptionsWrapper<BootstrapperOptions>(new BootstrapperOptions
            {
                EntryModules = new List<string>
                {
                    nameof(Module1),
                    nameof(Module3)
                }
            });

            var bootstrapper = new Bootstrapper(moduleLoader, serviceCollection, assemblyCatalog, options);

            bootstrapper.Start();

            Assert.True(Module1.Called);
            Assert.True(Module2.Called);
            Assert.True(Module3.Called);
            Assert.True(Module4.Called);
        }
    }

    [Module(nameof(Module1), Dependencies = new[] { nameof(Module2) })]
    public class Module1 : IServiceCollectionModule
    {
        public static bool Called;

        public void Build(IServiceCollection collection)
        {
            Called = true;
        }
    }

    [Module(nameof(Module2))]
    public class Module2 : IServiceCollectionModule
    {
        public static bool Called;

        public void Build(IServiceCollection collection)
        {
            Called = true;
        }
    }

    [Module(nameof(Module3), Dependencies = new[] { nameof(Module4) })]
    public class Module3 : IServiceCollectionModule
    {
        public static bool Called;

        public void Build(IServiceCollection collection)
        {
            Called = true;
        }
    }

    [Module(nameof(Module4))]
    public class Module4 : IServiceCollectionModule
    {
        public static bool Called;

        public void Build(IServiceCollection collection)
        {
            Called = true;
        }
    }
}
