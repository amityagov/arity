namespace Arity.Tests.Modules
{
    [Module(nameof(ModuleWithOptions))]
    public class ModuleWithOptions
    {
        public static IModuleOptions<OptionsClass> Options;
        public static bool Called;

        public class OptionsClass
        {
        }

        public ModuleWithOptions(IModuleOptions<OptionsClass> options)
        {
            Options = options;
            Called = true;
        }
    }
}