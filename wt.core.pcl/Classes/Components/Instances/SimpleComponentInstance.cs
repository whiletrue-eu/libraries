using System;

namespace WhileTrue.Classes.Components
{
    internal class SimpleComponentInstance : ComponentInstance
    {
        private object instance;

        internal SimpleComponentInstance(ComponentDescriptor componentDescriptor)
            : base(componentDescriptor)
        {
        }

        internal override object CreateInstance(Type interfaceType, ComponentContainer componentContainer, Action<string> progressCallback)
        {
            return this.instance ?? (this.instance = this.DoCreateInstance(interfaceType, componentContainer, progressCallback));
        }

        internal override void Dispose(ComponentContainer componentContainer)
        {
            (this.instance as IDisposable)?.Dispose();
            this.instance = null;
            base.Dispose(componentContainer);
        }
    }
}