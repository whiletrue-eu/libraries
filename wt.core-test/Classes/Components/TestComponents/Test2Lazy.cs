
using System;

namespace WhileTrue.Classes.Components.TestComponents
{
    [Component(ThreadAffinity = ThreadAffinity.NeedsUiThread)]
    internal class Test2Lazy : ITestFacade2
    {
        private readonly Func<ITestFacade1> testFacade1;
        private readonly Func<ITestFacade1[]> testFacade1Array;

        public Test2Lazy(Func<ITestFacade1> testFacade1, Func<ITestFacade1[]> testFacade1Array)
        {
            this.testFacade1 = testFacade1;
            this.testFacade1Array = testFacade1Array;
        }

        public ITestFacade1 TestFacade1 => this.testFacade1();
        public ITestFacade1[] TestFacade1Array => this.testFacade1Array();
    }
}