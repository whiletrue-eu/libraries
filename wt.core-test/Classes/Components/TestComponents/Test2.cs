
namespace WhileTrue.Classes.Components.TestComponents
{
    [Component]
    internal class Test2 : ITestFacade2
    {
        public Test2(ITestFacade1 testFacade1)
        {
            this.TestFacade1 = testFacade1;
        }

        internal ITestFacade1 TestFacade1 { get; }
    }
}