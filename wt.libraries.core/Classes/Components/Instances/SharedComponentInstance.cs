using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WhileTrue.Classes.Components
{
    internal class SharedComponentInstance : ComponentInstance
    {
        private static readonly Dictionary<ComponentDescriptor, SharedInstanceWrapper> singletonInstances =
            new Dictionary<ComponentDescriptor, SharedInstanceWrapper>();

        internal SharedComponentInstance(ComponentDescriptor componentDescriptor)
            : base(componentDescriptor)
        {
        }

        private SharedInstanceWrapper InstanceReference
        {
            get
            {
                if (singletonInstances.ContainsKey(Descriptor))
                    return singletonInstances[Descriptor];
                return null;
            }
            set
            {
                if (value != null)
                    singletonInstances.Add(Descriptor, value);
                else
                    singletonInstances.Remove(Descriptor);
            }
        }

        internal override object CreateInstance(Type interfaceType, ComponentContainer componentContainer,
            Action<string> progressCallback)
        {
            if (InstanceReference == null)
                InstanceReference =
                    new SharedInstanceWrapper(DoCreateInstance(interfaceType, componentContainer, progressCallback));
            return InstanceReference.AddReference(componentContainer);
        }

        internal override void Dispose(ComponentContainer componentContainer)
        {
            if (InstanceReference != null &&
                InstanceReference.ReleaseReference(componentContainer))
            {
                (InstanceReference.Target as IDisposable)?.Dispose();
                InstanceReference = null;
                Debug.WriteLine($"Disposing shared component instance {Name}");
                base.Dispose(componentContainer);
            }
        }

        #region Nested type: SharedInstanceWrapper

        private class SharedInstanceWrapper
        {
            private readonly List<ComponentContainer> references = new List<ComponentContainer>();

            public SharedInstanceWrapper(object instance)
            {
                Target = instance;
            }

            public object Target { get; }

            public object AddReference(ComponentContainer componentContainer)
            {
                if (references.Contains(componentContainer) == false) references.Add(componentContainer);
                return Target;
            }

            public bool ReleaseReference(ComponentContainer componentContainer)
            {
                //Debug.Assert(this.references.Contains(componentContainer));
                //DbC.Assure(this.references.Contains(componentContainer)); -> there is a bug open!
                references.Remove(componentContainer);
                return references.Count == 0;
            }
        }

        #endregion
    }
}