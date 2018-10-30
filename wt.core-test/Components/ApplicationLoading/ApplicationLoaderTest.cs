// ReSharper disable InconsistentNaming

using NUnit.Framework;
using WhileTrue.Classes.Components;
using WhileTrue.Components.ApplicationLoading.TestComponents;
using WhileTrue.Facades.ApplicationLoader;
using WhileTrue.Facades.SplashScreen;

namespace WhileTrue.Components.ApplicationLoading
{
    [TestFixture]
    public class ApplicationLoaderTest
    {

        [Test]
        public void application_loader_shall_instanciate_all_modules_and_notify_the_splash_progress_accordingly()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddComponent<ApplicationLoader>();
            Repository.AddComponent<SplashScreenMock>();
            Repository.AddComponent<Test1>();
            Repository.AddComponent<Test2>();
            Repository.AddComponent<Test3>();


            using (ComponentContainer ComponentContainer = new ComponentContainer(Repository))
            {

                SplashScreenMock SplashScreen = (SplashScreenMock)ComponentContainer.ResolveInstance<ISplashScreen>();//resolve before because otherwise it is removed after application run and below that a new instance would be created

                ComponentContainer.ResolveInstance<IApplicationLoader>().Run();

                Assert.IsTrue(Test2.RunCalled);

                Assert.IsTrue(SplashScreen.ShowCalled);
                Assert.IsTrue(SplashScreen.HideCalled);

                Assert.That(SplashScreen.StatusTexts, Is.EquivalentTo(new[]{ "Status: Test1" , "Status: Test2" }));
            }
        }
    }
}