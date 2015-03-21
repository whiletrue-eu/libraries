#if TEST
using System.Collections.Generic;
using Mz.Classes.TestUtilities;
using Mz.Classes.Components;
using Mz.Components.PluginFramework;
using mz.Facades.PluginFramework;
using Mz.Facades.SplashScreen;
using NUnit.Framework;

namespace Mz.Classes.PluginFramework
{
    [TestFixture]
	public class ApplicationLoaderTest
	{

        [Test]
        public void Load()
        {
            ComponentRepository Repository = new ComponentRepository();
            Repository.AddSingletonComponent("ApplicationLoader", typeof(ApplicationLoader));
            Repository.AddSingletonComponent("SplashScreen", typeof(SplashScreenMock));
            
            Repository.AddSimpleComponent("TestFacade1", typeof (Test1));
            Repository.AddSingletonComponent("TestFacade2", typeof (Test2));
            Repository.AddSimpleComponent("TestFacade3", typeof(Test3));


            using (ComponentContainer ComponentContainer = new ComponentContainer(Repository))
            {
                ComponentContainer.CreateComponentInstance<IApplicationLoader>().Run();


                Assert.IsTrue(((Test2) ComponentContainer.CreateComponentInstance<ITestFacade2>()).RunCalled);

                SplashScreenMock SplashScreen = (SplashScreenMock) ComponentContainer.CreateComponentInstance<ISplashScreen>();
                Assert.IsTrue(SplashScreen.ShowCalled);
                Assert.IsTrue(SplashScreen.HideCalled);

                Assert.AreEqual(10, SplashScreen.StatusTexts.Count);

                AutoIndex AutoIndex = new AutoIndex();
                Assert.AreEqual("Begin: TestFacade2", SplashScreen.StatusTexts[AutoIndex]);
                Assert.AreEqual("Begin: TestFacade1", SplashScreen.StatusTexts[AutoIndex]);
                Assert.AreEqual("End", SplashScreen.StatusTexts[AutoIndex]);
                Assert.AreEqual("End", SplashScreen.StatusTexts[AutoIndex]);
                Assert.AreEqual("Begin: TestFacade3", SplashScreen.StatusTexts[AutoIndex]);
                Assert.AreEqual("Begin: Test3Custom", SplashScreen.StatusTexts[AutoIndex]);
                Assert.AreEqual("Status: Test3Custom,1", SplashScreen.StatusTexts[AutoIndex]);
                Assert.AreEqual("Status: Test3Custom,2", SplashScreen.StatusTexts[AutoIndex]);
                Assert.AreEqual("End", SplashScreen.StatusTexts[AutoIndex]);
                Assert.AreEqual("End", SplashScreen.StatusTexts[AutoIndex]);
            }
        }
	}

    internal class SplashScreenMock : ISplashScreen 
    {

        private List<string> statusTexts = new List<string>();
        private bool showCalled;
        private bool hideCalled;


        public void Show()
        {
            this.showCalled = true;
        }

        public void Hide()
        {
            this.hideCalled = true;
        }

        public ISplashScreenLoadToken CreateLoadToken(string name)
        {
            return new LoadToken(this, name);
        }

        internal class LoadToken : ISplashScreenLoadToken
        {
            private readonly SplashScreenMock owner;
            private readonly string name;

            public LoadToken(SplashScreenMock owner, string name)
            {
                this.owner = owner;
                this.name = name;

                this.owner.statusTexts.Add(string.Format("Begin: {0}", this.name));
            }

            public string Status
            {
                set
                {
                    this.owner.statusTexts.Add(string.Format("Status: {0},{1}", this.name, value));
                }
            }

            public void Dispose()
            {
                this.owner.statusTexts.Add("End");
            }
        }

           

        public List<string> StatusTexts
        {
            get
            {
                return this.statusTexts;
            }
        }

        public bool ShowCalled
        {
            get { return showCalled; }
        }

        public bool HideCalled
        {
            get { return hideCalled; }
        }
    }
}
#endif