using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Components
{
    internal class SharedComponentInstance : ComponentInstance
    {
        private static readonly Dictionary<ComponentDescriptor, SharedInstanceWrapper> singletonInstances = new Dictionary<ComponentDescriptor, SharedInstanceWrapper>();
        private static readonly SemaphoreSlim instanceLock = new SemaphoreSlim(1, 1);
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
            //We need to seperate locking on the instance reference collection (new wrapper) and setting/add-ref-ing because otherwise we can dead-lock
            await SharedComponentInstance.instanceLock.WaitAsync();
            bool MustCreate = false;
            try
            {
                try
                {
                    if (this.InstanceReference == null)
                    {
                        this.InstanceReference = new SharedInstanceWrapper();
                        MustCreate = true;
                    }

                    await this.InstanceReference.Lock.WaitAsync();
                }
                finally
                {
                    SharedComponentInstance.instanceLock.Release();
                }

                if (MustCreate)
                {
                    this.InstanceReference.SetInstance(await this.DoCreateInstanceAsync(interfaceType, componentContainer, progressCallback, resolveStack));
                }

                return this.InstanceReference.AddReference(componentContainer);
            }
            finally
            {
                this.InstanceReference?.Lock.Release();
            }
        }
        internal override object CreateInstance(Type interfaceType, ComponentContainer componentContainer, Action<string> progressCallback, ComponentDescriptor[] resolveStack)
        {
            //We need to seperate locking on the instance reference collection (new wrapper) and setting/add-ref-ing because otherwise we can dead-lock
            SharedComponentInstance.instanceLock.Wait();
            bool MustCreate = false;
            try
            {
                try
                {
                    if (this.InstanceReference == null)
                    {
                        this.InstanceReference = new SharedInstanceWrapper();
                        MustCreate = true;
                    }

                    this.InstanceReference.Lock.Wait();
                }
                finally
                {
                    SharedComponentInstance.instanceLock.Release();
                }

                if (MustCreate)
                {
                    this.InstanceReference.SetInstance(this.DoCreateInstance(interfaceType, componentContainer, progressCallback, resolveStack));
                }

                return this.InstanceReference.AddReference(componentContainer);
            }
            finally
            {
                this.InstanceReference?.Lock.Release();
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
            public readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

            public void SetInstance(object instance)
            {
                this.Target.DbC_AssureNull();
                this.Target = instance;
            }

            public object Target { get; private set; }

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