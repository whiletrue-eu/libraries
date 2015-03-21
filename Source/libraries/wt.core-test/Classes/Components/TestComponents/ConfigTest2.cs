namespace WhileTrue.Classes.Components.TestComponents
{
    [Component]
    internal class ConfigTest2 : ITestFacade2
    {
        public ConfigTest2(ITestFacade1 testFacade1, Config config)
        {
            this.TestFacade1 = testFacade1;
            this.Config = config;
        }


        internal ITestFacade1 TestFacade1 { get; }

        public Config Config { get; }
    }
}