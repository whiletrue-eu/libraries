namespace WhileTrue.Classes.Components.TestComponents
{
    [Component]
    internal class RepositoryParameterTest1 : ITestFacade1
    {
        public RepositoryParameterTest1(ComponentRepository repository, ComponentContainer container)
        {
            this.Repository = repository;
            this.Container = container;
        }


        public ComponentRepository Repository { get; }

        public ComponentContainer Container { get; }
    }
}