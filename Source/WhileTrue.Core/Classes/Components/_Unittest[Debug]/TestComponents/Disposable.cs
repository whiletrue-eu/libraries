using System;

namespace WhileTrue.Classes.Components._Unittest.TestComponents
{
    [Component]
    internal class Disposable : ITestFacade1,IDisposable
    {
        private bool isDisposed;

        public bool IsDisposed
        {
            get {
                return this.isDisposed;
            }
        }

        public void Dispose()
        {
            this.isDisposed = true;
        }
    }
}