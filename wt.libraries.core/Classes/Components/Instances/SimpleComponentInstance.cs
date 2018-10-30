using System;
using System.Diagnostics;

namespace WhileTrue.Classes.Components
{
    internal class SimpleComponentInstance : ComponentInstance
    {
        private object instance;

        internal SimpleComponentInstance(ComponentDescriptor componentDescriptor)
            : base(componentDescriptor)
        {
        }

        internal override object CreateInstance(Type interfaceType, ComponentContainer componentContainer,
            Action<string> progressCallback)
        {
            return instance ?? (instance = DoCreateInstance(interfaceType, componentContainer, progressCallback));
        }

        internal override void Dispose(ComponentContainer componentContainer)
        {
            (instance as IDisposable)?.Dispose();
            instance = null;
            Debug.WriteLine($"Disposing simple component instance {Name}");
            base.Dispose(componentContainer);
        }
    }
}