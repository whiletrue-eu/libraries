using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Components
{
    internal class SingletonComponentInstance : ComponentInstance
    {
        private static readonly Dictionary<Type, SingletonInstanceWrapper> singletonInstances = new Dictionary<Type, SingletonInstanceWrapper>();
        private static readonly SemaphoreSlim instanceLock = new SemaphoreSlim(1, 1);

        internal SingletonComponentInstance(ComponentDescriptor componentDescriptor)
            : base(componentDescriptor)
        {
        }

        private SingletonInstanceWrapper InstanceReference
        {
            get
            {
                if (SingletonComponentInstance.singletonInstances.ContainsKey(this.Descriptor.Type))
                {
                    return SingletonComponentInstance.singletonInstances[this.Descriptor.Type];
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
                    SingletonComponentInstance.singletonInstances.Add(this.Descriptor.Type, value);
                }
                else
                {
                    SingletonComponentInstance.singletonInstances.Remove(this.Descriptor.Type);
                }
            }
        }

        internal override async Task<object> CreateInstanceAsync(Type interfaceType, ComponentContainer componentContainer, Action<string> progressCallback, ComponentDescriptor[] resolveStack)
        {
            //We need to seperate locking on the instance reference collection (new wrapper) and setting/add-ref-ing because otherwise we can dead-lock
            await SingletonComponentInstance.instanceLock.WaitAsync();
            bool MustCreate = false;
            try
            {
                try
                {
                    if (this.InstanceReference == null)
                    {
                        this.InstanceReference = new SingletonInstanceWrapper();
                        MustCreate = true;
                    }

                    await this.InstanceReference.Lock.WaitAsync();
                }
                finally
                {
                    SingletonComponentInstance.instanceLock.Release();
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
            SingletonComponentInstance.instanceLock.Wait();
            bool MustCreate = false;
            try
            {
                try
                {
                    if (this.InstanceReference == null)
                    {
                        this.InstanceReference = new SingletonInstanceWrapper();
                        MustCreate = true;
                    }

                    this.InstanceReference.Lock.Wait();
                }
                finally
                {
                    SingletonComponentInstance.instanceLock.Release();
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
                Debug.WriteLine($"Disposing singleton component instance {this.Name}");
                base.Dispose(componentContainer);
            }
        }

        #region Nested type: SharedInstanceWrapper

        private class SingletonInstanceWrapper
        {
            private readonly List<ComponentContainer> references = new List<ComponentContainer>();
            public readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

            public void SetInstance(object instance)
            {
                this.Target.DbC_AssureNull();
                this.Target = instance;
            }

            public object Target { get; private set; }

            [UsedImplicitly]
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
                DbC.Assure(this.references.Contains(componentContainer));
                this.references.Remove(componentContainer);
                return this.references.Count == 0;
            }
        }

        #endregion
    }
}