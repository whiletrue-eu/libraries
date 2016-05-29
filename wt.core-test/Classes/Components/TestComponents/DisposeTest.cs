
using System;

namespace WhileTrue.Classes.Components.TestComponents
{
    [Component]
    internal class DisposeTest : ITestFacade1, IDisposable
    {
        public bool Disposed { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            this.Disposed = true;
        }

        #endregion
    }
}