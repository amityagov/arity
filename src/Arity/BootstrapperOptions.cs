using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Arity
{
    public class BootstrapperOptions
    {
        public ICollection<Action<IServiceCollection>> ConfigureBuildTimeServices { get; } =
            new List<Action<IServiceCollection>>();

        public ICollection<string> EntryModules { get; set; }
    }
}
