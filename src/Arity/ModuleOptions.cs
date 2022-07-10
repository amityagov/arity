using System;
using Microsoft.Extensions.Options;

namespace Arity
{
    public class ModuleOptions<T> : IModuleOptions<T>, IDisposable
        where T : class
    {
        private readonly IOptions<T> _options;

        public T Value
        {
            get
            {
                CheckDisposed();
                return _options.Value;
            }
        }

        public ModuleOptions(IOptions<T> options)
        {
            _options = options;
        }

        private void CheckDisposed()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(GetType().Name, "Module options already disposed");
            }
        }

        public void Dispose()
        {
            Disposed = true;
        }

        public bool Disposed { get; private set; }
    }
}
