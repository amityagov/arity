using JetBrains.Annotations;

namespace Arity
{
    [PublicAPI]
    public interface ILifecycleListener<in T>
    {
        void OnCreated(T value);
    }
}
