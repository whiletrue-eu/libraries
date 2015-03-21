
using WhileTrue.Classes.CodeInspection;

namespace WhileTrue.Classes.Components._Unittest.TestComponents
{
    [Component]
    internal class Test2 : ITestFacade2
    {
        private readonly ITestFacade1 testFacade1;

        public Test2(ITestFacade1 testFacade1)
        {
            this.testFacade1 = testFacade1;
        }

        [ComponentBindingProperty]
        internal ITestFacade1 TestFacade1
        {
            get { return this.testFacade1; }
        }
    }
}