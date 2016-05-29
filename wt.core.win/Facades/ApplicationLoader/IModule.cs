using WhileTrue.Classes.Components;

namespace WhileTrue.Facades.ApplicationLoader
{
    /// <summary>
    /// Implements a module to wrap a collection of components for the <see cref="IApplicationLoader"/>
    /// </summary>
    [ComponentInterface]
    public interface IModule
    {
        /// <summary>
        /// Is called before any component is instantiated. Must be used to add the contained components into the repository
        /// </summary>
        void AddSubcomponents(ComponentRepository componentRepository);
        /// <summary>
        /// Can be used to initialize components in the component container which is used to launch the application
        /// </summary>
        void Initialize(ComponentContainer componentContainer);
    }
}