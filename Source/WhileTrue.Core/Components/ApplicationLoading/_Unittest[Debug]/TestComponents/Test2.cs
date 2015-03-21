
using System;
using WhileTrue.Classes.Components;
using WhileTrue.Facades.ApplicationLoader;

namespace WhileTrue.Components.ApplicationLoading._Unittest.TestComponents
{
    [Component]
    class Test2 : ITestFacade2, IApplicationMain, IDisposable 
    {
        private readonly ITestFacade1 testFacade1;
        private static bool runCalled;

        public Test2(ITestFacade1 testFacade1)
        {
            this.testFacade1 = testFacade1;
        }

        public static void Reset()
        {
            runCalled = false;
        }

        internal ITestFacade1 TestFacade1
        {
            get { return testFacade1; }
        }

        public static bool RunCalled
        {
            get { return runCalled; }
        }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        public void AddSubcomponents(ComponentRepository componentRepository)
        {
        }

        public void Initialize(ComponentContainer componentContainer)
        {
        }

        public int Run(ComponentContainer componentContainer)
        {
            runCalled = true;
            return 0;
        }
    }
}