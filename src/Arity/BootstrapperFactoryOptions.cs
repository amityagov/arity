using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Arity
{
    public class BootstrapperFactoryOptions
    {
        public ICollection<Action<IServiceCollection>> ConfigureBuildTimeServices { get; } = new List<Action<IServiceCollection>>();

        public string EntryModule { get; set; }
    }
}
