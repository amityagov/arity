using System.Collections.Generic;
using System.Reflection;

namespace Arity
{
    public interface IAssemblyCatalog
    {
        ICollection<Assembly> GetAssemblies();
    }
}
