using System;

namespace WhileTrue.Classes.Components.TestComponents
{
    [Component]
    internal class DisposeCrashTest : ITestFacade1, IDisposable
    {
        public void Dispose()
        {
            throw new Exception("Crash - to be ignored");
        }
    }
}