using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace WhileTrue.Classes.Components
{
    internal class SimpleComponentInstance : ComponentInstance
    {
        private object instance;

        internal SimpleComponentInstance(ComponentDescriptor componentDescriptor)
            : base(componentDescriptor)
        {
        }

        internal override async Task<object> CreateInstanceAsync(Type interfaceType, ComponentContainer componentContainer, Action<string> progressCallback,ComponentDescriptor[] resolveStack)
        {
            using (await new AsyncMonitor().EnterAsync())
            {
                return this.instance ?? (this.instance = await this.DoCreateInstanceAsync(interfaceType, componentContainer, progressCallback, resolveStack));
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