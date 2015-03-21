namespace WhileTrue.Classes.Components._Unittest.TestComponents
{
    [Component]
    internal class Test2b : ITestFacade2
    {
        private readonly ITestFacade1[] test1;

        public Test2b(ITestFacade1[] test1)
        {
            this.test1 = test1;
        }

        public ITestFacade1[] Test1
        {
            get
            {
                return this.test1;
            }
        }
    }
}