using System.Threading.Tasks;

namespace WhileTrue.Classes.Components.TestComponents
{
    [Component]
    internal class Test2Tasks: ITestFacade2
    {
        public Test2Tasks(Task<ITestFacade1> testFacade1, Task<ITestFacade1[]> testFacade1Array)
        {
            this.TestFacade1 = testFacade1;
            this.TestFacade1Array = testFacade1Array;
        }

        public Task<ITestFacade1> TestFacade1 { get; }

        public Task<ITestFacade1[]> TestFacade1Array { get; }
    }
}