using System;
using System.Collections.Generic;

namespace WhileTrue.Classes.Components
{
    internal class SharedComponentInstance : ComponentInstance
    {
        private static readonly Dictionary<ComponentDescriptor, SharedInstanceWrapper> singletonInstances = new Dictionary<ComponentDescriptor, SharedInstanceWrapper>();

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

        internal override object CreateInstance(Type interfaceType, ComponentContainer componentContainer, Action<string> progressCallback)
        {
            if (this.InstanceReference == null)
            {
                this.InstanceReference = new SharedInstanceWrapper(this.DoCreateInstance(interfaceType, componentContainer, progressCallback));
            }
            return this.InstanceReference.AddReference(componentContainer);
        }

        internal override void Dispose(ComponentContainer componentContainer)
        {
            if (this.InstanceReference != null &&
                this.InstanceReference.ReleaseReference(componentContainer))
            {
                (this.InstanceReference.Target as IDisposable)?.Dispose();
                this.InstanceReference = null;
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