using System;
using System.Collections.Generic;
using System.Reflection;
using Arity.Tests.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Arity.Tests
{
    public class BootstrapperTests
    {
        [Fact]
        public void Start_SingleEntryModule_EntryModuleAndAllDepsCalled()
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
        public void Start_MultiplyEntryModule_AllEntryModulesAndAllDepsCalled()
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

        [Fact]
        public void Start_ConfigureBootstrapper_EntryModulesOverridenFromConfiguration()
        {
            Module1.Called = false;
            Module4.Called = false;

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IAssemblyCatalog>(new StaticAssemblyCatalog(typeof(Module1).Assembly));

            var configuration = new ConfigurationManager();

            configuration.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("Modularity:EntryModules:0", "Module4")
            });

            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddBootstrapper(new[] { nameof(Module1) });
            serviceCollection.ConfigureBootstrapper();

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            var bootstrapperFactory = serviceProvider.GetRequiredService<BootstrapperFactory>();

            Bootstrapper bootstrapper = bootstrapperFactory.Create(serviceCollection);
            bootstrapper.Start();

            Assert.True(Module4.Called);
            Assert.False(Module1.Called);
        }

        [Fact]
        public void Start_ModuleOptionsResolved()
        {
            ModuleWithOptions.Called = false;

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IAssemblyCatalog>(new StaticAssemblyCatalog(typeof(Module1).Assembly));

            serviceCollection.AddBootstrapper(new[] { nameof(ModuleWithOptions) });

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            var bootstrapperFactory = serviceProvider.GetRequiredService<BootstrapperFactory>();

            Bootstrapper bootstrapper = bootstrapperFactory.Create(serviceCollection);
            bootstrapper.Start();

            Assert.True(ModuleWithOptions.Called);
            Assert.NotNull(ModuleWithOptions.Options);
            Assert.True(ModuleWithOptions.Options.Disposed);

            Assert.Throws<ObjectDisposedException>(() => _ = ModuleWithOptions.Options.Value);
        }

        [Fact]
        public void Start_ListenerInvoked()
        {
            TestListener.RegisterCalledTimes = 0;

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IAssemblyCatalog>(new StaticAssemblyCatalog(typeof(Module1).Assembly));

            serviceCollection.AddBootstrapper(new[] { nameof(Module1) });

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            var bootstrapperFactory = serviceProvider.GetRequiredService<BootstrapperFactory>();

            Bootstrapper bootstrapper = bootstrapperFactory.Create(serviceCollection);
            bootstrapper.Start();

            Assert.Equal(1, TestListener.RegisterCalledTimes);
        }

        public class TestListener : RegisterAssemblyTypesListener
        {
            public static int RegisterCalledTimes;

            protected override void Register(IServiceCollection serviceCollection, Assembly assembly,
                ModuleMetadata[] modules)
            {
                RegisterCalledTimes++;
            }
        }
    }
}
