using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Arity
{
    public class BootstrapperFactory : IServiceProviderFactory<Bootstrapper>
    {
        private readonly ModuleLoader _moduleLoader;
        private readonly IAssemblyCatalog _assemblyCatalog;
        private readonly BootstrapperFactoryOptions _options;

        private BootstrapperFactory(ModuleLoader moduleLoader,
            IAssemblyCatalog assemblyCatalog,
            BootstrapperFactoryOptions options)
        {
            _moduleLoader = moduleLoader;
            _assemblyCatalog = assemblyCatalog;
            _options = options;
        }

        public static BootstrapperFactory Create(ModuleLoader moduleLoader,
            IAssemblyCatalog assemblyCatalog,
            BootstrapperFactoryOptions options)
        {
            return new BootstrapperFactory(moduleLoader, assemblyCatalog, options);
        }

        [UsedImplicitly]
        public BootstrapperFactory(ModuleLoader moduleLoader,
            IAssemblyCatalog assemblyCatalog,
            IOptions<BootstrapperFactoryOptions> options)
        {
            _moduleLoader = moduleLoader;
            _assemblyCatalog = assemblyCatalog;
            _options = options.Value;
        }

        public Bootstrapper Create(IServiceCollection services)
        {
            return new Bootstrapper(_moduleLoader, services, _assemblyCatalog, _options);
        }

        public Bootstrapper CreateBuilder(IServiceCollection services)
        {
            return Create(services);
        }

        public IServiceProvider CreateServiceProvider(Bootstrapper bootstrapper)
        {
            return bootstrapper.Start();
        }
    }
}
