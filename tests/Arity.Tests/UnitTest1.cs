using Microsoft.Extensions.Hosting;
using Xunit;

namespace Arity.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
        }
    }

    [Module(nameof(Module1), Dependencies = new[] { nameof(Module2) })]
    public class Module1
    {
        private readonly IHostEnvironment _environment;

        public Module1(IHostEnvironment environment)
        {
            _environment = environment;
        }
    }

    [Module(nameof(Module2))]
    public class Module2
    {
    }
}
