using WhileTrue.Classes.Components;

namespace WhileTrue.Facades.ApplicationLoader
{
    /// <summary>
    /// Implements the main module. Only one module shall implement this interface
    /// </summary>
    [ComponentInterface]
    public interface IMainModule : IModule, IApplicationMain
    {
    }
}