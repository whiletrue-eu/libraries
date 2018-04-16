using WhileTrue.Classes.Components;

namespace WhileTrue.Facades.ApplicationLoader
{
    /// <summary>
    /// Implements the application loader
    /// </summary>
    [ComponentInterface]
    public interface IApplicationLoader
    {
        /// <summary>
        /// Resolves the modules, sets up and runs the application. The run method shall block until the application is shut down
        /// </summary>
        int Run();

        /// <summary>
        /// Resolves the modules but does not run the application. Useful when running the application cannot be done
        /// by a interface like <see cref="IApplicationMain"/> but must be custom.
        /// </summary>
        void ResolveModules(ComponentContainer container);
    }
}