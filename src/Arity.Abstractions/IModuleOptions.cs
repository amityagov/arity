using Microsoft.Extensions.Options;

namespace Arity
{
    public interface IModuleOptions<out T> : IOptions<T>
        where T : class
    {
        bool Disposed { get; }
    }
}
