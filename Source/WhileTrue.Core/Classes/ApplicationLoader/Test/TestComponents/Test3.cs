#if TEST

namespace Mz.Classes.PluginFramework
{
	class Test3 : IPlugin
	{
	    public Test3( ISplashScreen splashScreen )
	    {
            using (ISplashScreenLoadToken LoadToken = splashScreen.CreateLoadToken("Test3Custom"))
            {
                LoadToken.Status = "1";
                LoadToken.Status = "2";
            }
	    }
	}
}
#endif