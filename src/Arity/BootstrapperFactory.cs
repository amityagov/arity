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
        private readonly IOptions<BootstrapperOptions> _options;

        public static BootstrapperFactory Create(ModuleLoader moduleLoader,
            IAssemblyCatalog assemblyCatalog, BootstrapperOptions options)
        {
            return new BootstrapperFactory(moduleLoader, assemblyCatalog,
                new OptionsWrapper<BootstrapperOptions>(options));
        }

        [UsedImplicitly]
        public BootstrapperFactory(ModuleLoader moduleLoader,
            IAssemblyCatalog assemblyCatalog,
            IOptions<BootstrapperOptions> options)
        {
            _moduleLoader = moduleLoader;
            _assemblyCatalog = assemblyCatalog;
            _options = options;
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
