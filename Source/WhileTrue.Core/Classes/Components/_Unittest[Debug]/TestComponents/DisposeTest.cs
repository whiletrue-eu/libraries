
using System;
using WhileTrue.Classes.CodeInspection;

namespace WhileTrue.Classes.Components._Unittest.TestComponents
{
    [Component]
    internal class DisposeTest : ITestFacade1, IDisposable
    {
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