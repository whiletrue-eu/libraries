namespace WhileTrue.Classes.Components.TestComponents
{
    [Component]
    internal class Test2B : ITestFacade2
    {
        public Test2B(ITestFacade1[] test1)
        {
            this.Test1 = test1;
        }

        public ITestFacade1[] Test1 { get; }
    }
}