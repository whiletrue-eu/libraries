using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using WhileTrue.Classes.Components;
using WhileTrue.Facades.ApplicationLoader;
using WhileTrue.Facades.SplashScreen;

namespace WhileTrue.Components.ApplicationLoading
{
    /// <summary>
    ///     Implements a component loader based on a component container and the <see cref="IModule" /> and
    ///     <see cref="IApplicationMain" /> ( <see cref="IMainModule" />
    ///     interface implementations
    /// </summary>
    [Component("Application Loader")]
    public class ApplicationLoader : IApplicationLoader
    {
        private readonly ComponentRepository componentRepository;

        /// <summary />
        public ApplicationLoader(ComponentRepository componentRepository)
        {
            this.componentRepository = componentRepository;
        }

        #region IApplicationLoader Members

        /// <summary>
        ///     Sets up and runs the application. The run method shall block until the application is shut down
        /// </summary>
        public int Run()
        {
            using (var ComponentContainer = new ComponentContainer(componentRepository))
            {
                var SplashScreen = ComponentContainer.TryResolveInstance<ISplashScreen>() ?? new SplashDummy();

                SplashScreen.Show();
                IApplicationMain ApplicationMain;

                try
                {
                    ResolveModules(ComponentContainer);
                    ApplicationMain =
                        ComponentContainer.ResolveInstance<IApplicationMain>(status => SplashScreen.SetStatus(status));
                }
                finally
                {
                    SplashScreen.Hide();
                }

                //Get Main Module
                return ApplicationMain.Run(ComponentContainer);
            }
        }

        /// <summary>
        ///     resolves all modules,i.e. callas all modules to include subcomponents into the compontent container
        /// </summary>
        public void ResolveModules(ComponentContainer componentContainer)
        {
            // Add components through modules. To support recursive modules, 
            // do this until no new modules are inserted
            var AlreadyResolvedModules = new List<IModule>();
            var Modules = componentContainer.ResolveInstances<IModule>();
            do
            {
                foreach (var Plugin in Modules) Plugin.AddSubcomponents(componentContainer.Repository);

                AlreadyResolvedModules.AddRange(Modules);
                Modules = (from Module in componentContainer.ResolveInstances<IModule>()
                    where AlreadyResolvedModules.Contains(Module) == false
                    select Module).ToArray();
            } while (Modules.Length > 0);

            // Call initialize
            foreach (var Plugin in AlreadyResolvedModules) Plugin.Initialize(componentContainer);
        }

        [ExcludeFromCodeCoverage]
        private class SplashDummy : ISplashScreen
        {
            public void Show()
            {
            }

            public void Hide()
            {
            }

            public void SetStatus(string name)
            {
            }
        }

        #endregion
    }
}