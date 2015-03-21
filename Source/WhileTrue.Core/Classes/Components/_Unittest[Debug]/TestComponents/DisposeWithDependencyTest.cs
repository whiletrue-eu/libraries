using System;

namespace WhileTrue.Classes.Components._Unittest.TestComponents
{
    [Component]
    internal class DisposeWithDependencyTest : ITestFacade2, IDisposable
    {
        public DisposeWithDependencyTest(ITestFacade1 dependency)
        {
        }

        private bool disposed;

        public bool Disposed
        {
            get { return this.disposed; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.disposed = true;
        }

        #endregion
    }
}