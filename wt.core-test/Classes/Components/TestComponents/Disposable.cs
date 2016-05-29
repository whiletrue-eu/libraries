using System;

namespace WhileTrue.Classes.Components.TestComponents
{
    [Component]
    internal class Disposable : ITestFacade1,IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.IsDisposed = true;
        }
    }
}