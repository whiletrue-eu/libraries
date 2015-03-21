#if TEST

namespace Mz.Classes.PluginFramework
{
	class Test2 : ITestFacade2, IPlugin, IMainModule, IDisposable 
	{
        private readonly ITestFacade1 testFacade1;
	    private bool runCalled;

	    public Test2(ITestFacade1 testFacade1)
        {
            this.testFacade1 = testFacade1;
        }


        internal ITestFacade1 TestFacade1
        {
            get { return testFacade1; }
        }

	    public int Run()
	    {
	        this.runCalled = true;
	        return 0;
	    }


	    public bool RunCalled
	    {
	        get { return runCalled; }
	    }

	    #region IDisposable Members

	    public void Dispose()
	    {
	        throw new NotImplementedException();
	    }

	    #endregion
	}
}
#endif