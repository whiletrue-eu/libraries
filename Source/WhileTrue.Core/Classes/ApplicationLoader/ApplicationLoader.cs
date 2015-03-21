using Mz.Classes.Components;
using Mz.Facades.PluginFramework;
using Mz.Facades.SplashScreen;

namespace Mz.Classes.ApplicationLoader
{
    public class ApplicationLoader
    {
        private ISplashScreenLoadToken currentLoadToken;
        private ISplashScreen splashScreen;

        public int Run()
        {
            using (ComponentContainer ComponentContainer = new ComponentContainer(this.componentRepository))
            {
                this.splashScreen = ComponentContainer.CreateComponentInstance<ISplashScreen>();
                ComponentContainer.InstanceCreating += this.ComponentContainer_InstanceCreating;
                ComponentContainer.InstanceCreated += this.ComponentContainer_InstanceCreated;

                this.splashScreen.Show();

                try
                {
                    //Create Plugins
                    ComponentContainer.CreateComponentInstances<IPlugin>();
                }
                finally
                {
                    this.splashScreen.Hide();
                }

                //Get Main Module
                IMainModule MainModule = ComponentContainer.CreateComponentInstance<IMainModule>();
                return MainModule.Run();
            }
        }

        private void ComponentContainer_InstanceCreated(object sender, ComponentInstanceEventArgs e)
        {
            this.currentLoadToken.Dispose();
        }

        private void ComponentContainer_InstanceCreating(object sender, ComponentInstanceEventArgs e)
        {
            this.currentLoadToken = this.splashScreen.CreateLoadToken(e.ComponentInstance.Name);
        }
    }
}