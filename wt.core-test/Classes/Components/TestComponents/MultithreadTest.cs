using System.Threading.Tasks;

namespace WhileTrue.Classes.Components.TestComponents
{
    [Component]
    internal class MultithreadTest: ITestFacade1
    {
        public ITestFacade2 Dependency1 { get; }
        public ITestFacade2 Dependency2 { get; }
        public ITestFacade2[] Dependency3 { get; }
        public Task<ITestFacade2> Dependency4 { get; }
        public Task<ITestFacade2[]> Dependency5 { get; }

        public MultithreadTest(ITestFacade2 dependency1, ITestFacade2 dependency2, ITestFacade2[] dependency3, Task<ITestFacade2> dependency4, Task<ITestFacade2[]> dependency5)
        {
            this.Dependency1 = dependency1;
            this.Dependency2 = dependency2;
            this.Dependency3 = dependency3;
            this.Dependency4 = dependency4;
            this.Dependency5 = dependency5;
        }
    }
}