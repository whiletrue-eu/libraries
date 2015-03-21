namespace WhileTrue.Classes.Components._Unittest.TestComponents
{
    [Component]
    internal class RepositoryParameterTest1 : ITestFacade1
    {
        private readonly ComponentRepository repository;
        private readonly ComponentContainer container;

        public RepositoryParameterTest1(ComponentRepository repository, ComponentContainer container)
        {
            this.repository = repository;
            this.container = container;
        }


        public ComponentRepository Repository
        {
            get { return this.repository; }
        }

        public ComponentContainer Container
        {
            get
            {
                return container;
            }
        }
    }
}