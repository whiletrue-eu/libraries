using System;
using System.Windows;
using JetBrains.Annotations;
using WhileTrue.Classes.Components;
using WhileTrue.Components.ApplicationLoading;
using WhileTrue.Facades.ApplicationLoader;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    ///     Eases the way to integrate a component repository / container into a WPF based application.
    ///     Just change the base class of the generated <c>App</c> class to this class, implement the abstract methods to add
    ///     components
    ///     Add one componentent that implements <see cref="IApplicationLoader" /> to bootstrap yur application. It will be
    ///     constructed and run automatically on application start
    /// </summary>
    [PublicAPI]
    public abstract class ComponentApplication : Application
    {
        private ComponentRepository componentRepository;

        /// <summary />
        protected ComponentApplication()
        {
            base.Startup += ComponentApplication_Startup;
            base.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            InitializeRepository();
        }

        private void InitializeRepository()
        {
            componentRepository = new ComponentRepository();

            componentRepository.AddComponent<ApplicationLoader>();
            AddComponents(componentRepository);
        }

        /// <summary>
        ///     Use this method to add your components or modules to the application
        /// </summary>
        protected abstract void AddComponents(ComponentRepository componentRepository);

        private void ComponentApplication_Startup(object sender, StartupEventArgs e)
        {
            Dispatcher.BeginInvoke(
                (Action) delegate
                {
                    using (var ComponentContainer = new ComponentContainer(componentRepository))
                    {
                        var Loader = ComponentContainer.ResolveInstance<IApplicationLoader>();
                        var Result = Loader.Run();
                        Shutdown(Result);
                    }
                }
            );
        }

        #region Redefine some properties as private to make sure that they are not set within Xaml

        // ReSharper disable UnusedMember.Local
        // ReSharper disable EventNeverSubscribedTo.Local
#pragma warning disable 67
        private new Uri StartupUri { get; set; }
        private new ShutdownMode ShutdownMode { get; set; }
        private new event EventHandler Startup;
        // ReSharper restore UnusedMember.Local
        // ReSharper restore EventNeverSubscribedTo.Local
#pragma warning restore 67

        #endregion
    }
}