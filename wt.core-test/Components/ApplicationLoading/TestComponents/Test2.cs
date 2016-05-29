using System;
using WhileTrue.Classes.Components;
using WhileTrue.Facades.ApplicationLoader;

namespace WhileTrue.Components.ApplicationLoading.TestComponents
{
    [Component]
    class Test2 : ITestFacade2, IApplicationMain, IDisposable 
    {
        private static bool runCalled;

        public Test2(ITestFacade1 testFacade1)
        {
            this.TestFacade1 = testFacade1;
        }

        public static void Reset()
        {
            Test2.runCalled = false;
        }

        internal ITestFacade1 TestFacade1 { get; }

        public static bool RunCalled => Test2.runCalled;

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
            Test2.runCalled = true;
            return 0;
        }
    }
}