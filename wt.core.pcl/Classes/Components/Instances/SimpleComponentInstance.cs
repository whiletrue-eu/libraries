using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WhileTrue.Classes.Components
{
    internal class SimpleComponentInstance : ComponentInstance
    {
        private object instance;
        private readonly SemaphoreSlim instanceLock = new SemaphoreSlim(1, 1);

        internal SimpleComponentInstance(ComponentDescriptor componentDescriptor)
            : base(componentDescriptor)
        {
        }

        internal override async Task<object> CreateInstanceAsync(Type interfaceType, ComponentContainer componentContainer, Action<string> progressCallback,ComponentDescriptor[] resolveStack)
        {
            await this.instanceLock.WaitAsync();
            try
            {
                return this.instance ?? (this.instance = await this.DoCreateInstanceAsync(interfaceType, componentContainer, progressCallback, resolveStack));
            }
            finally
            {
                this.instanceLock.Release();
            }
        }

        internal override void Dispose(ComponentContainer componentContainer)
        {
            (this.instance as IDisposable)?.Dispose();
            this.instance = null;
            Debug.WriteLine($"Disposing simple component instance {this.Name}");
            base.Dispose(componentContainer);
        }
    }
}