
using WhileTrue.Classes.CodeInspection;

namespace WhileTrue.Classes.Components._Unittest.TestComponents
{
    [Component]
    internal class Test2Lazy : ITestFacade2
    {
        [ComponentBindingProperty]
        public ITestFacade1 TestFacade1 { set; internal get; }
    }
}