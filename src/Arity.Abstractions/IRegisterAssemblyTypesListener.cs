namespace Arity
{
    public interface IRegisterAssemblyTypesListener
    {
        void OnLoad(AssemblyModuleLoadArgs value);
    }
}
