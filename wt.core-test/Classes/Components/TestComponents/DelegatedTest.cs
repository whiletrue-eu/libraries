namespace WhileTrue.Classes.Components.TestComponents
{
    [Component]
    internal class DelegatedTest
    {
        [ComponentBindingProperty]
        public ITestFacade1 Delegated => new Test1();
    }
}