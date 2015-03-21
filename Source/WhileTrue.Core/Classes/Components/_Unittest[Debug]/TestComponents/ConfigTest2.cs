using WhileTrue.Classes.CodeInspection;

namespace WhileTrue.Classes.Components._Unittest.TestComponents
{
    [Component]
    internal class ConfigTest2 : ITestFacade2
    {
        private readonly Config config;
        private readonly ITestFacade1 testFacade1;

        public ConfigTest2(ITestFacade1 testFacade1, Config config)
        {
            this.testFacade1 = testFacade1;
            this.config = config;
        }


        internal ITestFacade1 TestFacade1
        {
            get { return this.testFacade1; }
        }

        public Config Config
        {
            get { return this.config; }
        }
    }
}