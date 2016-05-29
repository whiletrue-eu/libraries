using System;

namespace WhileTrue.Classes.Components.TestComponents
{
    [Component]
    internal class DisposeWithDependencyTest : ITestFacade2, IDisposable
    {
        public DisposeWithDependencyTest(ITestFacade1 dependency)
        {
        }

        public bool Disposed { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            this.Disposed = true;
        }

        #endregion
    }
}