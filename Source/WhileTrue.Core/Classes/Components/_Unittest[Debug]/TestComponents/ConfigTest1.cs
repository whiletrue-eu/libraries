using WhileTrue.Classes.CodeInspection;

namespace WhileTrue.Classes.Components._Unittest.TestComponents
{
    [Component]
    internal class ConfigTest1 : ITestFacade1
    {
        private readonly Config config;

        public ConfigTest1(Config config)
        {
            this.config = config;
        }

        public Config Config
        {
            get { return this.config; }
        }
    }
}