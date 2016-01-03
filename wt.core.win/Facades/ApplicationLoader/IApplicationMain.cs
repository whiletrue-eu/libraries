using WhileTrue.Classes.Components;

namespace WhileTrue.Facades.ApplicationLoader
{
    /// <summary>
    /// Implements the main entry point method.
    /// </summary>
    [ComponentInterface]
    public interface IApplicationMain
    {
        /// <summary>
        /// Must launch the application. The method shall block until the application is shut down. The components will be disposed once this method retunrs
        /// </summary>
        int Run(ComponentContainer componentContainer);
    }
}