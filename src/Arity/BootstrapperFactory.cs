using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Arity
{
    public class BootstrapperFactory : IServiceProviderFactory<Bootstrapper>
    {
        private readonly IConfiguration _configuration;
        private readonly ModuleLoader _moduleLoader;
        private readonly IAssemblyCatalog _assemblyCatalog;
        private readonly BootstrapperFactoryOptions _options;

        private BootstrapperFactory(IConfiguration configuration,
            ModuleLoader moduleLoader,
            IAssemblyCatalog assemblyCatalog,
            BootstrapperFactoryOptions options)
        {
            _configuration = configuration;
            _moduleLoader = moduleLoader;
            _assemblyCatalog = assemblyCatalog;
            _options = options;
        }

        public static BootstrapperFactory Create(IConfiguration configuration,
            ModuleLoader moduleLoader,
            IAssemblyCatalog assemblyCatalog,
            BootstrapperFactoryOptions options)
        {
            return new BootstrapperFactory(configuration, moduleLoader, assemblyCatalog, options);
        }

        [UsedImplicitly]
        public BootstrapperFactory(IConfiguration configuration,
            ModuleLoader moduleLoader,
            IAssemblyCatalog assemblyCatalog,
            IOptions<BootstrapperFactoryOptions> options)
        {
            _configuration = configuration;
            _moduleLoader = moduleLoader;
            _assemblyCatalog = assemblyCatalog;
            _options = options.Value;
        }

        public Bootstrapper Create(IServiceCollection services)
        {
            return new Bootstrapper(_moduleLoader, _configuration, services, _assemblyCatalog, _options.EntryModule);
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
