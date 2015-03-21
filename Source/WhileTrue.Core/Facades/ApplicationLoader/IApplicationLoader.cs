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
        /// Sets up and runs the application. The run method shall block until the application is shut down
        /// </summary>
        int Run();
    }
}