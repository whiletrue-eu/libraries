using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WhileTrue.Classes.Components
{
    internal class SharedComponentInstance : ComponentInstance
    {
        private static readonly Dictionary<ComponentDescriptor, SharedInstanceWrapper> singletonInstances = new Dictionary<ComponentDescriptor, SharedInstanceWrapper>();
        private readonly SemaphoreSlim instanceLock = new SemaphoreSlim(1, 1);
        internal SharedComponentInstance(ComponentDescriptor componentDescriptor)
            : base(componentDescriptor)
        {
        }

        private SharedInstanceWrapper InstanceReference
        {
            get
            {
                if (SharedComponentInstance.singletonInstances.ContainsKey(this.Descriptor))
                {
                    return SharedComponentInstance.singletonInstances[this.Descriptor];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null)
                {
                    SharedComponentInstance.singletonInstances.Add(this.Descriptor, value);
                }
                else
                {
                    SharedComponentInstance.singletonInstances.Remove(this.Descriptor);
                }
            }
        }

        internal override async Task<object> CreateInstanceAsync(Type interfaceType, ComponentContainer componentContainer, Action<string> progressCallback, ComponentDescriptor[] resolveStack)
        {
            await this.instanceLock.WaitAsync();
            try
            {
                if (this.InstanceReference == null)
                {
                    this.InstanceReference = new SharedInstanceWrapper(await this.DoCreateInstanceAsync(interfaceType, componentContainer, progressCallback, resolveStack));
                }

                return this.InstanceReference.AddReference(componentContainer);
            }
            finally
            {
                this.instanceLock.Release();
            }
        }


        internal override void Dispose(ComponentContainer componentContainer)
        {
            if (this.InstanceReference != null &&
                this.InstanceReference.ReleaseReference(componentContainer))
            {
                (this.InstanceReference.Target as IDisposable)?.Dispose();
                this.InstanceReference = null;
                Debug.WriteLine($"Disposing shared component instance {this.Name}");
                base.Dispose(componentContainer);
            }
        }

        #region Nested type: SharedInstanceWrapper

        private class SharedInstanceWrapper
        {
            private readonly List<ComponentContainer> references = new List<ComponentContainer>();

            public SharedInstanceWrapper(object instance)
            {
                this.Target = instance;
            }

            public object Target { get; }

            public object AddReference(ComponentContainer componentContainer)
            {
                if (this.references.Contains(componentContainer) == false)
                {
                    this.references.Add(componentContainer);
                }
                return this.Target;
            }

            public bool ReleaseReference(ComponentContainer componentContainer)
            {
                //Debug.Assert(this.references.Contains(componentContainer));
                //DbC.Assure(this.references.Contains(componentContainer)); -> there is a bug open!
                this.references.Remove(componentContainer);
                return this.references.Count == 0;
            }
        }

        #endregion
    }
}