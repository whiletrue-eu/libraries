namespace WhileTrue.Classes.Components.TestComponents
{
    [Component]
    internal class ConfigTest1 : ITestFacade1
    {
        public ConfigTest1(Config config)
        {
            this.Config = config;
        }

        public Config Config { get; }
    }
}