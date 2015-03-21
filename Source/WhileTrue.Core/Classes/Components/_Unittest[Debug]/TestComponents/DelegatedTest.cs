namespace WhileTrue.Classes.Components._Unittest.TestComponents
{
    [Component]
    internal class DelegatedTest
    {
        [ComponentBindingProperty]
        public ITestFacade1 Delegated
        {
            get
            {
                return new Test1();
            }
        }
    }
}