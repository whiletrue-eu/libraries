
namespace WhileTrue.Classes.Components.TestComponents
{
    [Component]
    internal class Test2A : ITestFacade2
    {
        public Test2A(ITestFacade1 testFacade1)
        {
            this.TestFacade1 = testFacade1;
        }

        public Test2A()
        {
        }

        internal ITestFacade1 TestFacade1 { get; }
    }
}