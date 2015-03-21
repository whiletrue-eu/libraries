using System;
using System.Windows;
using WhileTrue.Classes.Components;
using WhileTrue.Components.ApplicationLoading;
using WhileTrue.Facades.ApplicationLoader;

namespace WhileTrue.Classes.Wpf
{
    public abstract class ComponentApplication : Application
    {

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

        private ComponentRepository componentRepository;

        protected ComponentApplication()
        {
            base.Startup += ComponentApplication_Startup;
            base.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;

            this.InitializeRepository();
        }

        private void InitializeRepository()
        {
            this.componentRepository = new ComponentRepository();

            this.componentRepository.AddComponent<ApplicationLoader>();
            this.AddComponents(componentRepository);
        }

        protected abstract void AddComponents(ComponentRepository componentRepository);

        void ComponentApplication_Startup(object sender, StartupEventArgs e)
        {
            this.Dispatcher.BeginInvoke(
                (Action)(delegate
                              {
                                  using (ComponentContainer ComponentContainer = new ComponentContainer(this.componentRepository))
                                  {
                                      IApplicationLoader Loader = ComponentContainer.ResolveInstance<IApplicationLoader>();
                                      int Result = Loader.Run();
                                      this.Shutdown(Result);
                                  }
                              })
                );
        }
    }
}
