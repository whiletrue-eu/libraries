namespace WhileTrue.Classes.Components.TestComponents
{
    [Component]
    internal class NoSuitableConstructor : ITestFacade1
    {
        public NoSuitableConstructor(string unsupportedType)
        {
            
        }
        public NoSuitableConstructor(IUnsupportedInterface iface)
        {
        }

        internal interface IUnsupportedInterface
        {
        }
    }
}